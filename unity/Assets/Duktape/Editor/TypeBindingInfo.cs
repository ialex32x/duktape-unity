using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class TypeBindingInfo
    {
        public BindingManager bindingManager;
        public Type type;
        public Dictionary<string, List<MethodInfo>> methods = new Dictionary<string, List<MethodInfo>>();
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
            var name = "Bind" + methodInfo.Name;
            List<MethodInfo> list;
            if (!methods.TryGetValue(name, out list))
            {
                list = new List<MethodInfo>();
                methods.Add(name, list);
            }
            list.Add(methodInfo);
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
