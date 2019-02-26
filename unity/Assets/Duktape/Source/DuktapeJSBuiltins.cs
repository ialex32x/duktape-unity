using System;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public class DuktapeJSBuiltins : DuktapeBinding
    {
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int BindEnum_GetName(IntPtr ctx)
        {
            Debug.LogWarning("not implemented");
            return 0;
        }

        public static void reg(IntPtr ctx)
        {
            duk_begin_namespace(ctx, "DuktapeJS");
            duk_begin_special(ctx, DuktapeVM.SPECIAL_ENUM);
            duk_add_method(ctx, "GetName", BindEnum_GetName, true);
            duk_end_special(ctx);
            duk_end_namespace(ctx);
        }
    }
}
