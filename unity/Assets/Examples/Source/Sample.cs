using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public class Sample : MonoBehaviour, Duktape.IDuktapeListener
{
    public string launchScript = "main.js";

    DuktapeVM vm = new DuktapeVM();

    public void OnTypesBinding(DuktapeVM vm)
    {
        // 此处进行手工导入
        // var ctx = vm.context.rawValue;
        // xxx.reg(ctx);
    }

    public void OnBindingError(DuktapeVM vm, Type type)
    {
    }

    public void OnProgress(DuktapeVM vm, int step, int total)
    {
    }

    public void OnLoaded(DuktapeVM vm)
    {
        vm.AddSearchPath("Assets/Examples/Scripts/Generated");
        vm.EvalMain(launchScript);
    }

    void Awake()
    {
        vm.Initialize(new FakeFileSystem(), this);
    }

    void OnDestroy()
    {
        // DuktapeDebugger.Shutdown();
        vm.Destroy();
        vm = null;
    }
}
