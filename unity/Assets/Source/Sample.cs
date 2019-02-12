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
        DuktapeDLL.duk_push_string(heap.ctx, string.Format("test {0} {1} native varg", "hello", 123));
        var str = DuktapeAux.duk_to_string(heap.ctx, -1);
        Debug.LogFormat("testcase[1]: {0} ## {1}", str, DuktapeDLL.duk_get_top(heap.ctx));
        DuktapeDLL.duk_pop(heap.ctx);

        heap.EvalFile("console-minimal.js");
        heap.EvalMain("main.js");
        // heap.EvalFile("test.js");
//         var err = DuktapeDLL.duk_peval_string_noresult(heap.ctx, @"
// // if (typeof console === 'undefined') {
// //     Object.defineProperty(this, 'console', {
// //         value: {}, writable: true, enumerable: false, configurable: true
// //     });
// // }
// // if (typeof console.log === 'undefined') {
// //     (function () {
// //         var origPrint = print;  // capture in closure in case changed later
// //         Object.defineProperty(this.console, 'log', {
// //             value: function () {
// //                 var strArgs = Array.prototype.map.call(arguments, function (v) { return String(v); });
// //                 origPrint(Array.prototype.join.call(strArgs, ' '));
// //             }, writable: true, enumerable: false, configurable: true
// //         });
// //     })();
// // }

// console.log('haha');
// var test = new Test();
// test.foo();
// // test = undefined;
// Test.static_foo();
// // print(typeof Duptake);
// // var pig = require('./game/base/pig');
// // print(pig);
//         ");
//         if (err != 0)
//         {
//             Debug.LogError("error");
//         }
//         DuktapeDLL.duk_gc(heap.ctx, DuktapeDLL.DUK_GC_COMPACT);
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
