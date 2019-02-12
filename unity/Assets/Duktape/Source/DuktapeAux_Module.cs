using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duktape
{
    using duk_ret_t = System.Int32;

    public static partial class DuktapeAux
    {
        private static List<string> _searchPaths = new List<string>();

        public static void AddSearchPath(string path)
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

        public static string ResolvePath(string filename)
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
                resolve_to = ResolvePath(module_id);
            }

            if (resolve_to != null)
            {
                DuktapeDLL.duk_push_string(ctx, resolve_to);
                Debug.LogFormat("resolve_cb: id:{0}', parent-id:'{1}', resolve-to:'{2}'",
                    module_id, parent_id, resolve_to);
            }
            else
            {
                DuktapeDLL.duk_type_error(ctx, "cannot find module: %s", module_id);
            }
            return 1;
        }

        private static duk_ret_t cb_load_module(IntPtr ctx)
        {
            var module_id = DuktapeAux.duk_require_string(ctx, 0);
            DuktapeDLL.duk_get_prop_string(ctx, 2, "filename");
            var filename = DuktapeAux.duk_require_string(ctx, -1);

            var source = _fileManager.LoadText(filename);
            if (source != null)
            {
                DuktapeDLL.duk_push_string(ctx, source);
                Debug.LogFormat("load_cb: id:'{0}', filename:'{1}'\n", module_id, filename);
            }
            else
            {
                DuktapeDLL.duk_type_error(ctx, "cannot load module: %s", module_id);
            }

            return 1;
        }
    }
}
