using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class NamespaceCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected string ns;

        public NamespaceCodeGen(CodeGenerator cg, string ns)
        {
            this.cg = cg;
            this.ns = ns;
            if (!string.IsNullOrEmpty(ns))
            {
                cg.AppendLine("namespace {0} {{", ns);
                this.cg.AddTabLevel();
            }
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(ns))
            {
                this.cg.DecTabLevel();
                cg.AppendLine("}");
            }
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

            cg.AppendLine("[AOT.MonoPInvokeCallbackAttribute(typeof(duk_c_function))]");
            cg.AppendLine("[UnityEngine.Scripting.Preserve]");
            cg.AppendLine("public static int {0}(IntPtr ctx)", name);
            cg.AppendLine("{");
            this.cg.AddTabLevel();
            cg.AppendLine("// {0} overrides", methodInfos.Count);
        }

        public virtual void Dispose()
        {
            cg.AppendLine("return 0");
            this.cg.DecTabLevel();
            cg.AppendLine("}");
        }
    }

    public class RegFuncCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public RegFuncCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            cg.AppendLine("public static int reg(IntPtr ctx)");
            cg.AppendLine("{");
            this.cg.AddTabLevel();
        }

        public virtual void Dispose()
        {
            this.cg.DecTabLevel();
            cg.AppendLine("}");
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
            cg.AppendLine("[{0}]", typeof(JSTypeAttribute).Name);
            cg.AppendLine("[UnityEngine.Scripting.Preserve]");
            cg.AppendLine("public class {0} : {1} {{", GetTypeName(type), typeof(DuktapeBinding).Name);
            this.cg.AddTabLevel();
        }

        public string GetTypeName(Type type)
        {
            return type.FullName.Replace(".", "_");
        }

        public virtual void Dispose()
        {
            this.cg.DecTabLevel();
            cg.AppendLine("}");
        }
    }

    public class ClassCodeGen : TypeCodeGen
    {
        private Dictionary<string, List<MethodInfo>> methods = new Dictionary<string, List<MethodInfo>>();
        private Dictionary<string, PropertyInfo> properties = new Dictionary<string, PropertyInfo>();

        public ClassCodeGen(CodeGenerator cg, Type type)
        : base(cg, type)
        {
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

            foreach (var kv in this.methods)
            {
                using (new MethodCodeGen(cg, kv.Key, kv.Value))
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


        public void AddProperty(PropertyInfo propInfo)
        {
            properties.Add(propInfo.Name, propInfo);
        }

        public void AddMethod(MethodInfo methodInfo)
        {
            List<MethodInfo> list;
            if (!methods.TryGetValue(methodInfo.Name, out list))
            {
                list = new List<MethodInfo>();
                methods.Add(methodInfo.Name, list);
            }
            list.Add(methodInfo);
        }

        public override void Dispose()
        {
            using (new RegFuncCodeGen(cg))
            {
                cg.AppendLine("duk_begin_namespace(ctx, \"{0}\");", type.Namespace);
                cg.AppendLine("duk_begin_class(ctx, typeof({0}), ctor);", type.FullName);
                foreach (var kv in methods)
                {
                    var methodInfo = kv.Value[0];
                    var regName = methodInfo.Name;
                    var funcName = methodInfo.Name;
                    var bStatic = methodInfo.IsStatic;
                    cg.AppendLine("duk_put_method(ctx, \"{0}\", {1}, {2});", regName, funcName, bStatic ? "true" : "false");
                }
                foreach (var kv in properties)
                {
                    var propertyInfo = kv.Value;
                    var bStatic = false;
                    cg.AppendLine("duk_put_property(ctx, \"{0}\", {1}, {2}, {3});",
                        propertyInfo.Name,
                        propertyInfo.CanRead ? propertyInfo.GetMethod.Name : "null",
                        propertyInfo.CanWrite ? propertyInfo.SetMethod.Name : "null",
                        bStatic ? "true" : "false");
                }
                cg.AppendLine("duk_end_class(ctx);");
                cg.AppendLine("duk_end_namespace(ctx);");
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
                    cg.AppendLineL("#if UNITY_ANDROID");
                    break;
                case BuildTarget.iOS:
                    cg.AppendLineL("#if UNITY_IOS");
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    cg.AppendLineL("#if UNITY_STANDALONE_WIN");
                    break;
                case BuildTarget.StandaloneOSX:
                    cg.AppendLineL("#if UNITY_STANDALONE_OSX");
                    break;
                default:
                    cg.AppendLineL("#if false");
                    break;
            }
        }


        public void Dispose()
        {
            cg.AppendLineL("#endif");
        }
    }
}