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
        var temp = new List<Action<IntPtr>>
        {
            Duktape.UnityEngine_Object.reg,
            Duktape.UnityEngine_GameObject.reg,
        };
        vm.Initialize(new FakeFileSystem(), temp, null, () =>
        {
            vm.AddSearchPath("Assets/Scripts/polyfills");
            vm.AddSearchPath("Assets/Scripts/Generated");
            vm.EvalFile("console-minimal.js");
            vm.EvalMain("main.js");
        });
    }

    void OnDestroy()
    {
        vm.Destroy();
        vm = null;
    }
}
