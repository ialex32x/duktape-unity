using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public class Sample : MonoBehaviour
{
    [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.DuktapeCSFunction))]
    public static int Foo(IntPtr ctx)
    {
        Debug.Log("sample.foo");
        return 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        var ctx = DuktapeDLL.duk_create_heap_default();
        DuktapeDLL.duk_push_global_object(ctx);
        DuktapeDLL.duk_push_c_function(ctx, Sample.Foo, DuktapeDLL.DUK_VARARGS);
        DuktapeDLL.duk_put_prop_string(ctx, -2, "foo");
        DuktapeDLL.duk_pop(ctx);
        DuktapeDLL.duk_peval_string_noresult(ctx, "foo()");
        DuktapeDLL.duk_destroy_heap(ctx);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
