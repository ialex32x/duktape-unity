using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class TopLevelCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public TopLevelCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            this.cg.csharp.AppendLine("// {0} {1}", Environment.UserName, DateTime.Now);
            this.cg.csharp.AppendLine("using System;");
            this.cg.csharp.AppendLine("using System.Collections.Generic;");
            this.cg.csharp.AppendLine();
        }

        public void Dispose()
        {
        }
    }

    public class NamespaceCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected string ns;
        protected Type type;

        public NamespaceCodeGen(CodeGenerator cg, string ns, Type type)
        {
            this.cg = cg;
            this.ns = ns;
            this.type = type;
            if (!string.IsNullOrEmpty(ns))
            {
                this.cg.csharp.AppendLine("namespace {0} {{", ns);
                this.cg.csharp.AddTabLevel();
            }
            this.cg.csharp.AppendLine("using Duktape;");
            // cg.csharp.AppendLine("using UnityEngine;");
            this.cg.typescript.AppendLine("declare namespace {0} {{", type.Namespace);
            this.cg.typescript.AddTabLevel();
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(ns))
            {
                this.cg.csharp.DecTabLevel();
                this.cg.csharp.AppendLine("}");
            }
            this.cg.typescript.DecTabLevel();
            this.cg.typescript.AppendLine("}");
        }
    }

    public class FieldCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected string name;
        protected FieldInfo fieldInfo;

        public FieldCodeGen(CodeGenerator cg, string name, FieldInfo fieldInfo)
        {
            this.cg = cg;
            this.name = name;
            this.fieldInfo = fieldInfo;

            this.cg.csharp.AppendLine("[AOT.MonoPInvokeCallbackAttribute(typeof(duk_c_function))]");
            this.cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.csharp.AppendLine("public static int {0}(IntPtr ctx)", name);
            this.cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
        }

        public void Dispose()
        {
            this.cg.csharp.AppendLine("return 0");
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }
    }

    public class MethodCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected string name;
        protected List<MethodInfo> methodInfos; // 所有重载

        public MethodCodeGen(CodeGenerator cg, string name, List<MethodInfo> methodInfos)
        {
            this.cg = cg;
            this.name = name;
            this.methodInfos = methodInfos;

            cg.csharp.AppendLine("[AOT.MonoPInvokeCallbackAttribute(typeof(duk_c_function))]");
            cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            cg.csharp.AppendLine("public static int {0}(IntPtr ctx)", name);
            cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
            cg.csharp.AppendLine("// {0} overrides", methodInfos.Count);
        }

        public virtual void Dispose()
        {
            cg.csharp.AppendLine("return 0");
            this.cg.csharp.DecTabLevel();
            cg.csharp.AppendLine("}");
        }
    }

    public class RegFuncCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public RegFuncCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            cg.csharp.AppendLine("public static int reg(IntPtr ctx)");
            cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
        }

        public virtual void Dispose()
        {
            this.cg.csharp.DecTabLevel();
            cg.csharp.AppendLine("}");
        }
    }

    public class TypeCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected Type type;

        public TypeCodeGen(CodeGenerator cg, Type type)
        {
            this.cg = cg;
            this.type = type;
            this.cg.csharp.AppendLine("[{0}]", typeof(JSTypeAttribute).Name);
            this.cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.csharp.AppendLine("public class {0} : {1} {{", GetTypeName(type), typeof(DuktapeBinding).Name);
            this.cg.csharp.AddTabLevel();
            this.cg.typescript.AppendLine("class {0} {{", type.Name);
            this.cg.typescript.AddTabLevel();
        }

        public string GetTypeName(Type type)
        {
            return type.FullName.Replace(".", "_");
        }

        public virtual void Dispose()
        {
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
            this.cg.typescript.DecTabLevel();
            this.cg.typescript.AppendLine("}");
        }
    }

    public class ClassCodeGen : TypeCodeGen
    {
        private Dictionary<string, List<MethodInfo>> methods = new Dictionary<string, List<MethodInfo>>();
        private Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();
        private Dictionary<string, FieldInfo> fields = new Dictionary<string, FieldInfo>();

        public ClassCodeGen(CodeGenerator cg, Type type)
        : base(cg, type)
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
                    AddMethod(method);
                }
            }

            // 生成函数体
            foreach (var kv in this.methods)
            {
                using (new MethodCodeGen(cg, kv.Key, kv.Value))
                {

                }
            }
            foreach (var kv in this.fields)
            {
                using (new FieldCodeGen(cg, kv.Key, kv.Value))
                {
                }
            }
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
            var name = propInfo.Name;
            properties.Add(name, propInfo);
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

        // 获取指定类型在 ts 中的声明名称
        protected string GetTypeScriptName(Type type)
        {
            if (this.cg.bindingManager.IsExported(type))
            {
                return type.FullName;
            }
            return "any";
        }

        public override void Dispose()
        {
            using (new RegFuncCodeGen(cg))
            {
                cg.csharp.AppendLine("duk_begin_namespace(ctx, \"{0}\");", type.Namespace);
                cg.csharp.AppendLine("duk_begin_class(ctx, typeof({0}), ctor);", type.FullName);
                foreach (var kv in methods)
                {
                    var methodInfo = kv.Value[0];
                    var regName = methodInfo.Name;
                    var funcName = kv.Key;
                    var bStatic = methodInfo.IsStatic;
                    cg.csharp.AppendLine("duk_put_method(ctx, \"{0}\", {1}, {2});", regName, funcName, bStatic ? "true" : "false");
                }
                foreach (var kv in properties)
                {
                    var propertyInfo = kv.Value;
                    var bStatic = false;
                    cg.csharp.AppendLine("duk_put_property(ctx, \"{0}\", {1}, {2}, {3});",
                        propertyInfo.Name,
                        propertyInfo.CanRead ? propertyInfo.GetMethod.Name : "null",
                        propertyInfo.CanWrite ? propertyInfo.SetMethod.Name : "null",
                        bStatic ? "true" : "false");

                    var tsPropertyPrefix = propertyInfo.CanWrite ? "" : "readonly ";
                    var tsPropertyType = GetTypeScriptName(propertyInfo.PropertyType);
                    cg.typescript.AppendLine("{0}{1}: {2}", tsPropertyPrefix, propertyInfo.Name, tsPropertyType);
                }
                foreach (var kv in fields)
                {
                    var name = kv.Key;
                    var fieldInfo = kv.Value;
                    var bStatic = fieldInfo.IsStatic ? "true" : "false";
                    cg.csharp.AppendLine("duk_put_field(ctx, \"{0}\", {1}, {2});", fieldInfo.Name, name, bStatic);
                    var tsPropertyPrefix = fieldInfo.IsStatic ? "static " : "";
                    var tsPropertyType = GetTypeScriptName(fieldInfo.FieldType);
                    cg.typescript.AppendLine("{0}{1}: {2}", tsPropertyPrefix, fieldInfo.Name, tsPropertyType);
                }
                cg.csharp.AppendLine("duk_end_class(ctx);");
                cg.csharp.AppendLine("duk_end_namespace(ctx);");
            }
            base.Dispose();
        }
    }

    public class EnumCodeGen : TypeCodeGen
    {
        public EnumCodeGen(CodeGenerator cg, Type type)
        : base(cg, type)
        {
        }
    }

    public class PlatformCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public PlatformCodeGen(CodeGenerator cg)
        {
            this.cg = cg;

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    cg.csharp.AppendLineL("#if UNITY_ANDROID");
                    break;
                case BuildTarget.iOS:
                    cg.csharp.AppendLineL("#if UNITY_IOS");
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    cg.csharp.AppendLineL("#if UNITY_STANDALONE_WIN");
                    break;
                case BuildTarget.StandaloneOSX:
                    cg.csharp.AppendLineL("#if UNITY_STANDALONE_OSX");
                    break;
                default:
                    cg.csharp.AppendLineL("#if false");
                    break;
            }
        }


        public void Dispose()
        {
            cg.csharp.AppendLineL("#endif");
        }
    }
}