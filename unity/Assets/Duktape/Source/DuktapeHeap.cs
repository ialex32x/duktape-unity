using System;

namespace Duktape
{
    public class DuktapeContext
    {
        public const string HEAP_STASH_PROPS_REGISTRY = "registry";

        public readonly IntPtr ctx;

        public DuktapeContext(IntPtr ctx)
        {
            this.ctx = ctx;
        }

        public int duk_ref()
        {
            DuktapeDLL.duk_push_heap_stash(this.ctx);
            DuktapeDLL.duk_get_prop_string(this.ctx, -1, HEAP_STASH_PROPS_REGISTRY);
            throw new NotImplementedException();
            // return 0;
        }
    }

    public class DuktapeHeap
    {
        public readonly IntPtr ctx;

        public DuktapeHeap()
        {
            this.ctx = DuktapeDLL.duk_create_heap_default();
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
            DuktapeDLL.duk_push_c_function(this.ctx, DuktapeHeap.TestFoo, 0);
            DuktapeDLL.duk_put_prop_string(this.ctx, -2, "foo");
            DuktapeDLL.duk_put_prop_string(this.ctx, -2, "prototype");
            DuktapeDLL.duk_push_c_function(this.ctx, DuktapeHeap.TestStaticFoo, 0);
            DuktapeDLL.duk_put_prop_string(this.ctx, -2, "static_foo");
            DuktapeDLL.duk_put_prop_string(this.ctx, -2, "Test");
            DuktapeDLL.duk_pop(this.ctx);
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int Print(IntPtr ctx)
        {
            UnityEngine.Debug.LogFormat("Print {0}", DuktapeDLL.duk_get_top(ctx));
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int TestConstructor(IntPtr ctx)
        {
            UnityEngine.Debug.Log("TestConstructor");
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int TestFoo(IntPtr ctx)
        {
            UnityEngine.Debug.Log("TestFoo");
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int TestStaticFoo(IntPtr ctx)
        {
            UnityEngine.Debug.Log("TestStaticFoo");
            return 0;
        }

        public void Destroy()
        {
            DuktapeDLL.duk_destroy_heap(this.ctx);
        }
    }
}