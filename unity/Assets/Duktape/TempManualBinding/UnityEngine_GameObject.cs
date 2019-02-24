using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    [JSBinding]
    public class UnityEngine_GameObject : DuktapeBinding
    {
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        static int ctor(IntPtr ctx)
        {
            UnityEngine.GameObject o;
            var argc = DuktapeDLL.duk_get_top(ctx);
            if (argc == 1)
            {
                var arg1 = DuktapeAux.duk_get_string(ctx, 0);
                o = new UnityEngine.GameObject(arg1);
            }
            else
            {
                o = new UnityEngine.GameObject();
            }
            DuktapeDLL.duk_push_this(ctx);
            duk_bind_native(ctx, -1, o);
            DuktapeDLL.duk_pop(ctx);
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        static int SetActive(IntPtr ctx)
        {
            UnityEngine.GameObject o;
            duk_get_this(ctx, out o);
            if (o != null)
            {
                var b = DuktapeDLL.duk_get_boolean(ctx, 0);
                o.SetActive(b);
            }
            else
            {
                // throw op on undefined
            }
            return 0;
        }

        public struct FF {}

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        static int get_activeSelf(IntPtr ctx)
        {
            DuktapeDLL.duk_push_this(ctx);
            UnityEngine.GameObject self;
            duk_get_classvalue(ctx, -1, out self);
            DuktapeDLL.duk_pop(ctx); // pop this
            DuktapeDLL.duk_push_boolean(ctx, self.activeSelf);
            return 1;
        }

        public static void reg(IntPtr ctx)
        {
            duk_begin_namespace(ctx, "UnityEngine");
            duk_begin_class(ctx, typeof(UnityEngine.GameObject), ctor);
            duk_add_method(ctx, "SetActive", SetActive, false);
            duk_add_property(ctx, "activeSelf", get_activeSelf, null, false);
            duk_end_class(ctx);
            duk_end_namespace(ctx);
        }
    }
}
