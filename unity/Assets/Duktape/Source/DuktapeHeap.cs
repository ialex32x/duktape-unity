using System;
using UnityEngine;

namespace Duktape
{
    public class DuktapeHeap
    {
        public readonly IntPtr ctx;

        public DuktapeHeap()
        {
            this.ctx = DuktapeDLL.duk_create_heap_default();
            DuktapeAux.duk_open(this.ctx);
            DuktapeAux.duk_open_module(this.ctx);
        }

        public void Test()
        {
            DuktapeDLL.duk_push_global_object(this.ctx);
            DuktapeDLL.duk_push_c_function(this.ctx, DuktapeHeap.Print, DuktapeDLL.DUK_VARARGS);
            DuktapeDLL.duk_put_prop_string(this.ctx, -2, "print");
            DuktapeDLL.duk_pop(this.ctx);

            DuktapeDLL.duk_push_global_object(this.ctx);
            DuktapeDLL.duk_push_c_function(this.ctx, DuktapeHeap.TestConstructor, 0);
            DuktapeDLL.duk_push_object(this.ctx);
            DuktapeDLL.duk_push_c_function(this.ctx, DuktapeHeap.TestFinalizer, 1);
            DuktapeDLL.duk_set_finalizer(this.ctx, -2);
            DuktapeDLL.duk_push_c_function(this.ctx, DuktapeHeap.TestFoo, 0);
            DuktapeDLL.duk_put_prop_string(this.ctx, -2, "foo");
            DuktapeDLL.duk_put_prop_string(this.ctx, -2, "prototype");
            // DuktapeDLL.duk_set_prototype(this.ctx, -2);
            DuktapeDLL.duk_push_c_function(this.ctx, DuktapeHeap.TestStaticFoo, 0);
            DuktapeDLL.duk_put_prop_string(this.ctx, -2, "static_foo");
            DuktapeDLL.duk_put_prop_string(this.ctx, -2, "Test");
            DuktapeDLL.duk_pop(this.ctx);

            DuktapeDLL.duk_push_object(this.ctx);
            var ref1 = DuktapeAux.duk_ref(this.ctx);
            var ref2 = DuktapeAux.duk_ref(this.ctx);
            DuktapeAux.duk_unref(this.ctx, ref1);
            var ref3 = DuktapeAux.duk_ref(this.ctx);
            DuktapeAux.duk_unref(this.ctx, ref2);
            DuktapeAux.duk_unref(this.ctx, ref3);
            var ref4 = DuktapeAux.duk_ref(this.ctx);
            DuktapeAux.duk_unref(this.ctx, ref4);
            Debug.LogFormat("test ref/unref {0}, {1}, {2}, {3}", ref1, ref2, ref3, ref4);
            DuktapeDLL.duk_pop(this.ctx);
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int TestFinalizer(IntPtr ctx)
        {
            DuktapeDLL.duk_get_prop_string(ctx, 0, DuktapeDLL.DUK_HIDDEN_SYMBOL("native"));
            var number = DuktapeDLL.duk_get_number(ctx, -1);
            Debug.LogFormat("TestFinalizer {0}", number);
            DuktapeDLL.duk_pop(ctx); // pop native 
            Debug.LogFormat("TestFinalizer {0}", DuktapeDLL.duk_get_top(ctx));
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int Print(IntPtr ctx)
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

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int TestConstructor(IntPtr ctx)
        {
            Debug.LogFormat("TestConstructor top {0}", DuktapeDLL.duk_get_top(ctx));
            DuktapeDLL.duk_push_this(ctx);
            DuktapeDLL.duk_push_number(ctx, 123);
            DuktapeDLL.duk_put_prop_string(ctx, -2, DuktapeDLL.DUK_HIDDEN_SYMBOL("native"));
            DuktapeDLL.duk_pop(ctx);
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int TestFoo(IntPtr ctx)
        {
            Debug.Log("TestFoo");
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int TestStaticFoo(IntPtr ctx)
        {
            Debug.Log("TestStaticFoo");
            return 0;
        }

        public void Destroy()
        {
            DuktapeDLL.duk_destroy_heap(this.ctx);
        }
    }
}