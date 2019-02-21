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
            this.cg.csharp.AppendLine("// UserName: {0} @ {1}", Environment.UserName, this.cg.bindingManager.dateTime);
            this.cg.csharp.AppendLine("// Assembly: {0}", type.Assembly.GetName());
            this.cg.csharp.AppendLine("// Type: {0}", type.FullName);
            this.cg.csharp.AppendLine("using System;");
            this.cg.csharp.AppendLine("using System.Collections.Generic;");
            this.cg.csharp.AppendLine();

            this.cg.typescript.AppendLine("// {0} {1}", Environment.UserName, this.cg.bindingManager.dateTime);
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
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                this.cg.typescript.AppendLine("declare namespace {0} {{", type.Namespace);
                this.cg.typescript.AddTabLevel();
            }
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(ns))
            {
                this.cg.csharp.DecTabLevel();
                this.cg.csharp.AppendLine("}");
            }
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                this.cg.typescript.DecTabLevel();
                this.cg.typescript.AppendLine("}");
            }
        }
    }

    public class FieldGetterCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected FieldBindingInfo bindingInfo;

        public FieldGetterCodeGen(CodeGenerator cg, FieldBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            this.cg.csharp.AppendLine("[AOT.MonoPInvokeCallbackAttribute(typeof(duk_c_function))]");
            this.cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.csharp.AppendLine("public static int {0}(IntPtr ctx)", bindingInfo.getterName);
            this.cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
            this.GenerateBody();
        }

        private void GenerateBody()
        {
            if (bindingInfo.isStatic)
            {
                this.cg.csharp.AppendLine("var ret = {0}.{1};", bindingInfo.fieldInfo.DeclaringType, bindingInfo.fieldInfo.Name);
                this.cg.csharp.AppendLine("duk_push_any(ctx, ret);");
                this.cg.csharp.AppendLine("return 1;");
            }
            else
            {
                this.cg.csharp.AppendLine("var self = ({0})duk_get_this(ctx);", bindingInfo.fieldInfo.DeclaringType);
                this.cg.csharp.AppendLine("var ret = self.{0};", bindingInfo.fieldInfo.Name);
                this.cg.csharp.AppendLine("duk_push_any(ctx, ret);");
                this.cg.csharp.AppendLine("return 1;");
            }
        }

        public void Dispose()
        {
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }
    }

    public class FieldSetterCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected FieldBindingInfo bindingInfo;

        public FieldSetterCodeGen(CodeGenerator cg, FieldBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            this.cg.csharp.AppendLine("[AOT.MonoPInvokeCallbackAttribute(typeof(duk_c_function))]");
            this.cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.csharp.AppendLine("public static int {0}(IntPtr ctx)", bindingInfo.setterName);
            this.cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
        }

        public void Dispose()
        {
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }
    }

    // 生成成员方法绑定代码
    public class MethodCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected MethodBindingInfo bindingInfo;

        public static int MethodComparer(MethodInfo a, MethodInfo b)
        {
            var va = a.GetParameters().Length;
            var vb = b.GetParameters().Length;
            return va > vb ? 1 : ((va == vb) ? 0 : -1);
        }

        public MethodCodeGen(CodeGenerator cg, MethodBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            cg.csharp.AppendLine("[AOT.MonoPInvokeCallbackAttribute(typeof(duk_c_function))]");
            cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            cg.csharp.AppendLine("public static int {0}(IntPtr ctx)", bindingInfo.name);
            cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
            //TODO: 如果是扩展方法且第一参数不是本类型, 则是因为目标类没有导出而降级的普通静态方法, 按普通静态方法处理
            if (this.bindingInfo.count > 1)
            {
                // 需要处理重载
                cg.csharp.AppendLine("// override {0}", this.bindingInfo.count);
                cg.csharp.AppendLine("do {");
                cg.csharp.AddTabLevel();
                {
                    foreach (var variantKV in this.bindingInfo.variants)
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
                            cg.csharp.AppendLine("break;");
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
                cg.csharp.DecTabLevel();
                cg.csharp.AppendLine("} while(false);");
            }
            else
            {
                // 没有重载的情况
                cg.csharp.AppendLine("// no override");
                foreach (var variantKV in this.bindingInfo.variants)
                {
                    var argc = variantKV.Key;
                    var variant = variantKV.Value;

                    if (variant.isVararg)
                    {
                        var method = variant.varargMethods[0];
                        cg.csharp.AppendLine("// argc >= {0}", argc);
                        cg.csharp.AppendLine("// {0}", method);

                    }
                    else
                    {
                        var method = variant.plainMethods[0];
                        cg.csharp.AppendLine("// argc == {0}", argc);
                        cg.csharp.AppendLine("// {0}", method);
                    }
                }
            }

            foreach (var variantKV in this.bindingInfo.variants)
            {
                foreach (var method in variantKV.Value.plainMethods)
                {
                    WriteTSDeclaration(method);
                }
                foreach (var method in variantKV.Value.varargMethods)
                {
                    WriteTSDeclaration(method);
                }
            }
        }

        private void WriteTSDeclaration(MethodInfo method)
        {
            //TODO: 需要处理参数类型归并问题, 因为如果类型没有导入 ts 中, 可能会在声明中出现相同参数列表的定义
            this.cg.typescript.Append("{0}(", this.bindingInfo.regName);
            var parameters = method.GetParameters();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && i == parameters.Length - 1)
                {
                    var elementType = parameter.ParameterType.GetElementType();
                    var elementTS = this.cg.bindingManager.GetTypeFullNameTS(elementType);
                    this.cg.typescript.AppendL("...{0}: {1}[]", parameter.Name, elementTS);
                }
                else
                {
                    var parameterType = parameter.ParameterType;
                    var parameterTS = this.cg.bindingManager.GetTypeFullNameTS(parameterType);
                    this.cg.typescript.AppendL("{0}: {1}", parameter.Name, parameterTS);
                }
                if (i != parameters.Length - 1)
                {
                    this.cg.typescript.AppendL(", ");
                }
            }
            var returnTypeTS = this.cg.bindingManager.GetTypeFullNameTS(method.ReturnType);
            this.cg.typescript.AppendL("): {0}", returnTypeTS);
            this.cg.typescript.AppendLine();
        }

        public virtual void Dispose()
        {
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
            this.cg.csharp.AppendLine("public static int reg(IntPtr ctx)");
            this.cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
        }

        public virtual void Dispose()
        {
            this.cg.csharp.AppendLine("return 0;");
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }
    }

    public class TypeCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected TypeBindingInfo bindingInfo;

        public TypeCodeGen(CodeGenerator cg, TypeBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;
            this.cg.csharp.AppendLine("[{0}({1})]", typeof(JSBindingAttribute).Name, DuktapeVM.VERSION);
            this.cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.csharp.AppendLine("public class {0} : {1} {{", bindingInfo.JSBindingClassName, typeof(DuktapeBinding).Name);
            this.cg.csharp.AddTabLevel();

            this.cg.typescript.AppendLine("class {0} {{", bindingInfo.Name);
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
                var bindingInfo = kv.Value;
                using (new MethodCodeGen(cg, bindingInfo))
                {
                }
            }
            foreach (var kv in type.staticMethods)
            {
                var bindingInfo = kv.Value;
                using (new MethodCodeGen(cg, bindingInfo))
                {
                }
            }
            foreach (var kv in type.fields)
            {
                var bindingInfo = kv.Value;
                using (new FieldGetterCodeGen(cg, bindingInfo))
                {
                }
                if (bindingInfo.setterName != null)
                {
                    using (new FieldSetterCodeGen(cg, bindingInfo))
                    {
                    }
                }
            }
        }

        public override void Dispose()
        {
            using (new RegFuncCodeGen(cg))
            {
                cg.csharp.Append("duk_begin_namespace(ctx");
                // Debug.LogErrorFormat("{0}: {1}", bindingInfo.type, bindingInfo.Namespace);
                if (bindingInfo.Namespace != null)
                {
                    var split_ns = bindingInfo.Namespace.Split('.');
                    for (var i = 0; i < split_ns.Length; i++)
                    {
                        var el_ns = split_ns[i];
                        cg.csharp.AppendL(", \"{0}\"", el_ns);
                    }
                }
                cg.csharp.AppendLineL(");");
                cg.csharp.AppendLine("duk_begin_class(ctx, typeof({0}), ctor);", bindingInfo.FullName);
                foreach (var kv in bindingInfo.methods)
                {
                    var regName = kv.Value.regName;
                    var funcName = kv.Value.name;
                    var bStatic = "false";
                    cg.csharp.AppendLine("duk_add_method(ctx, \"{0}\", {1}, {2});", regName, funcName, bStatic);
                }
                foreach (var kv in bindingInfo.staticMethods)
                {
                    var regName = kv.Value.regName;
                    var funcName = kv.Value.name;
                    var bStatic = "true";
                    cg.csharp.AppendLine("duk_add_method(ctx, \"{0}\", {1}, {2});", regName, funcName, bStatic);
                }
                foreach (var kv in bindingInfo.properties)
                {
                    var bindingInfo = kv.Value;
                    var bStatic = "false";
                    cg.csharp.AppendLine("duk_add_property(ctx, \"{0}\", {1}, {2}, {3});",
                        bindingInfo.regName,
                        bindingInfo.getterName,
                        bindingInfo.setterName,
                        bStatic);

                    var tsPropertyPrefix = bindingInfo.setterName != null ? "" : "readonly ";
                    var tsPropertyType = this.cg.bindingManager.GetTypeFullNameTS(bindingInfo.propertyInfo.PropertyType);
                    cg.typescript.AppendLine("{0}{1}: {2}", tsPropertyPrefix, bindingInfo.propertyInfo.Name, tsPropertyType);
                }
                foreach (var kv in bindingInfo.fields)
                {
                    var fieldInfo = kv.Value;
                    var bStatic = fieldInfo.isStatic ? "true" : "false";
                    cg.csharp.AppendLine("duk_add_property(ctx, \"{0}\", {1}, {2}, {3});",
                        fieldInfo.regName,
                        fieldInfo.getterName != null ? fieldInfo.getterName : "null",
                        fieldInfo.setterName != null ? fieldInfo.setterName : "null",
                        bStatic);
                    var tsPropertyPrefix = fieldInfo.isStatic ? "static " : "";
                    if (fieldInfo.setterName == null)
                    {
                        tsPropertyPrefix += "readonly ";
                    }
                    var tsPropertyType = this.cg.bindingManager.GetTypeFullNameTS(fieldInfo.fieldInfo.FieldType);
                    cg.typescript.AppendLine("{0}{1}: {2}", tsPropertyPrefix, fieldInfo.regName, tsPropertyType);
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