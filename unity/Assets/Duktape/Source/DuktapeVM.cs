using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using duk_ret_t = System.Int32;

    public struct UnrefAction
    {
        public int refid;
        public Action<IntPtr, int> action;
    }

    public class DuktapeVM // : Scripting.ScriptEngine
    {
        public const string HEAP_STASH_PROPS_REGISTRY = "registry";
        public static readonly string OBJ_PROP_NATIVE = DuktapeDLL.DUK_HIDDEN_SYMBOL("native");

        private DuktapeContext _ctx;
        private IFileSystem _fileManager;
        private ObjectCache _objectCache = new ObjectCache();

        private List<string> _searchPaths = new List<string>();

        private Queue<UnrefAction> _unrefActions = new Queue<UnrefAction>();

        // 已经导出的本地类
        private Dictionary<Type, DuktapeFunction> _exported = new Dictionary<Type, DuktapeFunction>();

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

        public void AddExported(Type type, DuktapeFunction fn)
        {
            _exported.Add(type, fn);
        }

        // 得到注册在js中的类型对应的构造函数
        public DuktapeFunction GetExported(Type type)
        {
            DuktapeFunction value;
            return _exported.TryGetValue(type, out value) ? value : null;
        }

        public static void duk_open_ref(IntPtr ctx)
        {
            DuktapeDLL.duk_push_heap_stash(ctx); // [stash]
            DuktapeDLL.duk_push_array(ctx); // [stash, array]
            DuktapeDLL.duk_dup_top(ctx); // [stash, array, array]
            DuktapeDLL.duk_put_prop_string(ctx, -3, HEAP_STASH_PROPS_REGISTRY); // [stash, array]
            DuktapeDLL.duk_push_int(ctx, 0); // [stash, array, 0]
            DuktapeDLL.duk_put_prop_index(ctx, -2, 0); // [stash, array]
            DuktapeDLL.duk_pop_2(ctx);
        }

        /// Creates and returns a reference for the object at the top of the stack (and pops the object).
        public static int duk_ref(IntPtr ctx)
        {
            DuktapeDLL.duk_push_heap_stash(ctx); // obj, stash
            DuktapeDLL.duk_get_prop_string(ctx, -1, HEAP_STASH_PROPS_REGISTRY); // obj, stash, array
            DuktapeDLL.duk_get_prop_index(ctx, -1, 0); // obj, stash, array, array[0]
            var refid = DuktapeDLL.duk_get_int(ctx, -1); // obj, stash, array, array[0]
            if (refid > 0)
            {
                DuktapeDLL.duk_get_prop_index(ctx, -2, (uint)refid); // obj, stash, array, array[0], array[refid]
                var freeid = DuktapeDLL.duk_get_int(ctx, -1);
                DuktapeDLL.duk_put_prop_index(ctx, -3, 0); // obj, stash, array, array[0] ** update free ptr
                DuktapeDLL.duk_dup(ctx, -4); // obj, stash, array, array[0], obj
                DuktapeDLL.duk_put_prop_index(ctx, -3, (uint)refid); // obj, stash, array, array[0]
                DuktapeDLL.duk_pop_n(ctx, 4); // []
            }
            else
            {
                refid = (int)DuktapeDLL.duk_unity_get_length(ctx, -2);
                DuktapeDLL.duk_dup(ctx, -4); // obj, stash, array, array[0], obj
                DuktapeDLL.duk_put_prop_index(ctx, -3, (uint)refid); // obj, stash, array, array[0]
                DuktapeDLL.duk_pop_n(ctx, 4); // []
            }
            return refid;
        }

        // 将refid关联的对象push到当前栈顶
        public static void duk_push_ref(IntPtr ctx, int refid)
        {
            DuktapeDLL.duk_push_heap_stash(ctx); // stash
            DuktapeDLL.duk_get_prop_string(ctx, -1, HEAP_STASH_PROPS_REGISTRY); // stash, array
            DuktapeDLL.duk_get_prop_index(ctx, -1, (uint)refid); // stash, array, array[refid]
            DuktapeDLL.duk_remove(ctx, -2);
            DuktapeDLL.duk_remove(ctx, -2);
        }

        /// Releases reference refid
        public static void duk_unref(IntPtr ctx, int refid)
        {
            DuktapeDLL.duk_push_heap_stash(ctx); // stash
            DuktapeDLL.duk_get_prop_string(ctx, -1, HEAP_STASH_PROPS_REGISTRY); // stash, array
            DuktapeDLL.duk_get_prop_index(ctx, -1, 0); // stash, array, array[0]
            var freeid = DuktapeDLL.duk_get_int(ctx, -1); // stash, array, array[0]
            DuktapeDLL.duk_push_int(ctx, refid); // stash, array, array[0], refid
            DuktapeDLL.duk_put_prop_index(ctx, -3, 0); // stash, array, array[0] ** set t[freeid] = refid
            DuktapeDLL.duk_push_int(ctx, freeid); // stash, array, array[0], freeid
            DuktapeDLL.duk_put_prop_index(ctx, -3, (uint)refid); // stash, array, array[0] ** set t[refid] = freeid
            DuktapeDLL.duk_pop_3(ctx); // []
        }


        public void GC(int refid, Action<IntPtr, int> op)
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

        public void Update(float dt)
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

            if (module_id.StartsWith("./") || module_id.StartsWith("../"))
            {
                // 显式相对路径直接从 parent 模块路径拼接
                var parent_path = PathUtils.GetDirectoryName(parent_id);
                resolve_to = PathUtils.GetFullPath(PathUtils.Combine(parent_path, module_id), '/');
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

            var source = GetVM(ctx)._fileManager.ReadAllText(filename);
            if (source != null)
            {
                DuktapeDLL.duk_push_string(ctx, source);
                // Debug.LogFormat("load_cb: id:'{0}', filename:'{1}'\n", module_id, filename);
            }
            else
            {
                DuktapeDLL.duk_type_error(ctx, "cannot load module: %s", module_id);
            }

            return 1;
        }

        public void Initialize(IFileSystem fs, List<Action<IntPtr>> custom, Action<float> onprogress, Action onloaded)
        {
            this._fileManager = fs;
            var ctx = DuktapeDLL.duk_create_heap_default();
            this._ctx = new DuktapeContext(this, ctx);
            DuktapeAux.duk_open(ctx);
            DuktapeVM.duk_open_module(ctx);
            DuktapeVM.duk_open_ref(ctx);

            DuktapeDLL.duk_push_global_object(ctx);
            for (int i = 0, size = custom.Count; i < size; i++)
            {
                var reg = custom[i];
                reg(ctx);
            }
            DuktapeDLL.duk_pop(ctx);
            // Debug.LogFormat("exported {0} classes", _exported.Count);

            // 设置导出类的继承链
            foreach (var kv in _exported)
            {
                var type = kv.Key;
                var baseType = type.BaseType;
                var fn = kv.Value;
                fn.PushPrototype(ctx);
                if (PushSuperPrototypeOf(ctx, type))
                {
                    // Debug.LogFormat("duktapeVM set {0} super {1}", type, baseType);
                    DuktapeDLL.duk_set_prototype(ctx, -2);
                }
                DuktapeDLL.duk_pop(ctx);
            }

            if (onloaded != null)
            {
                onloaded();
            }
        }

        // 将 type 的基类的 prototype 压栈 （未导出则向父类追溯）
        // 没有对应的基类 prototype 时, 不压栈
        private bool PushSuperPrototypeOf(IntPtr ctx, Type type)
        {
            var baseType = type.BaseType;
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
            return PushSuperPrototypeOf(ctx, baseType);
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
                    Debug.LogErrorFormat("call error: {0}\n{1}", DuktapeDLL.duk_safe_to_string(ctx, -1), filename);
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
            var path = ResolvePath(filename);
            var source = _fileManager.ReadAllText(path);
            DuktapeDLL.duk_push_string(ctx, source);
            var err = DuktapeDLL.duk_module_node_peval_main(ctx, path);
            // var err = DuktapeDLL.duk_peval(ctx);
            // var err = DuktapeDLL.duk_peval_string_noresult(ctx, source);
            if (err != 0)
            {
                Debug.LogErrorFormat("eval main error: {0}\n{1}", DuktapeDLL.duk_safe_to_string(ctx, -1), source);
            }
            DuktapeDLL.duk_pop(ctx);
        }

        public void Destroy()
        {
            DuktapeDLL.duk_destroy_heap(_ctx.rawValue);
        }
    }
}