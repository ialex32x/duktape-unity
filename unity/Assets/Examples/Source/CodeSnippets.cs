using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

public class CodeSnippets : MonoBehaviour
{
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

    public static void DelegateFooCompatible(string x, string a, string b)
    {
        Debug.Log($"{x}, {a}, {b}");
    }

    void testTypes()
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
    }

    private delegate void DelegateTest1<T>(int i32, out T f32);

    DelegateTest1<float> dt1;

    void dt_instance(int i32, out float f32)
    {
        f32 = i32 + 123.4f;
    }

    void testDelegates()
    {
        dt1 = dt_instance;
        float fval;
        dt1.Invoke(-123, out fval);
        Debug.Log(fval);
        // SampleStruct.X("", 1);
        // SampleStruct.X("");
        var compatible = this.GetType().GetMethod("DelegateFooCompatible");
        var call = Delegate.CreateDelegate(typeof(SampleNamespace.SampleClass.DelegateFoo), "test", compatible, true);
        call.DynamicInvoke("a", "b");
    }

    void testGenericTypes()
    {
        var g = typeof(DelegateTest1<float>);
        var purename = g.Namespace + "." + g.Name.Substring(0, g.Name.Length - 2);
        var gargs = g.GetGenericArguments();
        purename += "<";
        for (var i = 0; i < gargs.Length; i++)
        {
            var garg = gargs[i];
            purename += garg.Namespace + "." + garg.Name;
            if (i != gargs.Length - 1)
            {
                purename += ", ";
            }
        }
        purename += ">";
        Debug.Log(purename);
    }

    void testEmptyArray()
    {
        var a = new int[0];
        var b = new int[0];
        Debug.Log($"{a == b}");
    }

    void testVarargs(object o)
    {
    }

    void testVarargs(params int[] n)
    {
    }

    class InnerType
    { }

    void testInnerType(Type type)
    {
        Debug.Log($"{type.DeclaringType}");
    }

    void testPointer(Type type)
    {
        Debug.Log($"type {type} .IsPointer: {type.IsPointer}");
    }

    public const string constant = "test";
    public const int constantInt = 123;
    public readonly float constantSingle = 123.45f;
    void testConstant(FieldInfo field)
    {
        Debug.Log($"IsLiteral: {field.IsLiteral}");
        if (field.IsLiteral)
        {
            Debug.Log($"GetRawConstantValue: {field.GetRawConstantValue()}");
        }
    }

    public static void testTypeByRefMethod(ref string p)
    { }

    void testTypeByRef(MethodInfo method)
    {
        var ps = method.GetParameters();
        foreach (var p in ps)
        {
            Debug.Log($"IsByRef: {p.ParameterType.IsByRef}");
            Debug.Log($"UnderlyingSystemType: {p.ParameterType.UnderlyingSystemType}");
            Debug.Log($"BaseType: {p.ParameterType.BaseType}");
            Debug.Log($"DeclaringType: {p.ParameterType.DeclaringType}");
            Debug.Log($"GetElementType: {p.ParameterType.GetElementType()}");
        }
    }

    void testConcreteSubType()
    {
        var gb = new StringGB();
        gb.Foo("fff");
        var type = typeof(StringGB);
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        var methods = type.GetMethods(bindingFlags);
        foreach (var m in methods)
        {
            Debug.Log($"Method: {m} ({m.DeclaringType})");
        }
        var baseType = type.BaseType;
        Debug.Log($"IsConstructedGenericType: {baseType.IsConstructedGenericType}");
        var jsTypeAttr = baseType.GetGenericTypeDefinition().IsDefined(typeof(Duktape.JSTypeAttribute), false);
        Debug.Log($"GetGenericTypeDefinition: {baseType.GetGenericTypeDefinition()} ({jsTypeAttr})");
    }

    void Awake()
    {
        // testTypeByRef(GetType().GetMethod("testTypeByRefMethod"));
        // testConstant(GetType().GetField("constant"));
        // testConstant(GetType().GetField("constantInt"));
        // testConstant(GetType().GetField("constantSingle"));
        // testPointer(typeof(long*));
        // testPointer(typeof(IntPtr));
        // testInnerType(typeof(InnerType));
        // testVarargs(null);
        // testEmptyArray();
        // testTypes();
        // testDelegates();
        // testGenericTypes();
        testConcreteSubType();
    }
}