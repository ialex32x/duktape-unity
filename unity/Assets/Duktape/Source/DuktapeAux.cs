using System;
using UnityEngine;

namespace Duktape
{
    // 基础环境
    public static partial class DuktapeAux
    {
        public static void duk_open(IntPtr ctx)
        {
            DuktapeDLL.duk_push_global_object(ctx);
            DuktapeDLL.duk_push_c_function(ctx, duk_print, DuktapeDLL.DUK_VARARGS);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "print");
            DuktapeDLL.duk_pop(ctx);
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int duk_print(IntPtr ctx)
        {
            var narg = DuktapeDLL.duk_get_top(ctx);
            var str = string.Empty;
            for (int i = 0; i < narg; i++)
            {
                str += DuktapeDLL.duk_safe_to_string(ctx, i) + " ";
            }
            Debug.Log(str);
            return 0;
        }
    }
}
