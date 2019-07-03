using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public class Sample : MonoBehaviour, IDuktapeListener
{
    public string launchScript = "main.js";

    private bool _loaded;
    DuktapeVM vm = new DuktapeVM();

    public void OnBinded(DuktapeVM vm, int numRegs)
    {
        if (numRegs == 0)
        {
            throw new Exception("no type binding registered, please run <MENU>/Duktape/Generate Bindings in Unity Editor Mode before the first running of this project.");
        }
    }

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
        vm.AddSearchPath("Assets/Examples/Scripts/out");
        vm.EvalMain(launchScript);
        _loaded = true;
    }

    public void OnSourceModified()
    {
        if (_loaded)
        {
            vm.context.Invoke("OnBeforeSourceReload");
            vm.EvalMain(launchScript);
            vm.context.Invoke("OnAfterSourceReload");
        }
    }

    void Awake()
    {
        vm.Initialize(new FakeFileSystem(), this);
        {
            var start = DateTime.Now;
            var v = new Vector3(0, 0, 0);
            for (var i = 1; i < 200000; i++)
            {
                v.Set(i, i, i);
                v.Normalize();
            }
            Debug.LogFormat("c#/vector3/normalize {0}", (DateTime.Now - start).TotalSeconds);
        }
        {
            var start = DateTime.Now;
            var sum = 0;
            for (var i = 1; i < 200000; i++)
            {
                sum += i;
            }
            Debug.LogFormat("c#/number/add {0}", (DateTime.Now - start).TotalSeconds);
        }
    }

    void OnDestroy()
    {
        // DuktapeDebugger.Shutdown();
        vm.Destroy();
        vm = null;
    }
}
