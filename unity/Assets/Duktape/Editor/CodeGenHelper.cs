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

        public TopLevelCodeGen(CodeGenerator cg, TypeBindingInfo type)
        {
            this.cg = cg;
            this.cg.csharp.AppendLine("// UserName: {0} @ {1}", Environment.UserName, DateTime.Now);
            this.cg.csharp.AppendLine("// Assembly: {0}", type.Assembly.GetName());
            this.cg.csharp.AppendLine("// Type: {0}", type.FullName);
            this.cg.csharp.AppendLine("using System;");
            this.cg.csharp.AppendLine("using System.Collections.Generic;");
            this.cg.csharp.AppendLine();

            this.cg.typescript.AppendLine("// {0} {1}", Environment.UserName, DateTime.Now);
        }

        public void Dispose()
        {
        }
    }

    public class NamespaceCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected string ns;
        protected TypeBindingInfo type;

        public NamespaceCodeGen(CodeGenerator cg, string ns, TypeBindingInfo type)
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

    // 生成成员方法绑定代码
    public class MethodCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected string name;
        protected MethodVariants methods;

        public static int MethodComparer(MethodInfo a, MethodInfo b)
        {
            var va = a.GetParameters().Length;
            var vb = b.GetParameters().Length;
            return va > vb ? 1 : ((va == vb) ? 0 : -1);
        }

        public MethodCodeGen(CodeGenerator cg, string name, MethodVariants methods)
        {
            this.cg = cg;
            this.name = name;
            this.methods = methods;

            cg.csharp.AppendLine("[AOT.MonoPInvokeCallbackAttribute(typeof(duk_c_function))]");
            cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            cg.csharp.AppendLine("public static int {0}(IntPtr ctx)", name);
            cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
            //TODO: 如果是扩展方法且第一参数不是本类型, 则是因为目标类没有导出而降级的普通静态方法, 按普通静态方法处理
            if (this.methods.count > 1)
            {
                // 需要处理重载
                cg.csharp.AppendLine("// override {0}", this.methods.count);
                foreach (var variantKV in this.methods.variants)
                {
                    var argc = variantKV.Key;
                    var variant = variantKV.Value;
                    cg.csharp.AppendLine("if (argc >= {0}) {{", argc);
                    cg.csharp.AddTabLevel();
                    {
                        cg.csharp.AppendLine("if (argc == {0}) {{", argc);
                        cg.csharp.AddTabLevel();
                        if (variant.plainMethods.Count > 1)
                        {
                            foreach (var method in variant.plainMethods)
                            {
                                cg.csharp.AppendLine("// {0}", method);
                            }
                        }
                        else
                        {
                            foreach (var method in variant.plainMethods)
                            {
                                cg.csharp.AppendLine("// [if match] {0}", method);
                            }
                        }
                        cg.csharp.DecTabLevel();
                        cg.csharp.AppendLine("}");
                    }
                    {
                        foreach (var method in variant.varargMethods)
                        {
                            cg.csharp.AppendLine("// [if match] {0}", method);
                        }
                    }
                    cg.csharp.DecTabLevel();
                    cg.csharp.AppendLine("}");
                }
            }
            else
            {
                // 没有重载的情况
                cg.csharp.AppendLine("// no override");
                foreach (var variantKV in this.methods.variants)
                {
                    var argc = variantKV.Key;
                    var variant = variantKV.Value;

                    if (variant.isVararg)
                    {
                        cg.csharp.AppendLine("// argc >= {0} && match types", argc);
                    }
                    else
                    {
                        cg.csharp.AppendLine("// argc == {0} && match types", argc);
                    }
                }
            }
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
        protected TypeBindingInfo type;

        public TypeCodeGen(CodeGenerator cg, TypeBindingInfo type)
        {
            this.cg = cg;
            this.type = type;
            this.cg.csharp.AppendLine("[{0}]", typeof(JSBindingAttribute).Name);
            this.cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.csharp.AppendLine("public class {0} : {1} {{", type.JSBindingClassName, typeof(DuktapeBinding).Name);
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
        public ClassCodeGen(CodeGenerator cg, TypeBindingInfo type)
        : base(cg, type)
        {
            // 生成函数体
            foreach (var kv in type.methods)
            {
                using (new MethodCodeGen(cg, kv.Key, kv.Value))
                {

                }
            }
            foreach (var kv in type.staticMethods)
            {
                using (new MethodCodeGen(cg, kv.Key, kv.Value))
                {

                }
            }
            foreach (var kv in type.fields)
            {
                using (new FieldCodeGen(cg, kv.Key, kv.Value))
                {
                }
            }
        }

        public string GetTypeFullNameTS(Type type)
        {
            var info = cg.bindingManager.GetExportedType(type);
            return info != null ? info.FullName : "any";
        }

        public override void Dispose()
        {
            using (new RegFuncCodeGen(cg))
            {
                cg.csharp.Append("duk_begin_namespace(ctx");
                var split_ns = type.Namespace.Split('.');
                for (var i = 0; i < split_ns.Length; i++)
                {
                    var el_ns = split_ns[i];
                    cg.csharp.AppendL(", \"{0}\"", el_ns);
                }
                cg.csharp.AppendLineL(");");
                cg.csharp.AppendLine("duk_begin_class(ctx, typeof({0}), ctor);", type.FullName);
                foreach (var kv in type.methods)
                {
                    var regName = kv.Value.name;
                    var funcName = kv.Key;
                    var bStatic = false;
                    cg.csharp.AppendLine("duk_add_method(ctx, \"{0}\", {1}, {2});", regName, funcName, bStatic ? "true" : "false");
                }                
                foreach (var kv in type.staticMethods)
                {
                    var regName = kv.Value.name;
                    var funcName = kv.Key;
                    var bStatic = true;
                    cg.csharp.AppendLine("duk_add_method(ctx, \"{0}\", {1}, {2});", regName, funcName, bStatic ? "true" : "false");
                }
                foreach (var kv in type.properties)
                {
                    var propertyInfo = kv.Value;
                    var bStatic = false;
                    cg.csharp.AppendLine("duk_add_property(ctx, \"{0}\", {1}, {2}, {3});",
                        propertyInfo.Name,
                        propertyInfo.CanRead ? propertyInfo.GetMethod.Name : "null",
                        propertyInfo.CanWrite ? propertyInfo.SetMethod.Name : "null",
                        bStatic ? "true" : "false");

                    var tsPropertyPrefix = propertyInfo.CanWrite ? "" : "readonly ";
                    var tsPropertyType = GetTypeFullNameTS(propertyInfo.PropertyType);
                    cg.typescript.AppendLine("{0}{1}: {2}", tsPropertyPrefix, propertyInfo.Name, tsPropertyType);
                }
                foreach (var kv in type.fields)
                {
                    var name = kv.Key;
                    var fieldInfo = kv.Value;
                    var bStatic = fieldInfo.IsStatic ? "true" : "false";
                    cg.csharp.AppendLine("duk_add_field(ctx, \"{0}\", {1}, {2});", fieldInfo.Name, name, bStatic);
                    var tsPropertyPrefix = fieldInfo.IsStatic ? "static " : "";
                    var tsPropertyType = GetTypeFullNameTS(fieldInfo.FieldType);
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
        public EnumCodeGen(CodeGenerator cg, TypeBindingInfo type)
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