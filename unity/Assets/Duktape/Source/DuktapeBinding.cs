using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;

    public partial class DuktapeBinding
    {
        public static void duk_get_namespace(IntPtr ctx, string el) // [parent]
        {
            if (!DuktapeDLL.duk_get_prop_string(ctx, -1, el)) // [parent, el]
            {
                DuktapeDLL.duk_pop(ctx); // [parent]
                DuktapeDLL.duk_push_object(ctx); // [parent, new_object]
                DuktapeDLL.duk_dup_top(ctx); // [parent, new_object]
                DuktapeDLL.duk_put_prop_string(ctx, -3, el); // [parent, el]
            }
        }

        public static void duk_get_namespace(IntPtr ctx, string el1, string el2) // [parent]
        {
            duk_get_namespace(ctx, el1); // [parent, el1]
            duk_get_namespace(ctx, el2); // [parent, el1, el2]
            DuktapeDLL.duk_remove(ctx, -2); // [parent, el2]
        }

        public static void duk_get_namespace(IntPtr ctx, string el1, string el2, string el3) // [parent]
        {
            duk_get_namespace(ctx, el1); // [parent, el1]
            duk_get_namespace(ctx, el2); // [parent, el1, el2]
            DuktapeDLL.duk_remove(ctx, -2); // [parent, el2]
            duk_get_namespace(ctx, el3); // [parent, el2, el3]
            DuktapeDLL.duk_remove(ctx, -2); // [parent, el3]
        }

        // return [parent, el]
        public static void duk_get_namespace(IntPtr ctx, params string[] els) // [parent]
        {
            duk_get_namespace(ctx, els[0]); // [parent, el0]
            for (int i = 1, size = els.Length; i < size; i++)
            {
                var el = els[i];
                duk_get_namespace(ctx, el); // [parent, eli-1, eli]
                DuktapeDLL.duk_remove(ctx, -2); // [parent, eli]
            }
        }

        protected static void duk_begin_type(IntPtr ctx, Type type, Type super)
        {
            // type.Namespace
            DuktapeDLL.duk_push_global_object(ctx); // [global]
            // duk_get_namespace(ctx, )
        }

        protected static void duk_end_type(IntPtr ctx)
        {

        }

        protected static void duk_ctor(IntPtr ctx, DuktapeDLL.duk_c_function func)
        {

        }

        protected static void duk_dtor(IntPtr ctx, DuktapeDLL.duk_c_function func)
        {

        }

        protected static void duk_put_method(IntPtr ctx, string name, DuktapeDLL.duk_c_function func, bool bStatic)
        {

        }

        protected static void duk_put_property(IntPtr ctx, string name, DuktapeDLL.duk_c_function getter, DuktapeDLL.duk_c_function setter, bool bStatic)
        {

        }

        protected static void duk_put_value(IntPtr ctx, string name, int v)
        {

        }

        protected static void duk_put_value(IntPtr ctx, string name, string v)
        {

        }
    }
}
