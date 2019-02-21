using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public enum SampleEnum
{
    a, b, c
}

[Duktape.JSType]
public struct SampleStruct
{
    public int a;

    public void Foo(List<int> list) { }

    public void Foo(int a, string b) { }
    public void Foo(int a, params string[] b) { }

    public static void X(string a1, string a2) { }
    public static void X(string a1, int a2) { }
    public static void X(string a1, params int[] a2)
    {
        Debug.LogFormat("X var {0}", a2.Length);
    }
}

public static class SampleStructExtensions
{
    public static void Foo(this SampleStruct self)
    {
    }
}

public class Sample : MonoBehaviour
{
    public delegate void DelegateFoo();
    public DelegateFoo delegateFoo;

    DuktapeVM vm = new DuktapeVM();

    void checking<T>(T o)
    {
        var t = typeof(T);
        var sb = new System.Text.StringBuilder();
        sb.AppendFormat("# type: {0}\n", t.FullName);
        sb.AppendFormat("    value: {0}\n", o);
        sb.AppendFormat("    null: {0}\n", o == null);
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
        foreach (var method in t.GetMethods())
        {
            if (method.Name == "Foo")
            {
                sb.AppendFormat("# method.Foo: {0} {1}\n", method, method.IsDefined(typeof(System.Runtime.CompilerServices.ExtensionAttribute), false) ? "(Extension)" : "");
                foreach (var parameter in method.GetParameters())
                {
                    sb.AppendFormat("    {0} {1}\n", parameter.IsDefined(typeof(ParamArrayAttribute), false) ? "(params)" : "", parameter);
                }
            }
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
        checking(delegateFoo);
        delegateFoo = Awake;
        checking(delegateFoo);
        delegateFoo += Awake;
        delegateFoo += Update;
        checking(delegateFoo);
        SampleStruct.X("", 1);
        SampleStruct.X("");

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
