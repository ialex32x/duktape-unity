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
            this.cg.AppendJSDoc(type.type);
            var prefix = bindingInfo.Namespace != null ? "" : "declare ";
            this.cg.typescript.AppendLine("{0}class {1} {{", prefix, bindingInfo.regName);
            this.cg.typescript.AddTabLevel();

            // 生成函数体
            // 构造函数
            if (type.constructors.hasValid)
            {
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, type.constructors.name))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new ConstructorCodeGen(cg, type.constructors))
                            {
                            }
                        }
                    }
                }
            }
            // 非静态成员方法
            foreach (var kv in type.methods)
            {
                var bindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, bindingInfo.name))
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
            // 静态成员方法
            foreach (var kv in type.staticMethods)
            {
                var bindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, bindingInfo.name))
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
            // 所有属性
            foreach (var kv in type.properties)
            {
                var bindingInfo = kv.Value;
                // 可读属性
                if (bindingInfo.getterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, bindingInfo.getterName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new PropertyGetterCodeGen(cg, bindingInfo))
                                {
                                }
                            }
                        }
                    }
                }
                // 可写属性
                if (bindingInfo.setterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, bindingInfo.setterName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new PropertySetterCodeGen(cg, bindingInfo))
                                {
                                }
                            }
                        }
                    }
                }
            }
            // 所有字段
            foreach (var kv in type.fields)
            {
                var bindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, bindingInfo.getterName))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new FieldGetterCodeGen(cg, bindingInfo))
                            {
                            }
                        }
                    }
                }
                // 可写字段 
                if (bindingInfo.setterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, bindingInfo.setterName))
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
                        var constructor = bindingInfo.constructors.hasValid ? bindingInfo.constructors.name : "object_private_ctor";
                        if (!bindingInfo.constructors.hasValid)
                        {
                            cg.typescript.AppendLine("private constructor()");
                        }
                        cg.csharp.AppendLine("duk_begin_class(ctx, \"{0}\", typeof({1}), {2});",
                            bindingInfo.regName,
                            this.cg.bindingManager.GetTypeFullNameCS(bindingInfo.type),
                            constructor);
                        foreach (var kv in bindingInfo.methods)
                        {
                            var regName = kv.Value.regName;
                            var funcName = kv.Value.name;
                            var bStatic = false;
                            cg.csharp.AppendLine("duk_add_method(ctx, \"{0}\", {1}, {2});", regName, funcName, bStatic ? -2 : -1);
                        }
                        foreach (var kv in bindingInfo.staticMethods)
                        {
                            var regName = kv.Value.regName;
                            var funcName = kv.Value.name;
                            var bStatic = true;
                            cg.csharp.AppendLine("duk_add_method(ctx, \"{0}\", {1}, {2});", regName, funcName, bStatic ? -2 : -1);
                        }
                        foreach (var kv in bindingInfo.properties)
                        {
                            var bindingInfo = kv.Value;
                            var bStatic = false;
                            cg.csharp.AppendLine("duk_add_property(ctx, \"{0}\", {1}, {2}, {3});",
                                bindingInfo.regName,
                                bindingInfo.getterName != null ? bindingInfo.getterName : "null",
                                bindingInfo.setterName != null ? bindingInfo.setterName : "null",
                                bStatic ? -2 : -1);

                            var tsPropertyPrefix = bindingInfo.setterName != null ? "" : "readonly ";
                            var tsPropertyType = this.cg.bindingManager.GetTypeFullNameTS(bindingInfo.propertyInfo.PropertyType);
                            cg.typescript.AppendLine("{0}{1}: {2}", tsPropertyPrefix, bindingInfo.propertyInfo.Name, tsPropertyType);
                        }
                        foreach (var kv in bindingInfo.fields)
                        {
                            var fieldInfo = kv.Value;
                            var bStatic = fieldInfo.isStatic;
                            cg.csharp.AppendLine("duk_add_field(ctx, \"{0}\", {1}, {2}, {3});",
                                fieldInfo.regName,
                                fieldInfo.getterName != null ? fieldInfo.getterName : "null",
                                fieldInfo.setterName != null ? fieldInfo.setterName : "null",
                                bStatic ? -2 : -1);
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
