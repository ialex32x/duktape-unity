using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;

namespace Duktape
{
    using System.Runtime.InteropServices;
    using UnityEngine;
    using duk_ret_t = System.Int32;

    public struct UnrefAction
    {
        public uint refid;
        public object target;
        public Action<IntPtr, uint, object> action;
    }

    public class DuktapeVM // : Scripting.ScriptEngine
    {
        // duktape-unity 版本, 生成规则发生无法兼容的改变时增加版本号
        public const int VERSION = 0x10001;
        // public const string HEAP_STASH_PROPS_REGISTRY = "registry";
        // duk_add_event 生成的 object 中存储 this 的属性名
        // public static readonly string EVENT_PROP_THIS = DuktapeDLL.DUK_HIDDEN_SYMBOL("this");
        // 在jsobject实例上记录关联的本地对象 object cache refid
        // public static readonly string OBJ_PROP_NATIVE = DuktapeDLL.DUK_HIDDEN_SYMBOL("1");
        // public static readonly string OBJ_PROP_NATIVE_WEAK = DuktapeDLL.DUK_HIDDEN_SYMBOL("2");
        // public static readonly string OBJ_PROP_TYPE = DuktapeDLL.DUK_HIDDEN_SYMBOL("3");
        // 导出类的js构造函数隐藏属性, 记录在vm中的注册id
        // public static readonly string OBJ_PROP_EXPORTED_REFID = DuktapeDLL.DUK_HIDDEN_SYMBOL("4");
        // public static readonly string OBJ_PROP_SPECIAL_REFID = DuktapeDLL.DUK_HIDDEN_SYMBOL("special-refid");

        public const string _DuktapeDelegates = "_DuktapeDelegates";

        public const string SPECIAL_ENUM = "Enum";
        public const string SPECIAL_DELEGATE = "Delegate";
        public const string SPECIAL_CSHARP = "CSharp";

        private static DuktapeVM _instance;
        private uint _updateTimer;
        private DuktapeContext _ctx;
        private IFileResolver _fileResolver;
        private ObjectCache _objectCache = new ObjectCache();
        private Queue<UnrefAction> _unrefActions = new Queue<UnrefAction>();

        // 已经导出的本地类
        private Dictionary<Type, DuktapeFunction> _exported = new Dictionary<Type, DuktapeFunction>();
        private Dictionary<uint, Type> _exportedTypes = new Dictionary<uint, Type>(); // 从 refid 反查 Type

        private Dictionary<string, DuktapeFunction> _specialTypes = new Dictionary<string, DuktapeFunction>(); // 从 refid 反查 Type

        private Dictionary<Type, MethodInfo> _delegates = new Dictionary<Type, MethodInfo>(); // 委托对应的 duktape 绑定函数

        // private static int _thread = 0;

        private static Dictionary<IntPtr, DuktapeContext> _contexts = new Dictionary<IntPtr, DuktapeContext>();
        private static IntPtr _lastContextPtr;
        private static DuktapeContext _lastContext;

        public DuktapeContext context
        {
            get { return _ctx; }
        }

        public IntPtr ctx
        {
            get { return _ctx != null ? _ctx.rawValue : IntPtr.Zero; }
        }

        public IFileResolver fileResolver
        {
            get { return _fileResolver; }
        }

        public DuktapeVM()
        {
            _instance = this;

            var ctx = DuktapeDLL.duk_create_heap_default();

            _ctx = new DuktapeContext(this, ctx);
            DuktapeAux.duk_open(ctx);
            DuktapeVM.duk_open_module(ctx);
            DuktapeDLL.duk_unity_open(ctx);
        }

        public void GetMemoryState(out uint count, out uint size)
        {
            DuktapeDLL.duk_unity_get_memory_state(ctx, out count, out size);
        }

        public static DuktapeVM GetInstance()
        {
            return _instance;
        }

        public static void addContext(DuktapeContext context)
        {
            var ctx = context.rawValue;
            _contexts[ctx] = context;
            _lastContext = context;
            _lastContextPtr = ctx;
        }

        public static DuktapeVM GetVM(IntPtr ctx)
        {
            return GetContext(ctx)?.vm;
        }

        public static ObjectCache GetObjectCache(IntPtr ctx)
        {
            return GetContext(ctx)?.vm?._objectCache;
        }

        public static DuktapeContext GetContext(IntPtr ctx)
        {
            if (_lastContextPtr == ctx)
            {
                return _lastContext;
            }
            DuktapeContext context;
            if (_contexts.TryGetValue(ctx, out context))
            {
                _lastContext = context;
                _lastContextPtr = ctx;
                return context;
            }
            // fixme 如果是 thread 则获取对应 main context
            return null;
        }

        public void AddSearchPath(string path)
        {
            _fileResolver.AddSearchPath(path);
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
            // Debug.Log($"add export: {type}");
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

        public void GC(uint refid, object target, Action<IntPtr, uint, object> op)
        {
            var act = new UnrefAction()
            {
                refid = refid,
                action = op,
                target = target,
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
                    act.action(ctx, act.refid, act.target);
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

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        private static duk_ret_t cb_resolve_module(IntPtr ctx)
        {
            var module_id = EnsureExtension(DuktapeAux.duk_require_string(ctx, 0));
            var parent_id = DuktapeAux.duk_require_string(ctx, 1);
            var resolve_to = module_id;
            // Debug.LogFormat("cb_resolve_module module_id:'{0}', parent_id:'{1}'\n", module_id, parent_id);

            if (module_id.StartsWith("./") || module_id.StartsWith("../") || module_id.Contains("/./") || module_id.Contains("/../"))
            {
                // 显式相对路径直接从 parent 模块路径拼接
                var parent_path = PathUtils.GetDirectoryName(parent_id);
                try
                {
                    resolve_to = PathUtils.ExtractPath(PathUtils.Combine(parent_path, module_id), '/');
                }
                catch
                {
                    // 不能提升到源代码目录外面
                    DuktapeDLL.duk_type_error(ctx, "invalid module path (out of sourceRoot): %s", module_id);
                    return 1;
                }
            }
            // Debug.LogFormat("resolve_cb(1): id:{0}', parent-id:'{1}', resolve-to:'{2}'", module_id, parent_id, resolve_to);
            // if (GetVM(ctx).ResolvePath(resolve_to) == null)
            // {
            //     DuktapeDLL.duk_type_error(ctx, "cannot find module: %s", module_id);
            //     return 1;
            // }

            if (resolve_to != null)
            {
                DuktapeDLL.duk_push_string(ctx, resolve_to);
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
            var source = GetVM(ctx)._fileResolver.ReadAllBytes(filename);
            // Debug.LogFormat("cb_load_module module_id:'{0}', filename:'{1}', resolved:'{2}'\n", module_id, filename, resolvedPath);
            do
            {
                if (source != null && source.Length > 0) // bytecode is unsupported
                {
                    if (source[0] != 0xbf)
                    {
                        DuktapeDLL.duk_unity_push_lstring(ctx, source, (uint)source.Length);
                    }
                    else
                    {
                        DuktapeDLL.duk_type_error(ctx, "cannot load module (bytecode): %s", module_id);
                    }
                    break;
                }
                DuktapeDLL.duk_type_error(ctx, "cannot load module: %s", module_id);
            } while (false);
            return 1;
        }

        private IEnumerator _InitializeStep(IDuktapeListener listener, int step)
        {
            DuktapeDLL.duk_push_global_object(ctx);
            DuktapeJSBuiltins.reg(ctx);
            listener?.OnTypesBinding(this);
            var ctxAsArgs = new object[] { ctx };
            var bindingTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int assemblyIndex = 0, assemblyCount = assemblies.Length; assemblyIndex < assemblyCount; assemblyIndex++)
            {
                var assembly = assemblies[assemblyIndex];
                try
                {
                    if (assembly.IsDynamic)
                    {
                        continue;
                    }
                    var exportedTypes = assembly.GetExportedTypes();
                    for (int i = 0, size = exportedTypes.Length; i < size; i++)
                    {
                        var type = exportedTypes[i];
#if UNITY_EDITOR
                        if (type.IsDefined(typeof(JSAutoRunAttribute), false))
                        {
                            try
                            {
                                var run = type.GetMethod("Run", BindingFlags.Static | BindingFlags.Public);
                                if (run != null)
                                {
                                    run.Invoke(null, null);
                                }
                            }
                            catch (Exception exception)
                            {
                                Debug.LogWarning($"JSAutoRun failed: {exception}");
                            }
                            continue;
                        }
#endif
                        var attributes = type.GetCustomAttributes(typeof(JSBindingAttribute), false);
                        if (attributes.Length == 1)
                        {
                            var jsBinding = attributes[0] as JSBindingAttribute;
                            if (jsBinding.Version == 0 || jsBinding.Version == VERSION)
                            {
                                bindingTypes.Add(type);
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
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("assembly: {0}, {1}", assembly, e);
                }
            }
            var numRegInvoked = bindingTypes.Count;
            for (var i = 0; i < numRegInvoked; ++i)
            {
                var type = bindingTypes[i];
                var reg = type.GetMethod("reg");
                if (reg != null)
                {
                    reg.Invoke(null, ctxAsArgs);
                    if (listener != null)
                    {
                        listener.OnProgress(this, i, numRegInvoked);
                    }

                    if (i % step == 0)
                    {
                        yield return null;
                    }
                }
            }
            if (listener != null)
            {
                listener.OnBinded(this, numRegInvoked);
            }
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
                    // Debug.LogFormat($"set {type} super {baseType}");
                    DuktapeDLL.duk_set_prototype(ctx, -2);
                }
                else
                {
                    Debug.LogWarning($"fail to push prototype, for {type}: {baseType}");
                }
                DuktapeDLL.duk_pop(ctx);
            }

            DuktapeJSBuiltins.postreg(ctx);
            DuktapeDLL.duk_pop(ctx); // pop global 

            _updateTimer = DuktapeRunner.SetInterval(this.OnUpdate, 100);

            if (listener != null)
            {
                listener.OnLoaded(this);
            }
        }

        public void Initialize(IDuktapeListener listener, int step = 30)
        {
            Initialize(new FileResolver(new DefaultFileSystem()), listener, step);
        }

        public void Initialize(IFileSystem fileSystem, IDuktapeListener listener, int step = 30)
        {
            Initialize(new FileResolver(fileSystem), listener, step);
        }

        public void Initialize(IFileResolver fileResolver, IDuktapeListener listener, int step = 30)
        {
            _fileResolver = fileResolver;
            var runner = DuktapeRunner.GetRunner();
            if (runner != null)
            {
                runner.StartCoroutine(_InitializeStep(listener, step));
            }
            else
            {
                var e = _InitializeStep(listener, step);
                while (e.MoveNext()) ;
            }
        }

        // 将 type 的 prototype 压栈 （未导出则向父类追溯）
        // 没有对应的基类 prototype 时, 不压栈
        public bool PushChainedPrototypeOf(IntPtr ctx, Type baseType)
        {
            if (baseType == null)
            {
                // Debug.LogFormat("super terminated {0}", baseType);
                return false;
            }
            if (baseType == typeof(Enum))
            {
                DuktapeFunction val;
                if (_specialTypes.TryGetValue(SPECIAL_ENUM, out val))
                {
                    val.PushPrototype(ctx);
                    return true;
                }
            }
            DuktapeFunction fn;
            if (_exported.TryGetValue(baseType, out fn))
            {
                fn.PushPrototype(ctx);
                return true;
            }
            return PushChainedPrototypeOf(ctx, baseType.BaseType);
        }

        public void EvalSource(string filename, byte[] source)
        {
            if (source == null || source.Length == 0)
            {
                return;
            }
            if (filename == null)
            {
                filename = "eval";
            }
            var ctx = _ctx.rawValue;
            do
            {
                if (source[0] == 0xbf)
                {
                    // load bytecode...
                    var buffer_ptr = DuktapeDLL.duk_push_fixed_buffer(ctx, (uint)source.Length);
                    Marshal.Copy(source, 0, buffer_ptr, source.Length);
                    DuktapeDLL.duk_load_function(ctx);
                }
                else
                {
                    DuktapeDLL.duk_unity_push_lstring(ctx, source, (uint)source.Length);
                    DuktapeDLL.duk_push_string(ctx, filename);
                    if (DuktapeDLL.duk_pcompile(ctx, 0) != 0)
                    {
                        DuktapeAux.PrintError(ctx, -1, filename);
                        DuktapeDLL.duk_pop(ctx);
                        break;
                    }
                }
                // Debug.LogFormat("check top {0}", DuktapeDLL.duk_get_top(ctx));
                if (DuktapeDLL.duk_pcall(ctx, 0) != DuktapeDLL.DUK_EXEC_SUCCESS)
                {
                    DuktapeAux.PrintError(ctx, -1, filename);
                }
                DuktapeDLL.duk_pop(ctx);
                // Debug.LogFormat("check top {0}", DuktapeDLL.duk_get_top(ctx));
            } while (false);
        }

        public void EvalFile(string filename)
        {
            filename = EnsureExtension(filename);
            var ctx = _ctx.rawValue;
            var source = _fileResolver.ReadAllBytes(filename);
            EvalSource(filename, source);
        }

        public void EvalMain(string filename)
        {
            filename = EnsureExtension(filename);
            var source = _fileResolver.ReadAllBytes(filename);
            EvalMain(filename, source);
        }

        public void EvalMain(string filename, byte[] source)
        {
            if (source == null || source.Length == 0)
            {
                return;
            }
            if (source[0] == 0xbf)
            {
                //NOTE: module is not supported in bytecode mode
                EvalSource(filename, source);
            }
            else
            {
                if (filename == null)
                {
                    filename = "eval";
                }
                var ctx = _ctx.rawValue;
                var top = DuktapeDLL.duk_get_top(ctx);

                DuktapeDLL.duk_unity_push_lstring(ctx, source, (uint)source.Length);
                var err = DuktapeDLL.duk_module_node_peval_main(ctx, filename);
                // var err = DuktapeDLL.duk_peval(ctx);
                // var err = DuktapeDLL.duk_peval_string_noresult(ctx, source);
                // Debug.Log($"load main module: {filename} ({resolvedPath})");
                if (err != 0)
                {
                    DuktapeAux.PrintError(ctx, -1, filename);
                    // Debug.LogErrorFormat("eval main error: {0}\n{1}", DuktapeDLL.duk_safe_to_string(ctx, -1), filename);
                }
                DuktapeDLL.duk_set_top(ctx, top);
            }
        }

        public byte[] DumpBytecode(string filename, byte[] source)
        {
            return _ctx != null ? DuktapeAux.DumpBytecode(_ctx.rawValue, filename, source) : null;
        }

        public void Destroy()
        {
            _instance = null;
            if (_ctx != null)
            {
                var ctx = _ctx.rawValue;
                _ctx.onDestroy();
                _ctx = null;
                _lastContextPtr = IntPtr.Zero;
                _lastContext = null;
                _contexts.Clear();
                _objectCache.Clear();
                DuktapeDLL.duk_destroy_heap_default(ctx);
                // Debug.LogWarning("duk_destroy_heap");
            }

            if (_updateTimer != 0)
            {
                DuktapeRunner.Clear(_updateTimer);
                _updateTimer = 0;
            }
        }
    }
}