using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

[Duktape.JSType]
public enum SampleEnum
{
    a, b, c
}

[Duktape.JSType]
public class SampleClass
{
    public delegate void DelegateFoo(string a, string b);
    public delegate void DelegateFoo2(string a, string b);
    // 暂不支持
    // public delegate void DelegateFoo3(string a, out string b);
    public delegate void DelegateFoo4(int a, float b);

    public DelegateFoo delegateFoo1;
    public DelegateFoo2 delegateFoo2;
    // public DelegateFoo3 delegateFoo3;
    public DelegateFoo4 delegateFoo4;

    private string _name;
    private SampleEnum _sampleEnum;

    public string name
    {
        get { return _name; }
    }

    public SampleEnum sampleEnum { get { return _sampleEnum; } }

    public bool SetEnum(SampleEnum sampleEnum)
    {
        _sampleEnum = sampleEnum;
        return true;
    }

    [JSDoc(
        "简单构造函数测试",
        "@param name 测试字符串",
        "@param additional 测试可变参数"
        )]
    public SampleClass(string name, params string[] additional)
    {
        this._name = name + String.Join("+", additional);
    }

    public int CheckingVA(params int[] args)
    {
        return Sum(args);
    }

    public int CheckingVA2(int b, params int[] args)
    {
        return Sum(args) + b;
    }

    public int Sum(int[] all)
    {
        var sum = 0;
        if (all != null)
        {
            for (int i = 0, size = all.Length; i < size; i++)
            {
                sum += all[i];
            }
        }
        return sum;
    }
}

[Duktape.JSType]
public struct SampleStruct
{
    // field 
    public int field_a;

    // static field
    public static string static_field_b;

    // readonly property
    public int readonly_property_c { get; }

    // read/write property
    public float readwrite_property_d { get; set; }

    // static read/write property
    public static double static_readwrite_property_d { get; set; }

    public static string StaticMethodWithReturnAndNoOverride(Vector3 a, ref float b, out string[] c) { c = null; return a.ToString(); }

    [Duktape.JSMutable]
    public void ChangeFieldA(int a)
    {
        this.field_a += a;
    }

    // vararg method without override
    // public void VarargMethodWithoutOverride(int a, string[] b, params float[] c) { }

    // public bool MethodWithOutParameter(int a, int b, out int c) { c = a + b; return true; }

    // public bool MethodWithRefParameter(int a, int b, ref int c) { c = a + b + c; return false; }

    // public void Foo(List<int> list) { }

    // public void Foo(int a, string b) { }
    // public void Foo(int a, params string[] b) { }

    // public static void X(string a1, string a2) { }
    // public static void X(string a1, int a2) { }
    // public static void X(string a1, params int[] a2)
    // {
    //     Debug.LogFormat("X var {0}", a2.Length);
    // }
}

public static class SampleStructExtensions
{
    public static void Foo(this SampleStruct self)
    {
    }
}

public class Sample : MonoBehaviour, Duktape.IDuktapeListener
{
    DuktapeVM vm = new DuktapeVM();

    void checking<T>(T o)
    {
        var t = typeof(T);
        var sb = new System.Text.StringBuilder();
        sb.AppendFormat("# type: {0}\n", t.FullName);
        sb.AppendFormat("    value: {0}\n", o);
        sb.AppendFormat("    BaseType: {0}\n", t.BaseType);
        sb.AppendFormat("    null: {0}\n", o == null);
        sb.AppendFormat("    IsValueType: {0}\n", t.IsValueType);
        sb.AppendFormat("    IsPrimitive: {0}\n", t.IsPrimitive);
        if (t.IsEnum)
        {
            sb.AppendFormat("    # Enum\n");
            sb.AppendFormat("    GetEnumUnderlyingType: {0}\n", t.GetEnumUnderlyingType());
        }
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
        // DuktapeDebugger.CreateDebugger(vm.context.rawValue);
        vm.AddSearchPath("Assets/Scripts/polyfills");
        vm.AddSearchPath("Assets/Scripts/Generated");
        vm.EvalFile("console-minimal.js");
        vm.EvalMain("main.js");
    }

    public static void DelegateFooCompatible(string x, string a, string b)
    {
        Debug.Log($"{x}, {a}, {b}");
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
        // SampleStruct.X("", 1);
        // SampleStruct.X("");
        var compatible = this.GetType().GetMethod("DelegateFooCompatible");
        var call = Delegate.CreateDelegate(typeof(SampleClass.DelegateFoo), "test", compatible, true);
        call.DynamicInvoke("a", "b");

        vm.Initialize(new FakeFileSystem(), this);
    }

    void OnDestroy()
    {
        // DuktapeDebugger.Shutdown();
        vm.Destroy();
        vm = null;
    }
}
