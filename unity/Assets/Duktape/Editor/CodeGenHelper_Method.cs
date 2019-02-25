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
    //TODO: 带有 JSMutable 属性的 struct 非静态成员方法调用完成后补充一个 duk_rebind_this()
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

            if (this.bindingInfo.count > 1)
            {
                // 需要处理重载
                var argc = cg.AppendGetArgc(true);
                cg.csharp.AppendLine("do {");
                cg.csharp.AddTabLevel();
                {
                    foreach (var variantKV in this.bindingInfo.variants)
                    {
                        var args = variantKV.Key;
                        var variant = variantKV.Value;
                        cg.csharp.AppendLine("if (argc >= {0}) {{", args);
                        cg.csharp.AddTabLevel();
                        {
                            cg.csharp.AppendLine("if (argc == {0}) {{", args);
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
                // 没有重载的情况 (this.bindingInfo.variants.Count == 1)
                foreach (var variantKV in this.bindingInfo.variants)
                {
                    var args = variantKV.Key;
                    var variant = variantKV.Value;
                    var argc = cg.AppendGetArgc(variant.isVararg);

                    if (variant.isVararg)
                    {
                        var method = variant.varargMethods[0];
                        WriteMethod(method, argc, true);

                    }
                    else
                    {
                        var method = variant.plainMethods[0];
                        WriteMethod(method, argc, false);
                    }
                }
            }

            //TODO: 如果产生了无法在 typescript 中声明的方法, 则作标记, 并输出一条万能声明 
            //      [key: string]: any
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

        //TODO: 如果是扩展方法且第一参数不是本类型, 则是因为目标类没有导出而降级的普通静态方法, 按普通静态方法处理
        private void WriteMethod(MethodInfo method, string argc, bool isVararg)
        {
            var parameters = method.GetParameters();
            var returnParameters = new List<ParameterInfo>();

            // get 'this'
            var caller = this.cg.AppendGetThisCS(method);
            // get all parameters
            var arglist = this.cg.AppendGetParameters(argc, parameters, returnParameters);

            if (method.ReturnType == typeof(void))
            {
                // 方法本身没有返回值
                cg.csharp.AppendLine("{0}.{1}({2});", caller, method.Name, arglist);
                if (returnParameters.Count > 0)
                {
                    cg.csharp.AppendLine("DuktapeDLL.duk_push_object(ctx);");
                    //TODO: 填充返回值组合
                    cg.csharp.AppendLine("// fill object properties here;");
                    cg.csharp.AppendLine("return 1;");
                }
                else
                {
                    if (!method.IsStatic && method.DeclaringType.IsValueType) // struct 非静态方法 检查 Mutable 属性
                    {
                        if (method.IsDefined(typeof(JSMutableAttribute), false))
                        {
                            cg.csharp.AppendLine("duk_rebind_this(ctx, {0});", caller);
                        }
                    }
                    cg.csharp.AppendLine("return 0;");
                }
            }
            else
            {
                // 方法本身有返回值
                cg.csharp.AppendLine("var ret = {0}.{1}({2});", caller, method.Name, arglist);
                if (returnParameters.Count > 0)
                {
                    cg.csharp.AppendLine("DuktapeDLL.duk_push_object(ctx);");
                    //TODO: 填充返回值组合
                    cg.csharp.AppendLine("// fill object properties here;");
                }
                else
                {
                    this.cg.AppendPushValue(method.ReturnType, "ret");
                }
                cg.csharp.AppendLine("return 1;");
            }
        }

        private void WriteTSDeclaration(MethodInfo method)
        {
            //TODO: 需要处理参数类型归并问题, 因为如果类型没有导入 ts 中, 可能会在声明中出现相同参数列表的定义
            //      在 MethodVariant 中创建每个方法对应的TS类型名参数列表, 完全相同的不再输出
            var prefix = "";
            if (method.IsStatic)
            {
                prefix = "static ";
            }
            this.cg.typescript.Append("{0}{1}(", prefix, this.bindingInfo.regName);
            var parameters = method.GetParameters();
            var returnParameters = new List<ParameterInfo>();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameter_prefix = "";
                if (parameter.IsOut && parameter.ParameterType.IsByRef)
                {
                    parameter_prefix = "/*out*/ ";
                    returnParameters.Add(parameter);
                }
                else if (parameter.ParameterType.IsByRef)
                {
                    parameter_prefix = "/*ref*/ ";
                    returnParameters.Add(parameter);
                }
                if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && i == parameters.Length - 1)
                {
                    var elementType = parameter.ParameterType.GetElementType();
                    var elementTS = this.cg.bindingManager.GetTypeFullNameTS(elementType);
                    this.cg.typescript.AppendL("{0}...{1}: {2}[]", parameter_prefix, parameter.Name, elementTS);
                }
                else
                {
                    var parameterType = parameter.ParameterType;
                    var parameterTS = this.cg.bindingManager.GetTypeFullNameTS(parameterType);
                    this.cg.typescript.AppendL("{0}{1}: {2}", parameter_prefix, parameter.Name, parameterTS);
                }
                if (i != parameters.Length - 1)
                {
                    this.cg.typescript.AppendL(", ");
                }
            }
            //TODO: 如果存在 ref/out 参数， 则返回值改写为带定义的 object
            //      例如 foo(/*out*/ b: string): { b: string, ret: original_return_type }
            var returnTypeTS = this.cg.bindingManager.GetTypeFullNameTS(method.ReturnType);
            if (returnParameters.Count > 0)
            {
                this.cg.typescript.AppendL("): {");
                this.cg.typescript.AppendLine();
                this.cg.typescript.AddTabLevel();
                this.cg.typescript.AppendLine("ret: {0}, ", returnTypeTS);
                for (var i = 0; i < returnParameters.Count; i++)
                {
                    var parameter = returnParameters[i];
                    var parameterType = parameter.ParameterType;
                    var parameterTypeTS = this.cg.bindingManager.GetTypeFullNameTS(parameterType);
                    this.cg.typescript.AppendLine("{0}: {1}, ", parameter.Name, parameterTypeTS);
                }
                this.cg.typescript.DecTabLevel();
                this.cg.typescript.AppendLine("}");
            }
            else
            {
                this.cg.typescript.AppendL("): {0}", returnTypeTS);
                this.cg.typescript.AppendLine();
            }
        }

        public virtual void Dispose()
        {
        }
    }
}
