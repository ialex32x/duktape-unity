using System;
using System.Reflection;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using duk_ret_t = System.Int32;

    public struct UnrefAction
    {
        public uint refid;
        public Action<IntPtr, uint> action;
    }

    public class DuktapeVM // : Scripting.ScriptEngine
    {
        // duktape-unity 版本, 生成规则发生无法兼容的改变时增加版本号
        public const int VERSION = 0x10001;
        public const string HEAP_STASH_PROPS_REGISTRY = "registry";
        // 在jsobject实例上记录关联的本地对象 object cache refid
        public static readonly string OBJ_PROP_NATIVE = DuktapeDLL.DUK_HIDDEN_SYMBOL("native-cache-id");
        public static readonly string OBJ_PROP_TYPE = DuktapeDLL.DUK_HIDDEN_SYMBOL("type-refid");
        // 导出类的js构造函数隐藏属性, 记录在vm中的注册id
        public static readonly string OBJ_PROP_EXPORTED_REFID = DuktapeDLL.DUK_HIDDEN_SYMBOL("exported-registry-refid");
        // public static readonly string OBJ_PROP_SPECIAL_REFID = DuktapeDLL.DUK_HIDDEN_SYMBOL("special-refid");

        public const string _DuktapeDelegates = "_DuktapeDelegates";

        public const string SPECIAL_ENUM = "Enum";
        public const string SPECIAL_DELEGATE = "Delegate";
        public const string SPECIAL_CSHARP = "CSharp";

        private DuktapeContext _ctx;
        private IFileSystem _fileManager;
        private ObjectCache _objectCache = new ObjectCache();

        private List<string> _searchPaths = new List<string>();

        private Queue<UnrefAction> _unrefActions = new Queue<UnrefAction>();

        // 已经导出的本地类
        private Dictionary<Type, DuktapeFunction> _exported = new Dictionary<Type, DuktapeFunction>();
        private Dictionary<uint, Type> _exportedTypes = new Dictionary<uint, Type>(); // 从 refid 反查 Type

        private Dictionary<string, DuktapeFunction> _specialTypes = new Dictionary<string, DuktapeFunction>(); // 从 refid 反查 Type

        private Dictionary<Type, MethodInfo> _delegates = new Dictionary<Type, MethodInfo>(); // 委托对应的 duktape 绑定函数

        public DuktapeContext context
        {
            get { return _ctx; }
        }

        public DuktapeVM()
        {
        }

        public static DuktapeVM GetVM(IntPtr ctx)
        {
            return DuktapeContext.GetVM(ctx);
        }

        public static ObjectCache GetObjectCache(IntPtr ctx)
        {
            return DuktapeContext.GetVM(ctx)._objectCache;
        }

        public void AddSearchPath(string path)
        {
            if (!_searchPaths.Contains(path))
            {
                _searchPaths.Add(path);
            }
        }

        public DuktapeFunction GetSpecial(string name)
        {
            DuktapeFunction val;
            if (_specialTypes.TryGetValue(name, out val))
            {
                return val;
            }
            return null;
        }

        public uint AddSpecial(string name, DuktapeFunction val)
        {
            // Debug.LogFormat("Add Special {0} {1}", name, val.rawValue);
            var refid = val.rawValue;
            _specialTypes[name] = val;
            return refid;
        }

        public void AddDelegate(Type type, MethodInfo method)
        {
            _delegates[type] = method;
            // Debug.LogFormat("Add Delegate {0} {1}", type, method);
        }

        public Delegate CreateDelegate(Type type, DuktapeDelegate fn)
        {
            MethodInfo method;
            if (_delegates.TryGetValue(type, out method))
            {
                var target = Delegate.CreateDelegate(type, fn, method, true);
                fn.target = target;
                return target;
            }
            return null;
        }

        public uint AddExported(Type type, DuktapeFunction fn)
        {
            var refid = fn.rawValue;
            _exported.Add(type, fn);
            _exportedTypes[refid] = type;
            return refid;
        }

        public Type GetExportedType(uint refid)
        {
            Type type;
            return _exportedTypes.TryGetValue(refid, out type) ? type : null;
        }

        // 得到注册在js中的类型对应的构造函数
        public DuktapeFunction GetExported(Type type)
        {
            DuktapeFunction value;
            return _exported.TryGetValue(type, out value) ? value : null;
        }

        public void GC(uint refid, Action<IntPtr, uint> op)
        {
            var act = new UnrefAction()
            {
                refid = refid,
                action = op,
            };
            lock (_unrefActions)
            {
                _unrefActions.Enqueue(act);
            }
        }

        private void OnUpdate()
        {
            var ctx = _ctx.rawValue;
            lock (_unrefActions)
            {
                while (true)
                {
                    var count = _unrefActions.Count;
                    if (count == 0)
                    {
                        break;
                    }
                    var act = _unrefActions.Dequeue();
                    act.action(ctx, act.refid);
                    // Debug.LogFormat("duktape gc {0}", act.refid);
                }
            }
        }

        public static void duk_open_module(IntPtr ctx)
        {
            DuktapeDLL.duk_push_object(ctx);
            DuktapeDLL.duk_push_c_function(ctx, cb_resolve_module, DuktapeDLL.DUK_VARARGS);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "resolve");
            DuktapeDLL.duk_push_c_function(ctx, cb_load_module, DuktapeDLL.DUK_VARARGS);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "load");
            DuktapeDLL.duk_module_node_init(ctx);
        }

        public static string EnsureExtension(string filename)
        {
            return filename != null && filename.EndsWith(".js") ? filename : filename + ".js";
        }

        public string ResolvePath(string filename)
        {
            filename = EnsureExtension(filename);
            for (int i = 0, count = _searchPaths.Count; i < count; i++)
            {
                var path = _searchPaths[i];
                var vpath = PathUtils.Combine(path, filename);
                if (_fileManager.Exists(vpath))
                {
                    return vpath;
                }
            }
            return null;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        private static duk_ret_t cb_resolve_module(IntPtr ctx)
        {
            var module_id = EnsureExtension(DuktapeAux.duk_require_string(ctx, 0));
            var parent_id = DuktapeAux.duk_require_string(ctx, 1);
            string resolve_to = null;
            // Debug.LogFormat("cb_resolve_module module_id:'{0}', parent_id:'{1}'\n", module_id, parent_id);

            if (module_id.StartsWith("./") || module_id.StartsWith("../"))
            {
                // 显式相对路径直接从 parent 模块路径拼接
                var parent_path = PathUtils.GetDirectoryName(parent_id);
                resolve_to = PathUtils.GetFullPath(PathUtils.Combine(parent_path, module_id), '/');
                resolve_to = GetVM(ctx).ResolvePath(resolve_to);
            }
            else
            {
                // 默认路径使用 searchPaths 查找
                resolve_to = GetVM(ctx).ResolvePath(module_id);
            }

            if (resolve_to != null)
            {
                DuktapeDLL.duk_push_string(ctx, resolve_to);
                // Debug.LogFormat("resolve_cb: id:{0}', parent-id:'{1}', resolve-to:'{2}'", module_id, parent_id, resolve_to);
            }
            else
            {
                DuktapeDLL.duk_type_error(ctx, "cannot find module: %s", module_id);
            }
            return 1;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        private static duk_ret_t cb_load_module(IntPtr ctx)
        {
            var module_id = DuktapeAux.duk_require_string(ctx, 0);
            DuktapeDLL.duk_get_prop_string(ctx, 2, "filename");
            var filename = DuktapeAux.duk_require_string(ctx, -1);
            // Debug.LogFormat("cb_load_module module_id:'{0}', filename:'{1}'\n", module_id, filename);

            var source = GetVM(ctx)._fileManager.ReadAllText(filename);
            if (source != null)
            {
                DuktapeDLL.duk_push_string(ctx, source);
            }
            else
            {
                DuktapeDLL.duk_type_error(ctx, "cannot load module: %s", module_id);
            }

            return 1;
        }

        public void Initialize(IFileSystem fs, IDuktapeListener listener)
        {
            this._fileManager = fs;
            var ctx = DuktapeDLL.duk_create_heap_default();
            this._ctx = new DuktapeContext(this, ctx);
            DuktapeAux.duk_open(ctx);
            DuktapeVM.duk_open_module(ctx);
            DuktapeDLL.duk_unity_open(ctx);

            DuktapeDLL.duk_push_global_object(ctx);
            DuktapeJSBuiltins.reg(ctx);
            if (listener != null)
            {
                listener.OnTypesBinding(this);
            }
            var exportedTypes = this.GetType().Assembly.GetExportedTypes();
            var ctx_t = new object[] { ctx };
            for (int i = 0, size = exportedTypes.Length; i < size; i++)
            {
                var type = exportedTypes[i];
                var attributes = type.GetCustomAttributes(typeof(JSBindingAttribute), false);
                if (attributes.Length == 1)
                {
                    var jsBinding = attributes[0] as JSBindingAttribute;
                    if (jsBinding.Version == 0 || jsBinding.Version == VERSION)
                    {
                        var reg = type.GetMethod("reg");
                        if (reg != null)
                        {
                            reg.Invoke(null, ctx_t);
                        }
                    }
                    else
                    {
                        if (listener != null)
                        {
                            listener.OnBindingError(this, type);
                        }
                    }
                }
            }
            DuktapeDLL.duk_pop(ctx);
            // Debug.LogFormat("exported {0} classes", _exported.Count);

            // 设置导出类的继承链
            foreach (var kv in _exported)
            {
                var type = kv.Key;
                var baseType = type.BaseType;
                if (baseType == null)
                {
                    // Debug.Log($"baseType is null, for {type}");
                    continue;
                }
                var fn = kv.Value;
                fn.PushPrototype(ctx);
                if (PushChainedPrototypeOf(ctx, baseType))
                {
                    // Debug.LogFormat("set {0} super {1}", type, baseType);
                    DuktapeDLL.duk_set_prototype(ctx, -2);
                }
                DuktapeDLL.duk_pop(ctx);
            }
            DuktapeRunner.SetInterval(this.OnUpdate, 100f);

            if (listener != null)
            {
                listener.OnLoaded(this);
            }
        }

        // 将 type 的 prototype 压栈 （未导出则向父类追溯）
        // 没有对应的基类 prototype 时, 不压栈
        public bool PushChainedPrototypeOf(IntPtr ctx, Type baseType)
        {
            if (baseType == typeof(Enum))
            {
                DuktapeFunction val;
                if (_specialTypes.TryGetValue(SPECIAL_ENUM, out val))
                {
                    val.PushPrototype(ctx);
                    return true;
                }
            }
            if (baseType == typeof(object) || baseType == typeof(ValueType))
            {
                // Debug.LogFormat("super terminated {0}", baseType);
                return false;
            }
            DuktapeFunction fn;
            if (_exported.TryGetValue(baseType, out fn))
            {
                fn.PushPrototype(ctx);
                return true;
            }
            return PushChainedPrototypeOf(ctx, baseType.BaseType);
        }

        public void EvalSource(string source, string filename)
        {
            if (filename == null)
            {
                filename = "eval";
            }
            if (string.IsNullOrEmpty(source))
            {
                return;
            }
            var ctx = _ctx.rawValue;
            DuktapeDLL.duk_push_string(ctx, source);
            DuktapeDLL.duk_push_string(ctx, filename);
            if (DuktapeDLL.duk_pcompile(ctx, 0) != 0)
            {
                Debug.LogErrorFormat("compile error: {0}\n{1}", DuktapeDLL.duk_safe_to_string(ctx, -1), filename);
                DuktapeDLL.duk_pop(ctx);
            }
            else
            {
                // Debug.LogFormat("check top {0}", DuktapeDLL.duk_get_top(ctx));
                if (DuktapeDLL.duk_pcall(ctx, 0) != DuktapeDLL.DUK_EXEC_SUCCESS)
                {
                    DuktapeAux.PrintError(ctx, -1, filename);
                }
                DuktapeDLL.duk_pop(ctx);
                // Debug.LogFormat("check top {0}", DuktapeDLL.duk_get_top(ctx));
            }
        }

        public void EvalFile(string filename)
        {
            var ctx = _ctx.rawValue;
            var path = ResolvePath(filename);
            var source = _fileManager.ReadAllText(path);
            EvalSource(source, filename);
        }

        public void EvalMain(string filename)
        {
            var ctx = _ctx.rawValue;
            var top = DuktapeDLL.duk_get_top(ctx);
            var path = ResolvePath(filename);
            var source = _fileManager.ReadAllText(path);
            DuktapeDLL.duk_push_string(ctx, source);
            var err = DuktapeDLL.duk_module_node_peval_main(ctx, filename);
            // var err = DuktapeDLL.duk_peval(ctx);
            // var err = DuktapeDLL.duk_peval_string_noresult(ctx, source);
            if (err != 0)
            {
                DuktapeAux.PrintError(ctx, -1, filename);
                // Debug.LogErrorFormat("eval main error: {0}\n{1}", DuktapeDLL.duk_safe_to_string(ctx, -1), filename);
            }
            DuktapeDLL.duk_set_top(ctx, top);
        }

        public void Destroy()
        {
            if (_ctx != null)
            {
                DuktapeDLL.duk_destroy_heap(_ctx.rawValue);
                _ctx = null;
            }
        }
    }
}