using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duktape
{
    using duk_ret_t = System.Int32;

    // 临时
    public class DuktapeVM // : Scripting.ScriptEngine
    {
        private DuktapeContext _ctx;
        private IFileSystem _fileManager;

        private List<string> _searchPaths = new List<string>();

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

        public void AddSearchPath(string path)
        {
            if (!_searchPaths.Contains(path))
            {
                _searchPaths.Add(path);
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

        public void Initialize(IFileSystem fs, Action<float> onprogress, Action onloaded)
        {
            this._fileManager = fs;
            var ctx = DuktapeDLL.duk_create_heap_default();
            this._ctx = new DuktapeContext(this, ctx);
            DuktapeAux.duk_open(ctx);
            DuktapeVM.duk_open_module(ctx);
            if (onloaded != null)
            {
                onloaded();
            }
        }

        public void EvalFile(string filename)
        {
            var ctx = _ctx.rawValue;
            var path = ResolvePath(filename);
            var source = _fileManager.ReadAllText(path);
            // var err = DuktapeDLL.duk_module_node_peval_main(ctx, path);
            var err = DuktapeDLL.duk_peval_string(ctx, source);
            // var err = DuktapeDLL.duk_peval_string_noresult(ctx, source);
            if (err != 0)
            {
                Debug.LogErrorFormat("eval error: {0}\n{1}", DuktapeDLL.duk_safe_to_string(ctx, -1), source);
            }
            DuktapeDLL.duk_pop(ctx);
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