using System;
using UnityEngine;

namespace Duktape
{
    public static partial class DuktapeAux
    {
        private static IFileManager _fileManager;
        public const string HEAP_STASH_PROPS_REGISTRY = "registry";

        // 临时
        public static IFileManager fileManager
        {
            get { return _fileManager; }
        }

        public static void duk_open(IntPtr ctx, IFileManager fileManager)
        {
            _fileManager = fileManager;

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
                DuktapeDLL.duk_pop_3(ctx); // obj
            }
            else
            {
                refid = (int)DuktapeDLL.duk_get_length(ctx, -2);
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
    }
}
