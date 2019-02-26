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
        public static int ClearTimer(IntPtr ctx)
        {
            if (DuktapeDLL.duk_is_number(ctx, 0))
            {
                var id = DuktapeDLL.duk_get_int(ctx, 0);
                DuktapeDLL.duk_push_boolean(ctx, DuktapeRunner.Clear(id));
                return 1;
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int SetInterval(IntPtr ctx)
        {
            DuktapeFunction fn;
            var idx = _GetTimerFunction(ctx, out fn);
            if (idx < 0)
            {
                var id = DuktapeRunner.SetInterval(fn, (float)DuktapeDLL.duk_get_number(ctx, 1));
                DuktapeDLL.duk_push_int(ctx, id);
                return 1;
            }
            return DuktapeDLL.duk_generic_error(ctx, "invalid arg " + idx);
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int SetTimeout(IntPtr ctx)
        {
            DuktapeFunction fn;
            var idx = _GetTimerFunction(ctx, out fn);
            if (idx < 0)
            {
                var id = DuktapeRunner.SetTimeout(fn, (float)DuktapeDLL.duk_get_number(ctx, 1));
                DuktapeDLL.duk_push_int(ctx, id);
                return 1;
            }
            return DuktapeDLL.duk_generic_error(ctx, "invalid arg " + idx);
        }

        private static int _GetTimerFunction(IntPtr ctx, out DuktapeFunction fn)
        {
            if (!DuktapeDLL.duk_is_function(ctx, 0))
            {
                fn = null;
                return 0;
            }
            if (!DuktapeDLL.duk_is_number(ctx, 1))
            {
                fn = null;
                return 1;
            }
            var top = DuktapeDLL.duk_get_top_index(ctx);
            DuktapeValue[] argv = null;
            if (top > 1)
            {
                argv = new DuktapeValue[top - 1];
                for (var i = 2; i <= top; i++)
                {
                    DuktapeDLL.duk_dup(ctx, i);
                    argv[i - 2] = new DuktapeValue(ctx, DuktapeDLL.duk_unity_ref(ctx));
                }
            }
            DuktapeDLL.duk_dup(ctx, 0);
            fn = new DuktapeFunction(ctx, DuktapeDLL.duk_unity_ref(ctx), argv);
            return -1;
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
            duk_add_method(ctx, "clearInterval", ClearTimer, -1);
            duk_add_method(ctx, "clearTimeout", ClearTimer, -1);
        }
    }
}
