using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public class Sample : MonoBehaviour
{
    DuktapeHeap heap = new DuktapeHeap();

    void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
        DuktapeAux.AddSearchPath("Assets/Scripts/polyfills");
        DuktapeAux.AddSearchPath("Assets/Scripts/Generated");

        heap.Test();
        DuktapeDLL.duk_push_string(heap.ctx, string.Format("test {0} {1} native varg", "hello", 123));
        var str = DuktapeAux.duk_to_string(heap.ctx, -1);
        Debug.LogFormat("testcase[1]: {0} ## {1}", str, DuktapeDLL.duk_get_top(heap.ctx));
        DuktapeDLL.duk_pop(heap.ctx);

        heap.EvalFile("console-minimal.js");
        heap.EvalMain("main.js");
    }

    void OnDestroy()
    {
        heap.Destroy();
        heap = null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
