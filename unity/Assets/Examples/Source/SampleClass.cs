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

// 泛型类本身不能导出
// 但 从泛型类具体化继承的类 导出时将据此判断是否自动导出 具体化的泛型类
[Duktape.JSType]
public class GB<T>
{
    public void Foo(T t)
    {
        Debug.Log($"{t.GetType()}: {t.ToString()}");
    }
}

[Duktape.JSType]
public class SomeImplicit
{
    public static SomeImplicit Accept(SomeImplicit si)
    {
        return si;
    }

    public static implicit operator int(SomeImplicit si)
    {
        return 123;
    }
}

[Duktape.JSType]
public class StringGB : GB<string>
{
}

namespace SampleNamespace
{
    [Duktape.JSType]
    public interface ISampleBase
    {
        string name { get; }
    }

    [Duktape.JSType]
    public class SampleClass : ISampleBase
    {
        public delegate void DelegateFoo(string a, string b);
        public delegate void DelegateFoo2(string a, string b);
        public delegate double DelegateFoo4(int a, float b);

        public DelegateFoo delegateFoo1;
        public DelegateFoo2 delegateFoo2;
        public DelegateFoo4 delegateFoo4;

        public Action action1;
        public Action<string> action2;
        public Action[] actions1;

        private string _name;
        private SampleEnum _sampleEnum;

        public event Action testEvent;
        public static event Action staticTestEvent;

        public string name
        {
            get { return _name; }
        }

        public SampleEnum sampleEnum { get { return _sampleEnum; } }

        public void DispatchTestEvent()
        {
            testEvent?.Invoke();
        }

        public static void DispatchStaticTestEvent()
        {
            staticTestEvent?.Invoke();
        }

        public void TestDelegate1()
        {
            if (delegateFoo1 != null)
            {
                delegateFoo1("hello", "delegate");
            }
        }

        public void TestDelegate4()
        {
            if (delegateFoo4 != null)
            {
                var r = delegateFoo4(1, 2.0f);
                Debug.Log($"TestDelegate4: {r}");
            }
        }

        public void TestVector3(Vector3 v)
        {
            Debug.Log($"TestVector3({v})");
        }

        public Type TestType1(Type type)
        {
            var ret = type ?? type.BaseType;
            Debug.Log($"[CS] TestType1({type}): {ret}");
            return ret;
        }

        public static byte[] GetBytes()
        {
            return System.Text.Encoding.UTF8.GetBytes("hello");
        }

        public static string InputBytes(byte[] bytes)
        {
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        [JSType]
        public class SampleInnerClass
        {
            public void Foo()
            {
                Debug.Log("SampleInnerClass.Foo");
            }
        }

        public static SampleClass GetSample()
        {
            return new SampleClass("SampleClass");
        }

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
            this._name = name + (additional != null ? String.Join("+", additional) : "");
        }

        public int CheckingVA(params int[] args)
        {
            return Sum(args);
        }

        public int CheckingVA2(int b, params int[] args)
        {
            return Sum(args) + b;
        }

        public void MethodOverride()
        {

        }

        public void MethodOverride(int x)
        {

        }

        public void MethodOverride(string x)
        {

        }

        public void MethodOverride(float x, float y)
        {

        }

        public void MethodOverride2(int x)
        {

        }

        [JSNaming("MethodOverride2F")]
        public void MethodOverride2(float x)
        {
        }

        public void MethodOverride3(float x)
        {
        }

        public void MethodOverride3(float x, float y)
        {
        }

        public void MethodOverride3(float x, float y, float z)
        {
        }

        public void MethodOverride3(float x, float y, float z, params int[] args)
        {
        }

        public void MethodOverride3(float x, float y, float z, object args)
        {
        }

        public void MethodOverride3(float x, float y, params int[] args)
        {
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

        public static void TestDelegate(Action act)
        {
            if (act != null)
            {
                act();
            }
        }

        public void TestDuktapeArray(DuktapeArray array)
        {
            var len = array.length;
            for (var i = 0; i < len; i++)
            {
                Debug.Log($"    #{i}: {array.GetFloatValue(i)}");
            }
        }

        public int GetPositions(int[] positions)
        {
            var size = positions != null ? positions.Length : 0;
            if (size > 2)
            {
                positions[2] = 2;
                positions[0] = 0;
                positions[1] = 1;
                return 3;
            }
            if (size > 1)
            {
                positions[0] = 0;
                positions[1] = 1;
                return 2;
            }
            if (size > 0)
            {
                positions[1] = 1;
                return 1;
            }
            return 0;
        }
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

[JSType]
[JSOmit]
public static class SampleStructExtensions
{
    public static int Foo(this SampleStruct self, int a, int b, params int[] any)
    {
        var sum = a + b;
        if (any != null)
        {
            for (var i = 0; i < any.Length; i++)
            {
                sum += any[i];
            }
        }
        return sum;
    }
}
