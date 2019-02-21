using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    // 所有具有相同参数数量的方法变体 (最少参数的情况下)
    public class MethodVariant
    {
        public int argc; // 最少参数数要求
        public List<MethodInfo> plainMethods = new List<MethodInfo>();
        public List<MethodInfo> varargMethods = new List<MethodInfo>();

        // 是否包含变参方法
        public bool isVararg
        {
            get { return varargMethods.Count > 0; }
        }

        public int count
        {
            get { return plainMethods.Count + varargMethods.Count; }
        }

        public MethodVariant(int argc)
        {
            this.argc = argc;
        }

        public void Add(MethodInfo methodInfo, bool isVararg)
        {
            if (isVararg)
            {
                this.varargMethods.Add(methodInfo);
            }
            else
            {
                this.plainMethods.Add(methodInfo);
            }
        }
    }

    public class MethodVariantComparer : IComparer<int>
    {
        public int Compare(int a, int b)
        {
            return a < b ? 1 : (a == b ? 0 : -1);
        }
    }

    public class MethodVariants
    {
        private int _count = 0;

        // 按照参数数逆序排序所有变体
        public SortedDictionary<int, MethodVariant> variants = new SortedDictionary<int, MethodVariant>(new MethodVariantComparer());

        public string name;

        public int count
        {
            get { return _count; }
        }

        public MethodVariants(string name)
        {
            this.name = name;
        }

        public static bool IsVarargMethod(ParameterInfo[] parameters)
        {
            return parameters.Length > 0 && parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
        }

        public void Add(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var argc = parameters.Length;
            var isVararg = IsVarargMethod(parameters);
            MethodVariant variants;
            if (isVararg)
            {
                argc--;
            }
            if (!this.variants.TryGetValue(argc, out variants))
            {
                variants = new MethodVariant(argc);
                this.variants.Add(argc, variants);
            }
            _count++;
            variants.Add(methodInfo, isVararg);
        }
    }

    public class TypeBindingInfo
    {
        public BindingManager bindingManager;
        public Type type;
        public Dictionary<string, MethodVariants> methods = new Dictionary<string, MethodVariants>();
        public Dictionary<string, MethodVariants> staticMethods = new Dictionary<string, MethodVariants>();
        public Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
        public Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();

        public Assembly Assembly
        {
            get { return type.Assembly; }
        }

        public string Namespace
        {
            get { return type.Namespace; }
        }

        public string FullName
        {
            get { return type.FullName; }
        }

        public string Name
        {
            get { return type.Name; }
        }

        public bool IsEnum
        {
            get { return type.IsEnum; }
        }

        public string JSBindingClassName
        {
            get { return type.FullName.Replace(".", "_"); }
        }

        public TypeBindingInfo(BindingManager bindingManager, Type type)
        {
            this.bindingManager = bindingManager;
            this.type = type;
        }

        // 将类型名转换成简单字符串 (比如用于文件名)
        public string GetFileName()
        {
            var filename = type.FullName.Replace(".", "_");
            return filename;
        }

        public bool IsPropertyMethod(MethodInfo methodInfo)
        {
            var name = methodInfo.Name;
            if (name.Length > 4 && (name.StartsWith("set_") || name.StartsWith("get_")))
            {
                PropertyInfo prop;
                if (properties.TryGetValue(name.Substring(4), out prop))
                {
                    return prop.GetMethod == methodInfo || prop.SetMethod == methodInfo;
                }
            }
            return false;
        }

        public void AddField(FieldInfo fieldInfo)
        {
            var name = fieldInfo.Name;
            fields.Add(name, fieldInfo);
        }

        public void AddProperty(PropertyInfo propInfo)
        {
            try
            {
                var name = propInfo.Name;
                properties.Add(name, propInfo);
            }
            catch (Exception exception)
            {
                bindingManager.Error("AddProperty failed {0} @ {1}\n{2}", propInfo, type, exception);
            }
        }

        public void AddMethod(MethodInfo methodInfo)
        {
            var prefix = methodInfo.IsStatic ? "BindStatic_" : "Bind_";
            var name = prefix + methodInfo.Name;
            var group = methodInfo.IsStatic ? staticMethods : methods;
            MethodVariants overrides;
            if (!group.TryGetValue(name, out overrides))
            {
                overrides = new MethodVariants(methodInfo.Name);
                group.Add(name, overrides);
            }
            overrides.Add(methodInfo);
        }

        public bool IsExtensionMethod(MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(ExtensionAttribute), false);
        }

        public void Collect()
        {

            // 收集所有 字段,属性,方法
            var fields = type.GetFields();
            foreach (var field in fields)
            {
                AddField(field);
            }
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                AddProperty(property);
            }
            var methods = type.GetMethods();
            foreach (var method in methods)
            {
                var name = method.Name;
                if (IsPropertyMethod(method))
                {
                }
                else
                {
                    do
                    {
                        if (IsExtensionMethod(method))
                        {
                            var targetType = method.GetParameters()[0].ParameterType;
                            var targetInfo = bindingManager.GetExportedType(targetType);
                            if (targetInfo != null)
                            {
                                targetInfo.AddMethod(method);
                                break;
                            }
                            // else fallthrough (as normal static method)
                        }
                        AddMethod(method);
                    } while (false);
                }
            }
        }
    }
}
