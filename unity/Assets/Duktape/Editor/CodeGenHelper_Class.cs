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
            if (type.constructors.available)
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
                        var constructor = bindingInfo.constructors.available ? bindingInfo.constructors.name : "object_private_ctor";
                        if (!bindingInfo.constructors.available)
                        {
                            cg.typescript.AppendLine("private constructor()");
                        }
                        cg.csharp.AppendLine("duk_begin_class(ctx, \"{0}\", typeof({1}), {2});",
                            bindingInfo.regName,
                            this.cg.bindingManager.GetCSTypeFullName(bindingInfo.type),
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
                            var tsPropertyVar = BindingManager.GetTSVariable(bindingInfo.regName);
                            cg.csharp.AppendLine("duk_add_property(ctx, \"{0}\", {1}, {2}, {3});",
                                tsPropertyVar,
                                bindingInfo.getterName != null ? bindingInfo.getterName : "null",
                                bindingInfo.setterName != null ? bindingInfo.setterName : "null",
                                bStatic ? -2 : -1);

                            var tsPropertyPrefix = bindingInfo.setterName != null ? "" : "readonly ";
                            var tsPropertyType = this.cg.bindingManager.GetTypeFullNameTS(bindingInfo.propertyInfo.PropertyType);
                            cg.typescript.AppendLine($"{tsPropertyPrefix}{tsPropertyVar}: {tsPropertyType}");
                        }
                        foreach (var kv in bindingInfo.fields)
                        {
                            var bindingInfo = kv.Value;
                            var bStatic = bindingInfo.isStatic;
                            var tsFieldVar = BindingManager.GetTSVariable(bindingInfo.regName);
                            cg.csharp.AppendLine("duk_add_field(ctx, \"{0}\", {1}, {2}, {3});",
                                tsFieldVar,
                                bindingInfo.getterName != null ? bindingInfo.getterName : "null",
                                bindingInfo.setterName != null ? bindingInfo.setterName : "null",
                                bStatic ? -2 : -1);
                            var tsFieldPrefix = bindingInfo.isStatic ? "static " : "";
                            if (bindingInfo.setterName == null)
                            {
                                tsFieldPrefix += "readonly ";
                            }
                            var tsFieldType = this.cg.bindingManager.GetTypeFullNameTS(bindingInfo.fieldInfo.FieldType);
                            cg.typescript.AppendLine($"{tsFieldPrefix}{tsFieldVar}: {tsFieldType}");
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
