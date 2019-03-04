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

    public class MethodBindingInfo
    {
        private int _count = 0;

        // 按照参数数逆序排序所有变体
        public SortedDictionary<int, MethodVariant> variants = new SortedDictionary<int, MethodVariant>(new MethodVariantComparer());

        public string name; // 绑定代码名

        public string regName; // 导出名

        public int count
        {
            get { return _count; }
        }

        public MethodBindingInfo(bool bStatic, string regName)
        {
            this.name = (bStatic ? "BindStatic_" : "Bind_") + regName;
            this.regName = regName;
        }

        public static bool IsVarargMethod(ParameterInfo[] parameters)
        {
            return parameters.Length > 0 && parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
        }

        public void Add(MethodInfo methodInfo)
        {
            var parameters = methodInfo.GetParameters();
            var args = parameters.Length;
            var isVararg = IsVarargMethod(parameters);
            MethodVariant variants;
            if (isVararg)
            {
                args--;
            }
            if (!this.variants.TryGetValue(args, out variants))
            {
                variants = new MethodVariant(args);
                this.variants.Add(args, variants);
            }
            _count++;
            variants.Add(methodInfo, isVararg);
        }
    }

    public class PropertyBindingInfo
    {
        public string getterName; // 绑定代码名
        public string setterName;
        public string regName; // js 注册名
        public PropertyInfo propertyInfo;

        public PropertyBindingInfo(PropertyInfo propertyInfo)
        {
            this.propertyInfo = propertyInfo;
            this.getterName = propertyInfo.CanRead ? "BindRead_" + propertyInfo.Name : null;
            this.setterName = propertyInfo.CanWrite ? "BindWrite_" + propertyInfo.Name : null;
            this.regName = TypeBindingInfo.GetNamingAttribute(propertyInfo) ?? propertyInfo.Name;
        }
    }

    public class FieldBindingInfo
    {
        public string getterName = null; // 绑定代码名
        public string setterName = null;
        public string regName = null; // js 注册名

        public FieldInfo fieldInfo;

        public bool isStatic { get { return fieldInfo.IsStatic; } }

        public FieldBindingInfo(FieldInfo fieldInfo)
        {
            if (fieldInfo.IsStatic)
            {
                this.getterName = "BindStaticRead_" + fieldInfo.Name;
                if (!fieldInfo.IsInitOnly && !fieldInfo.IsLiteral)
                {
                    this.setterName = "BindStaticWrite_" + fieldInfo.Name;
                }
            }
            else
            {
                this.getterName = "BindRead_" + fieldInfo.Name;
                if (!fieldInfo.IsInitOnly && !fieldInfo.IsLiteral)
                {
                    this.setterName = "BindWrite_" + fieldInfo.Name;
                }
            }
            this.regName = TypeBindingInfo.GetNamingAttribute(fieldInfo) ?? fieldInfo.Name;
            this.fieldInfo = fieldInfo;
        }
    }

    public class ConstructorBindingInfo
    {
        public string name = "BindConstructor";
        public string regName = "constructor";
        public Type decalringType;
        public List<ConstructorInfo> variants = new List<ConstructorInfo>();

        public bool hasValid
        {
            get
            {
                if (decalringType.IsValueType && !decalringType.IsPrimitive)
                {
                    return true; // default constructor for struct
                }
                return variants.Count > 0;
            }
        }

        public ConstructorBindingInfo(Type decalringType)
        {
            this.decalringType = decalringType;
        }

        public void Add(ConstructorInfo constructorInfo)
        {
            this.variants.Add(constructorInfo);
        }
    }

    public class TypeBindingInfo
    {
        public BindingManager bindingManager;
        public Type type;

        public string name; // 绑定代码名

        public string regName; // js注册名

        public Dictionary<string, MethodBindingInfo> methods = new Dictionary<string, MethodBindingInfo>();
        public Dictionary<string, MethodBindingInfo> staticMethods = new Dictionary<string, MethodBindingInfo>();
        public Dictionary<string, PropertyBindingInfo> properties = new Dictionary<string, PropertyBindingInfo>();
        public Dictionary<string, FieldBindingInfo> fields = new Dictionary<string, FieldBindingInfo>();
        public ConstructorBindingInfo constructors;

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

        public bool IsEnum
        {
            get { return type.IsEnum; }
        }

        public static string GetNamingAttribute(MemberInfo info)
        {
            var naming = info.GetCustomAttribute(typeof(JSNamingAttribute), false) as JSNamingAttribute;
            if (naming != null && !string.IsNullOrEmpty(naming.name))
            {
                return naming.name;
            }
            return null;
        }

        public TypeBindingInfo(BindingManager bindingManager, Type type)
        {
            this.bindingManager = bindingManager;
            this.type = type;
            this.name = "DuktapeJS_" + type.FullName.Replace('.', '_').Replace('+', '_');
            this.regName = GetNamingAttribute(type) ?? type.Name;
            this.constructors = new ConstructorBindingInfo(type);
        }

        // 将类型名转换成简单字符串 (比如用于文件名)
        public string GetFileName()
        {
            var filename = type.FullName.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("`", "_");
            return filename;
        }

        public bool IsGenericMethod(MethodInfo methodInfo)
        {
            return methodInfo.GetGenericArguments().Length > 0;
        }

        public void AddField(FieldInfo fieldInfo)
        {
            try
            {
                bindingManager.CollectDelegate(fieldInfo.FieldType);
                fields.Add(fieldInfo.Name, new FieldBindingInfo(fieldInfo));
                bindingManager.Info("[AddField] {0}.{1}", type, fieldInfo.Name);
            }
            catch (Exception exception)
            {
                bindingManager.Error("AddField failed {0} @ {1}: {2}", fieldInfo, type, exception.Message);
            }
        }

        public void AddProperty(PropertyInfo propInfo)
        {
            try
            {
                bindingManager.CollectDelegate(propInfo.PropertyType);
                properties.Add(propInfo.Name, new PropertyBindingInfo(propInfo));
                bindingManager.Info("[AddProperty] {0}.{1}", type, propInfo.Name);
            }
            catch (Exception exception)
            {
                bindingManager.Error("AddProperty failed {0} @ {1}: {2}", propInfo, type, exception.Message);
            }
        }

        public void AddMethod(MethodInfo methodInfo)
        {
            var group = methodInfo.IsStatic ? staticMethods : methods;
            MethodBindingInfo overrides;
            if (!group.TryGetValue(methodInfo.Name, out overrides))
            {
                overrides = new MethodBindingInfo(methodInfo.IsStatic, TypeBindingInfo.GetNamingAttribute(methodInfo) ?? methodInfo.Name);
                group.Add(methodInfo.Name, overrides);
            }
            var parameters = methodInfo.GetParameters();
            overrides.Add(methodInfo);
            for (var i = 0; i < parameters.Length; i++)
            {
                bindingManager.CollectDelegate(parameters[i].ParameterType);
            }
            bindingManager.Info("[AddMethod] {0}.{1}", type, methodInfo);
        }

        public void AddConstructor(ConstructorInfo constructorInfo)
        {
            constructors.Add(constructorInfo);
            this.bindingManager.Info("[AddConstructor] {0}.{1}", type, constructorInfo);
        }

        public bool IsExtensionMethod(MethodInfo methodInfo)
        {
            return methodInfo.IsDefined(typeof(ExtensionAttribute), false);
        }

        // 收集所有 字段,属性,方法
        public void Collect()
        {
            var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
            var fields = type.GetFields(bindingFlags);
            foreach (var field in fields)
            {
                if (field.IsSpecialName)
                {
                    continue;
                }
                if (field.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete field: {0}", field.Name);
                    continue;
                }
                AddField(field);
            }
            var properties = type.GetProperties(bindingFlags);
            foreach (var property in properties)
            {
                if (property.IsSpecialName)
                {
                    continue;
                }
                //TODO: 索引访问
                if (property.Name == "Item")
                {
                    continue;
                }
                if (property.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete property: {0}", property.Name);
                    continue;
                }
                AddProperty(property);
            }
            var constructors = type.GetConstructors();
            foreach (var constructor in constructors)
            {
                if (constructor.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    continue;
                }
                AddConstructor(constructor);
            }
            var methods = type.GetMethods(bindingFlags);
            foreach (var method in methods)
            {
                var name = method.Name;
                if (IsGenericMethod(method))
                {
                    continue;
                }
                if (method.IsSpecialName)
                {
                    continue;
                }
                if (method.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete method: {0}", method.Name);
                    continue;
                }
                // if (IsPropertyMethod(method))
                // {
                //     continue;
                // }
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
