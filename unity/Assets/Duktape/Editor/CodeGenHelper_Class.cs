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
        public ClassCodeGen(CodeGenerator cg, TypeBindingInfo bindingInfo)
        : base(cg, bindingInfo)
        {
            this.cg.AppendJSDoc(this.bindingInfo.type);
            var prefix = this.bindingInfo.Namespace != null ? "" : "declare ";
            var super = this.cg.bindingManager.GetTSSuperName(this.bindingInfo);
            var extends = string.IsNullOrEmpty(super) ? "" : $" extends {super}";
            var regName = this.bindingInfo.regName;
            if (bindingInfo.type.IsAbstract)
            {
                prefix += "abstract ";
            }
            this.cg.tsDeclare.AppendLine($"{prefix}class {regName}{extends} {{");
            this.cg.tsDeclare.AddTabLevel();

            // 生成函数体
            // 构造函数
            if (this.bindingInfo.constructors.available)
            {
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, this.bindingInfo.constructors.name))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new ConstructorCodeGen(cg, this.bindingInfo.constructors))
                            {
                            }
                        }
                    }
                }
            }
            // 非静态成员方法
            foreach (var kv in this.bindingInfo.methods)
            {
                var methodBindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, methodBindingInfo.name))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new MethodCodeGen(cg, methodBindingInfo))
                            {
                            }
                        }
                    }
                }
            }
            // 静态成员方法
            foreach (var kv in this.bindingInfo.staticMethods)
            {
                var propertyBindingInfo = kv.Value;
                using (new PInvokeGuardCodeGen(cg))
                {
                    using (new BindingFuncDeclareCodeGen(cg, propertyBindingInfo.name))
                    {
                        using (new TryCatchGuradCodeGen(cg))
                        {
                            using (new MethodCodeGen(cg, propertyBindingInfo))
                            {
                            }
                        }
                    }
                }
            }
            // 所有属性
            foreach (var kv in this.bindingInfo.properties)
            {
                var propertyBindingInfo = kv.Value;
                // 可读属性
                if (propertyBindingInfo.getterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, propertyBindingInfo.getterName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new PropertyGetterCodeGen(cg, propertyBindingInfo))
                                {
                                }
                            }
                        }
                    }
                }
                // 可写属性
                if (propertyBindingInfo.setterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, propertyBindingInfo.setterName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new PropertySetterCodeGen(cg, propertyBindingInfo))
                                {
                                }
                            }
                        }
                    }
                }
            }
            // 所有字段
            foreach (var kv in this.bindingInfo.fields)
            {
                var fieldBindingInfo = kv.Value;
                if (fieldBindingInfo.getterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, fieldBindingInfo.getterName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new FieldGetterCodeGen(cg, fieldBindingInfo))
                                {
                                }
                            }
                        }
                    }
                }
                // 可写字段 
                if (fieldBindingInfo.setterName != null)
                {
                    using (new PInvokeGuardCodeGen(cg))
                    {
                        using (new BindingFuncDeclareCodeGen(cg, fieldBindingInfo.setterName))
                        {
                            using (new TryCatchGuradCodeGen(cg))
                            {
                                using (new FieldSetterCodeGen(cg, fieldBindingInfo))
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
                        if (!bindingInfo.constructors.available && !bindingInfo.type.IsAbstract)
                        {
                            cg.tsDeclare.AppendLine("protected constructor()");
                        }
                        cg.cs.AppendLine("duk_begin_class(ctx, \"{0}\", typeof({1}), {2});",
                            bindingInfo.regName,
                            this.cg.bindingManager.GetCSTypeFullName(bindingInfo.type),
                            constructor);
                        foreach (var kv in bindingInfo.methods)
                        {
                            var regName = kv.Value.regName;
                            var funcName = kv.Value.name;
                            var bStatic = false;
                            cg.cs.AppendLine("duk_add_method(ctx, \"{0}\", {1}, {2});", regName, funcName, bStatic ? -2 : -1);
                        }
                        foreach (var kv in bindingInfo.staticMethods)
                        {
                            var regName = kv.Value.regName;
                            var funcName = kv.Value.name;
                            var bStatic = true;
                            cg.cs.AppendLine("duk_add_method(ctx, \"{0}\", {1}, {2});", regName, funcName, bStatic ? -2 : -1);
                        }
                        foreach (var kv in bindingInfo.properties)
                        {
                            var bindingInfo = kv.Value;
                            var bStatic = false;
                            var tsPropertyVar = BindingManager.GetTSVariable(bindingInfo.regName);
                            cg.cs.AppendLine("duk_add_property(ctx, \"{0}\", {1}, {2}, {3});",
                                tsPropertyVar,
                                bindingInfo.getterName != null ? bindingInfo.getterName : "null",
                                bindingInfo.setterName != null ? bindingInfo.setterName : "null",
                                bStatic ? -2 : -1);

                            var tsPropertyPrefix = bindingInfo.setterName != null ? "" : "readonly ";
                            var tsPropertyType = this.cg.bindingManager.GetTSTypeFullName(bindingInfo.propertyInfo.PropertyType);
                            cg.tsDeclare.AppendLine($"{tsPropertyPrefix}{tsPropertyVar}: {tsPropertyType}");
                        }
                        foreach (var kv in bindingInfo.fields)
                        {
                            var bindingInfo = kv.Value;
                            var bStatic = bindingInfo.isStatic;
                            var tsFieldVar = BindingManager.GetTSVariable(bindingInfo.regName);
                            if (bindingInfo.constantValue != null)
                            {
                                var cv = bindingInfo.constantValue;
                                cg.cs.AppendLine($"duk_add_const(ctx, \"{tsFieldVar}\", {cv}, {-2});");
                            }
                            else
                            {
                                cg.cs.AppendLine("duk_add_field(ctx, \"{0}\", {1}, {2}, {3});",
                                    tsFieldVar,
                                    bindingInfo.getterName != null ? bindingInfo.getterName : "null",
                                    bindingInfo.setterName != null ? bindingInfo.setterName : "null",
                                    bStatic ? -2 : -1);
                            }
                            var tsFieldPrefix = bindingInfo.isStatic ? "static " : "";
                            if (bindingInfo.setterName == null)
                            {
                                tsFieldPrefix += "readonly ";
                            }
                            var tsFieldType = this.cg.bindingManager.GetTSTypeFullName(bindingInfo.fieldInfo.FieldType);
                            cg.tsDeclare.AppendLine($"{tsFieldPrefix}{tsFieldVar}: {tsFieldType}");
                        }
                        cg.cs.AppendLine("duk_end_class(ctx);");
                    }
                }
                base.Dispose();
            }

            this.cg.tsDeclare.DecTabLevel();
            this.cg.tsDeclare.AppendLine("}");
        }
    }
}
