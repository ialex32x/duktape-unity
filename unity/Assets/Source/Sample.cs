using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public enum SampleEnum
{
    a, b, c
}

public struct SampleStruct
{
    public int a;

    public void Foo(List<int> list)
    {

    }
}

public class Sample : MonoBehaviour
{
    DuktapeVM vm = new DuktapeVM();

    void checking<T>(T o)
    {
        var t = typeof(T);
        var sb = new System.Text.StringBuilder();
        sb.AppendFormat("# type: {0}\n", t.FullName);
        sb.AppendFormat("# basic\n");
        sb.AppendFormat("    IsValueType: {0}\n", t.IsValueType);
        sb.AppendFormat("    IsPrimitive: {0}\n", t.IsPrimitive);
        sb.AppendFormat("    IsEnum: {0}\n", t.IsEnum);
        if (t.IsGenericType)
        {
            sb.AppendFormat("# generic\n");
            sb.AppendFormat("    GenericTypeArguments {0}\n", t.GenericTypeArguments);
        }
        if (t.IsArray)
        {
            sb.AppendFormat("# array\n");
            sb.AppendFormat("    GetElementType: {0}\n", t.GetElementType());
        }
        Debug.Log(sb.ToString());
    }

    void Awake()
    {
        checking(1);
        checking("abc");
        checking(SampleEnum.a);
        checking(new SampleStruct());
        checking((SampleStruct?)null);
        checking(new Nullable<int>());
        checking(new int[] { 1, 2, 3 });
        checking(new int?[] { 1, 2, 3 });
        checking(new List<SampleStruct>());
        checking(new List<SampleStruct?>());

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

    void Update()
    {
        vm.Update(Time.deltaTime);
    }

    void OnDestroy()
    {
        vm.Destroy();
        vm = null;
    }
}
