using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public class Sample : MonoBehaviour
{
    DuktapeHeap heap = new DuktapeHeap();

    // Start is called before the first frame update
    void Start()
    {
        heap.Test();
        var err = DuktapeDLL.duk_peval_string_noresult(heap.ctx, @"
print(123);
var test = new Test();
test.foo();
print(typeof Duptake);
        ");
        if (err != 0)
        {
            Debug.LogError("error");
        }
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
