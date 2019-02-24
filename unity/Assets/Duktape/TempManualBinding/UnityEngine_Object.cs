using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    [JSBinding]
    public class UnityEngine_Object : DuktapeBinding
    {
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        static int ctor(IntPtr ctx)
        {
            DuktapeDLL.duk_push_this(ctx);
            var o = new UnityEngine.Object();
            duk_bind_native(ctx, -1, o);
            DuktapeDLL.duk_pop(ctx);
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        static int Destroy(IntPtr ctx)
        {
            Object arg1;
            duk_get_classvalue(ctx, 0, out arg1);
            DuktapeDLL.duk_pop(ctx); // pop this
            Object.Destroy(arg1);
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        static int Foo(IntPtr ctx)
        {
            Debug.Log("Object.Foo");
            var go = UnityEngine.GameObject.Find("testing");
            IntPtr heapptr;
            if (DuktapeVM.GetObjectCache(ctx).TryGetJSValue(go, out heapptr))
            {
                DuktapeDLL.duk_push_heapptr(ctx, heapptr);
            }
            else
            {
                DuktapeDLL.duk_push_null(ctx);
            }
            return 1;
        }

        public static void reg(IntPtr ctx)
        {
            duk_begin_namespace(ctx, "UnityEngine");
            duk_begin_class(ctx, "Object", typeof(UnityEngine.Object), ctor);
            duk_add_method(ctx, "Destroy", Destroy, true);
            duk_add_method(ctx, "Foo", Foo, false);
            duk_end_class(ctx);
            duk_end_namespace(ctx);
        }
    }
}
