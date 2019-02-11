using System;
using UnityEngine;

namespace Duktape
{
    using duk_size_t = System.Int32;
    using duk_int_t = System.Int32;
    using duk_idx_t = System.Int32;
    using duk_uint_t = System.UInt32;
    using duk_uarridx_t = System.UInt32;
    using duk_bool_t = System.Boolean;
    using duk_double_t = System.Double;
    using duk_errcode_t = System.Int32;
    using duk_codepoint_t = System.Int32;
    using duk_ret_t = System.Int32;

    public static partial class DuktapeAux
    {
        public const string HEAP_STASH_PROPS_REGISTRY = "registry";

        public static void duk_open(IntPtr ctx)
        {
            DuktapeDLL.duk_push_heap_stash(ctx); // [stash]
            DuktapeDLL.duk_push_array(ctx); // [stash, array]
            DuktapeDLL.duk_dup_top(ctx); // [stash, array, array]
            DuktapeDLL.duk_put_prop_string(ctx, -3, HEAP_STASH_PROPS_REGISTRY); // [stash, array]
            DuktapeDLL.duk_push_int(ctx, 0); // [stash, array, 0]
            DuktapeDLL.duk_put_prop_index(ctx, -2, 0); // [stash, array]
            DuktapeDLL.duk_pop_2(ctx);
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
                DuktapeDLL.duk_pop_3(ctx); // obj
            }
            else
            {
                refid = DuktapeDLL.duk_get_length(ctx, -2);
                DuktapeDLL.duk_dup(ctx, -4); // obj, stash, array, array[0], obj
                DuktapeDLL.duk_put_prop_index(ctx, -3, (uint)refid); // obj, stash, array, array[0]
                DuktapeDLL.duk_pop_3(ctx); // obj
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

        private static duk_ret_t cb_resolve_module(IntPtr ctx)
        {
            var module_id = DuktapeAux.duk_require_string(ctx, 0);
            var parent_id = DuktapeAux.duk_require_string(ctx, 1);
            string resolve_to;

            if (module_id.StartsWith("./") || module_id.StartsWith("../"))
            {
                var parent_path = PathUtils.GetDirectoryName(parent_id);
                resolve_to = PathUtils.GetFullPath(PathUtils.Combine(parent_path, module_id + ".js"), '/');
            }
            else
            {
                resolve_to = module_id + ".js";
            }

            DuktapeDLL.duk_push_string(ctx, resolve_to);
            Debug.LogFormat("resolve_cb: id:{0}', parent-id:'{1}', resolve-to:'{2}'",
                module_id, parent_id, resolve_to);

            return 1;
        }

        private static duk_ret_t cb_load_module(IntPtr ctx)
        {
            string filename;
            string module_id;

            module_id = DuktapeAux.duk_require_string(ctx, 0);
            DuktapeDLL.duk_get_prop_string(ctx, 2, "filename");
            filename = DuktapeAux.duk_require_string(ctx, -1);

            Debug.LogFormat("load_cb: id:'{0}', filename:'{1}'\n", module_id, filename);

            if (module_id.Contains("pig.js"))
            {
                DuktapeDLL.duk_push_string(ctx, string.Format("module.exports = require('../dummy1/dummy2/../farm/duck');",
                    module_id));
            }
            else if (module_id.Contains("duck.js"))
            {
                DuktapeDLL.duk_push_string(ctx, string.Format("module.exports = 'you\\'re about to get eaten by {0}';",
                    module_id));
            }
            else
            {
                DuktapeDLL.duk_type_error(ctx, "cannot find module: %s", module_id);
            }

            return 1;
        }
    }
}
