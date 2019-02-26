using System;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public class DuktapeJSBuiltins : DuktapeBinding
    {
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int Enum_GetName(IntPtr ctx)
        {
            Debug.LogWarning("not implemented");
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int SetInterval(IntPtr ctx)
        {
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int ClearInterval(IntPtr ctx)
        {
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int SetTimeout(IntPtr ctx)
        {
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int ClearTimeout(IntPtr ctx)
        {
            return 0;
        }

        public static void reg(IntPtr ctx)
        {
            duk_begin_namespace(ctx, "DuktapeJS");
            duk_begin_special(ctx, DuktapeVM.SPECIAL_ENUM);
            duk_add_method(ctx, "GetName", Enum_GetName, true);
            duk_end_special(ctx);
            duk_end_namespace(ctx);
            duk_add_method(ctx, "setInterval", SetInterval, -1);
            duk_add_method(ctx, "setTimeout", SetTimeout, -1);
            duk_add_method(ctx, "clearInterval", ClearInterval, -1);
            duk_add_method(ctx, "clearTimeout", ClearTimeout, -1);
        }
    }
}
