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
    public class MethodBaseVariant<T>
    where T : MethodBase
    {
        public int argc; // 最少参数数要求
        public List<T> plainMethods = new List<T>();
        public List<T> varargMethods = new List<T>();

        // 是否包含变参方法
        public bool isVararg
        {
            get { return varargMethods.Count > 0; }
        }

        public int count
        {
            get { return plainMethods.Count + varargMethods.Count; }
        }

        public MethodBaseVariant(int argc)
        {
            this.argc = argc;
        }

        public void Add(T methodInfo, bool isVararg)
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

    public abstract class MethodBaseBindingInfo<T>
    where T : MethodBase
    {
        public string name { get; set; }    // 绑定代码名
        public string regName { get; set; } // 导出名

        private int _count = 0;

        // 按照参数数逆序排序所有变体
        // 有相同参数数量要求的方法记录在同一个 Variant 中 (变参方法按最少参数数计算, 不计变参参数数)
        public SortedDictionary<int, MethodBaseVariant<T>> variants = new SortedDictionary<int, MethodBaseVariant<T>>(new MethodVariantComparer());

        public int count
        {
            get { return _count; }
        }

        public static bool IsVarargMethod(ParameterInfo[] parameters)
        {
            return parameters.Length > 0 && parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
        }

        public void Add(T method)
        {
            var parameters = method.GetParameters();
            var nargs = parameters.Length;
            var isVararg = IsVarargMethod(parameters);
            MethodBaseVariant<T> variants;
            if (isVararg)
            {
                nargs--;
            }
            if (!this.variants.TryGetValue(nargs, out variants))
            {
                variants = new MethodBaseVariant<T>(nargs);
                this.variants.Add(nargs, variants);
            }
            _count++;
            variants.Add(method, isVararg);
        }
    }

    public class MethodBindingInfo : MethodBaseBindingInfo<MethodInfo>
    {
        public bool isIndexer;
        public MethodBindingInfo(bool isIndexer, bool bStatic, string bindName, string regName)
        {
            this.isIndexer = isIndexer;
            this.name = (bStatic ? "BindStatic_" : "Bind_") + bindName;
            this.regName = regName;
        }
    }

    public class ConstructorBindingInfo : MethodBaseBindingInfo<ConstructorInfo>
    {
        public Type decalringType;

        // public 构造是否可用
        public bool available
        {
            get
            {
                if (decalringType.IsValueType && !decalringType.IsPrimitive && !decalringType.IsAbstract)
                {
                    return true; // default constructor for struct
                }
                return variants.Count > 0;
            }
        }

        public ConstructorBindingInfo(Type decalringType)
        {
            this.decalringType = decalringType;
            this.name = "BindConstructor";
            this.regName = "constructor";
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
            this.getterName = (propertyInfo.CanRead && propertyInfo.GetMethod != null && propertyInfo.GetMethod.IsPublic) ? "BindRead_" + propertyInfo.Name : null;
            this.setterName = (propertyInfo.CanWrite && propertyInfo.SetMethod != null && propertyInfo.SetMethod.IsPublic) ? "BindWrite_" + propertyInfo.Name : null;
            this.regName = TypeBindingInfo.GetNamingAttribute(propertyInfo);
        }
    }

    public class FieldBindingInfo
    {
        public string getterName = null; // 绑定代码名
        public string setterName = null;
        public string regName = null; // js 注册名

        public FieldInfo fieldInfo;

        public string constantValue;

        public bool isStatic { get { return fieldInfo.IsStatic; } }

        public FieldBindingInfo(FieldInfo fieldInfo)
        {
            do
            {
                if (fieldInfo.IsLiteral)
                {
                    try
                    {
                        var cv = fieldInfo.GetRawConstantValue();
                        var cvType = cv.GetType();
                        if (cvType == typeof(string))
                        {
                            constantValue = $"\"{cv}\"";
                            break;
                        }
                        if (cvType == typeof(int)
                         || cvType == typeof(uint)
                         || cvType == typeof(byte)
                         || cvType == typeof(sbyte)
                         || cvType == typeof(short)
                         || cvType == typeof(ushort)
                         || cvType == typeof(bool))
                        {
                            constantValue = $"{cv}";
                            break;
                        }
                        if (cvType == typeof(float))
                        {
                            var fcv = (float)cv;
                            if (!float.IsInfinity(fcv)
                            && !float.IsNaN(fcv))
                            {
                                constantValue = $"{cv}";
                                break;
                            }
                        }
                        // if (cvType.IsPrimitive && cvType.IsValueType)
                        // {
                        //     constantValue = $"{cv}";
                        //     break;
                        // }
                    }
                    catch (Exception)
                    {
                    }
                }
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
            } while (false);
            this.regName = TypeBindingInfo.GetNamingAttribute(fieldInfo);
            this.fieldInfo = fieldInfo;
        }
    }

    public class TypeBindingInfo
    {
        public BindingManager bindingManager;
        public Type type;
        public TypeTransform transform;
        public Type super { get { return type.BaseType; } } // 父类类型

        public string name; // 绑定代码名

        public string Namespace; // js 命名空间

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
            return info.Name;
        }

        public TypeBindingInfo(BindingManager bindingManager, Type type)
        {
            this.bindingManager = bindingManager;
            this.type = type;
            this.transform = bindingManager.GetTypeTransform(type);
            if (type.DeclaringType != null)
            {
                this.Namespace = $"{type.Namespace}.{type.DeclaringType.Name}";
            }
            else
            {
                this.Namespace = type.Namespace;
            }
            this.name = "DuktapeJS_" + type.FullName.Replace('.', '_').Replace('+', '_');
            this.regName = GetNamingAttribute(type);
            this.constructors = new ConstructorBindingInfo(type);
        }

        // 将类型名转换成简单字符串 (比如用于文件名)
        public string GetFileName()
        {
            var filename = type.FullName.Replace(".", "_").Replace("<", "_").Replace(">", "_").Replace("`", "_");
            return filename;
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
            AddMethod(methodInfo, false, null);
        }

        public void AddMethod(MethodInfo methodInfo, bool isIndexer, string renameRegName)
        {
            var group = methodInfo.IsStatic ? staticMethods : methods;
            MethodBindingInfo overrides;
            var methodName = TypeBindingInfo.GetNamingAttribute(methodInfo);
            if (!group.TryGetValue(methodName, out overrides))
            {
                overrides = new MethodBindingInfo(isIndexer, methodInfo.IsStatic, methodName, renameRegName ?? methodName);
                group.Add(methodName, overrides);
            }
            overrides.Add(methodInfo);
            CollectDelegate(methodInfo);
            bindingManager.Info("[AddMethod] {0}.{1}", type, methodInfo);
        }

        private void CollectDelegate(MethodBase method)
        {
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                bindingManager.CollectDelegate(parameters[i].ParameterType);
            }
        }

        public void AddConstructor(ConstructorInfo constructorInfo)
        {
            constructors.Add(constructorInfo);
            CollectDelegate(constructorInfo);
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
                    bindingManager.Info("skip special field: {0}", field.Name);
                    continue;
                }
                if (field.FieldType.IsPointer)
                {
                    bindingManager.Info("skip pointer field: {0}", field.Name);
                    continue;
                }
                if (field.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete field: {0}", field.Name);
                    continue;
                }
                if (bindingManager.IsTypeMemberBlocked(type, field.Name))
                {
                    bindingManager.Info("skip blocked field: {0}", field.Name);
                    continue;
                }
                AddField(field);
            }
            var properties = type.GetProperties(bindingFlags);
            foreach (var property in properties)
            {
                if (property.IsSpecialName)
                {
                    bindingManager.Info("skip special property: {0}", property.Name);
                    continue;
                }
                if (property.PropertyType.IsPointer)
                {
                    bindingManager.Info("skip pointer property: {0}", property.Name);
                    continue;
                }
                if (property.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete property: {0}", property.Name);
                    continue;
                }
                if (bindingManager.IsTypeMemberBlocked(type, property.Name))
                {
                    bindingManager.Info("skip blocked property: {0}", property.Name);
                    continue;
                }
                //TODO: 索引访问
                if (property.Name == "Item")
                {
                    if (property.CanRead && property.GetMethod != null)
                    {
                        if (BindingManager.IsUnsupported(property.GetMethod))
                        {
                            bindingManager.Info("skip unsupported get-method: {0}", property.GetMethod);
                            continue;
                        }
                        AddMethod(property.GetMethod, true, "$GetValue");
                    }
                    if (property.CanWrite && property.SetMethod != null)
                    {
                        if (BindingManager.IsUnsupported(property.SetMethod))
                        {
                            bindingManager.Info("skip unsupported set-method: {0}", property.SetMethod);
                            continue;
                        }
                        AddMethod(property.SetMethod, true, "$SetValue");
                    }
                    // bindingManager.Info("skip indexer property: {0}", property.Name);
                    continue;
                }
                AddProperty(property);
            }
            if (!type.IsAbstract)
            {
                var constructors = type.GetConstructors();
                foreach (var constructor in constructors)
                {
                    if (constructor.IsDefined(typeof(ObsoleteAttribute), false))
                    {
                        bindingManager.Info("skip obsolete constructor: {0}", constructor);
                        continue;
                    }
                    if (BindingManager.ContainsPointer(constructor))
                    {
                        bindingManager.Info("skip pointer constructor: {0}", constructor);
                        continue;
                    }
                    AddConstructor(constructor);
                }
            }
            var methods = type.GetMethods(bindingFlags);
            foreach (var method in methods)
            {
                if (BindingManager.IsGenericMethod(method))
                {
                    bindingManager.Info("skip generic method: {0}", method);
                    continue;
                }
                if (BindingManager.ContainsPointer(method))
                {
                    bindingManager.Info("skip unsafe (pointer) method: {0}", method);
                    continue;
                }
                if (method.IsSpecialName)
                {
                    bindingManager.Info("skip special method: {0}", method);
                    continue;
                }
                if (method.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    bindingManager.Info("skip obsolete method: {0}", method);
                    continue;
                }
                if (bindingManager.IsTypeMemberBlocked(type, method.Name))
                {
                    bindingManager.Info("skip blocked method: {0}", method.Name);
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
