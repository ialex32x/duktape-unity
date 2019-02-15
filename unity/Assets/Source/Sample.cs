using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public class Sample : MonoBehaviour
{
    DuktapeVM vm = new DuktapeVM();

    public void Test()
    {
        var ctx = vm.context.rawValue;
        DuktapeDLL.duk_push_global_object(ctx);
        DuktapeDLL.duk_push_c_function(ctx, TestConstructor, 0); // ctor, 
        DuktapeDLL.duk_push_object(ctx); // ctor, prototype
        DuktapeDLL.duk_push_c_function(ctx, TestFinalizer, 1);
        DuktapeDLL.duk_set_finalizer(ctx, -2);
        DuktapeDLL.duk_push_c_function(ctx, TestFoo, 0);
        DuktapeDLL.duk_put_prop_string(ctx, -2, "foo"); 
        DuktapeDLL.duk_put_prop_string(ctx, -2, "prototype"); // ctor
        // DuktapeDLL.duk_set_prototype(ctx, -2);
        DuktapeDLL.duk_push_c_function(ctx, TestStaticFoo, 0);
        DuktapeDLL.duk_put_prop_string(ctx, -2, "static_foo");
        DuktapeDLL.duk_put_prop_string(ctx, -2, "Test");
        DuktapeDLL.duk_pop(ctx);
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

    void Awake()
    {
        vm.Initialize(new FakeFileSystem(), null, null);

        var ctx = vm.context.rawValue;
        Duktape.DuktapeDLL.duk_push_global_object(ctx);
        Duktape.UnityEngine_GameObject.reg(ctx);
        Duktape.DuktapeDLL.duk_pop(ctx);
    }

    // Start is called before the first frame update
    void Start()
    {
        vm.AddSearchPath("Assets/Scripts/polyfills");
        vm.AddSearchPath("Assets/Scripts/Generated");

        Test();
        vm.EvalFile("console-minimal.js");
        vm.EvalMain("main.js");
    }

    void OnDestroy()
    {
        vm.Destroy();
        vm = null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
