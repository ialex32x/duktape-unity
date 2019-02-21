using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class ClassCodeGen : TypeCodeGen
    {
        public ClassCodeGen(CodeGenerator cg, TypeBindingInfo type)
        : base(cg, type)
        {
            var prefix = bindingInfo.Namespace != null ? "" : "export ";
            this.cg.typescript.AppendLine("{0}class {1} {{", prefix, bindingInfo.Name);
            this.cg.typescript.AddTabLevel();

            // 生成函数体
            foreach (var kv in type.methods)
            {
                var bindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncCodeGen(cg, bindingInfo.name))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new MethodCodeGen(cg, bindingInfo))
                            {
                            }
                        }
                    }
                }
            }
            foreach (var kv in type.staticMethods)
            {
                var bindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncCodeGen(cg, bindingInfo.name))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new MethodCodeGen(cg, bindingInfo))
                            {
                            }
                        }
                    }
                }
            }
            foreach (var kv in type.fields)
            {
                var bindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncCodeGen(cg, bindingInfo.getterName))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new FieldGetterCodeGen(cg, bindingInfo))
                            {
                            }
                        }
                    }
                }
                if (bindingInfo.setterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncCodeGen(cg, bindingInfo.setterName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new FieldSetterCodeGen(cg, bindingInfo))
                                {
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void Dispose()
        {
            using (new PreservedCodeGen(cg))
            {
                using (new RegFuncCodeGen(cg))
                {
                    using (new RegFuncNamespaceCodeGen(cg, bindingInfo))
                    {
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
                    }
                }
                base.Dispose();
            }

            this.cg.typescript.DecTabLevel();
            this.cg.typescript.AppendLine("}");
        }
    }
}
