using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public class Sample : MonoBehaviour
{
    DuktapeVM vm = new DuktapeVM();

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
        vm.EvalFile("console-minimal.js");
        vm.EvalMain("main.js");
    }

    void OnDestroy()
    {
        vm.Destroy();
        vm = null;
    }
}
