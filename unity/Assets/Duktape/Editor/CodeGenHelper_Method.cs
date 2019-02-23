using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

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
                        var parameters = method.GetParameters();
                        var arglist = "";
                        for (var i = 0; i < parameters.Length; i++)
                        {
                            var parameter = parameters[i];
                            arglist += "arg" + i;
                            if (i != parameters.Length - 1)
                            {
                                arglist += ", ";
                            }
                            cg.csharp.AppendLine("{0} arg{1};", parameter.ParameterType.FullName, i);
                            cg.csharp.AppendLine("duk_get_???(ctx, {0}, out arg{0});", i);
                        }
                        var caller = "";
                        if (method.IsStatic)
                        {
                            caller = method.DeclaringType.FullName;
                        }
                        else
                        {
                            caller = "self";
                            cg.csharp.AppendLine("var self = duk_get_this(ctx);");
                        }
                        if (method.ReturnType == typeof(void))
                        {
                            cg.csharp.AppendLine("{0}.{1}({2});", caller, method.Name, arglist);
                            cg.csharp.AppendLine("return 0;");
                        }
                        else
                        {
                            cg.csharp.AppendLine("var ret = {0}.{1}({2});", caller, method.Name, arglist);
                            cg.csharp.AppendLine("duk_push_???(ctx, ret);");
                            cg.csharp.AppendLine("return 1;");
                        }
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
            //      在 MethodVariant 中创建每个方法对应的TS类型名参数列表, 完全相同的不再输出
            if (method.IsStatic)
            {
                this.cg.typescript.Append("static ");
            }
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
        }
    }
}
