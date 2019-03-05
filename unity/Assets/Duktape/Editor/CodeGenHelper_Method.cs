using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public abstract class MethodBaseCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        // 方法参数数比较, 用于列表排序
        public static int MethodComparer(MethodBase a, MethodBase b)
        {
            var va = a.GetParameters().Length;
            var vb = b.GetParameters().Length;
            return va > vb ? 1 : ((va == vb) ? 0 : -1);
        }

        public MethodBaseCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
        }

        public virtual void Dispose()
        {
        }

        // parametersByRef: 可修改参数将被加入此列表
        // hasParams: 是否包含变参 (最后一个参数将按数组处理)
        public string AppendGetParameters(bool hasParams, string nargs, ParameterInfo[] parameters, List<ParameterInfo> parametersByRef)
        {
            var arglist = "";
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                //TODO: 需要处理 ref/out 参数在 js 中的返回方式问题
                //      可能的处理方式是将这些参数合并函数返回值转化为一个 object 作为最终返回值
                if (parameter.IsOut)
                {
                    arglist += "out ";
                    if (parametersByRef != null)
                    {
                        parametersByRef.Add(parameter);
                    }
                }
                else if (parameter.ParameterType.IsByRef)
                {
                    arglist += "ref ";
                    if (parametersByRef != null)
                    {
                        parametersByRef.Add(parameter);
                    }
                }
                arglist += "arg" + i;
                if (i != parameters.Length - 1)
                {
                    arglist += ", ";
                }
                var argType = this.cg.bindingManager.GetTypeFullNameCS(parameter.ParameterType);
                if (hasParams && i == parameters.Length - 1)
                {
                    // 处理数组
                    var argElementType = this.cg.bindingManager.GetTypeFullNameCS(parameter.ParameterType.GetElementType());
                    var argElementIndex = i == 0 ? nargs : nargs + " - " + i;
                    this.cg.csharp.AppendLine($"{argType} arg{i} = new {argElementType}[{argElementIndex}];");
                    this.cg.csharp.AppendLine($"for (var i = {i}; i < {nargs}; i++)");
                    this.cg.csharp.AppendLine("{");
                    this.cg.csharp.AddTabLevel();
                    {
                        var argElementGetterOp = this.cg.bindingManager.GetDuktapeGetter(parameter.ParameterType.GetElementType());
                        var argElementOffset = i == 0 ? "" : " - " + i;
                        this.cg.csharp.AppendLine($"{argElementGetterOp}(ctx, i, out arg{i}[i{argElementOffset}]);");
                    }
                    this.cg.csharp.DecTabLevel();
                    this.cg.csharp.AppendLine("}");
                }
                else
                {
                    var argGetterOp = this.cg.bindingManager.GetDuktapeGetter(parameter.ParameterType);
                    this.cg.csharp.AppendLine($"{argType} arg{i};");
                    this.cg.csharp.AppendLine($"{argGetterOp}(ctx, {i}, out arg{i});");
                }
            }
            return arglist;
        }

        //TODO: 考虑将 ref/out 参数以额外增加一个参数的形式返回
        protected List<ParameterInfo> WriteTSDeclaration(MethodBase method, MethodBaseBindingInfo bindingInfo)
        {
            //TODO: 需要处理参数类型归并问题, 因为如果类型没有导入 ts 中, 可能会在声明中出现相同参数列表的定义
            //      在 MethodVariant 中创建每个方法对应的TS类型名参数列表, 完全相同的不再输出
            this.cg.AppendJSDoc(method);
            var prefix = "";
            if (method.IsStatic)
            {
                prefix = "static ";
            }
            this.cg.typescript.Append($"{prefix}{bindingInfo.regName}(");
            var parameters = method.GetParameters();
            var refParameters = new List<ParameterInfo>();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameter_prefix = "";
                if (parameter.IsOut && parameter.ParameterType.IsByRef)
                {
                    parameter_prefix = "/*out*/ ";
                    refParameters.Add(parameter);
                }
                else if (parameter.ParameterType.IsByRef)
                {
                    parameter_prefix = "/*ref*/ ";
                    refParameters.Add(parameter);
                }
                if (parameter.IsDefined(typeof(ParamArrayAttribute), false) && i == parameters.Length - 1)
                {
                    var elementType = parameter.ParameterType.GetElementType();
                    var elementTS = this.cg.bindingManager.GetTypeFullNameTS(elementType);
                    this.cg.typescript.AppendL($"{parameter_prefix}...{parameter.Name}: {elementTS}[]");
                }
                else
                {
                    var parameterType = parameter.ParameterType;
                    var parameterTS = this.cg.bindingManager.GetTypeFullNameTS(parameterType);
                    this.cg.typescript.AppendL($"{parameter_prefix}{parameter.Name}: {parameterTS}");
                }
                if (i != parameters.Length - 1)
                {
                    this.cg.typescript.AppendL(", ");
                }
            }
            this.cg.typescript.AppendL(")");
            return refParameters;
        }
    }

    public class ConstructorCodeGen : MethodBaseCodeGen
    {
        private ConstructorBindingInfo bindingInfo;

        public ConstructorCodeGen(CodeGenerator cg, ConstructorBindingInfo bindingInfo)
        : base(cg)
        {
            this.bindingInfo = bindingInfo;

            if (this.bindingInfo.variants.Count > 0)
            {
                foreach (var constructor in this.bindingInfo.variants)
                {
                    WriteCSConstructor(constructor);
                    WriteTSDeclaration(constructor, bindingInfo);
                }
            }
            else
            {
                WriteCSDefaultConstructor();
                WriteTSDefaultConsturctorDeclaration();
            }
        }

        private void WriteCSConstructor(ConstructorInfo constructor)
        {
            //TODO: 写入构造函数
            var parameters = constructor.GetParameters();
            if (parameters.Length == 0)
            {
                WriteCSDefaultConstructor();
            }
            else
            {
                var isVararg = parameters[parameters.Length - 1].IsDefined(typeof(ParamArrayAttribute), false);
                var argc = this.cg.AppendGetArgCount(isVararg);
                var arglist = this.AppendGetParameters(isVararg, argc, parameters, null);
                var decalringTypeName = this.cg.bindingManager.GetTypeFullNameCS(this.bindingInfo.decalringType);
                this.cg.csharp.AppendLine($"var o = new {decalringTypeName}({arglist});");
                this.cg.csharp.AppendLine("DuktapeDLL.duk_push_this(ctx);");
                this.cg.csharp.AppendLine("duk_bind_native(ctx, -1, o);");
                this.cg.csharp.AppendLine("DuktapeDLL.duk_pop(ctx);");
                this.cg.csharp.AppendLine("return 0;");
            }
        }

        private void WriteCSDefaultConstructor()
        {
            //TODO: 写入默认构造函数 (struct 无参构造)
            var decalringTypeName = this.cg.bindingManager.GetTypeFullNameCS(this.bindingInfo.decalringType);
            this.cg.csharp.AppendLine($"var o = new {decalringTypeName}();");
            this.cg.csharp.AppendLine("DuktapeDLL.duk_push_this(ctx);");
            this.cg.csharp.AppendLine("duk_bind_native(ctx, -1, o);");
            this.cg.csharp.AppendLine("DuktapeDLL.duk_pop(ctx);");
            this.cg.csharp.AppendLine("return 0;");
        }

        private void WriteTSDefaultConsturctorDeclaration()
        {
            this.cg.typescript.AppendLine($"{this.bindingInfo.regName}()");
        }
    }

    // 生成成员方法绑定代码
    public class MethodCodeGen : MethodBaseCodeGen
    {
        protected MethodBindingInfo bindingInfo;

        public MethodCodeGen(CodeGenerator cg, MethodBindingInfo bindingInfo)
        : base(cg)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            if (this.bindingInfo.count > 1)
            {
                // 需要处理重载
                var argc = cg.AppendGetArgCount(true);
                cg.csharp.AppendLine("do {");
                cg.csharp.AddTabLevel();
                {
                    foreach (var variantKV in this.bindingInfo.variants)
                    {
                        var args = variantKV.Key;
                        var variant = variantKV.Value;
                        cg.csharp.AppendLine("if (argc >= {0}) {{", args);
                        cg.csharp.AddTabLevel();
                        // 处理定参
                        {
                            cg.csharp.AppendLine("if (argc == {0}) {{", args);
                            cg.csharp.AddTabLevel();
                            if (variant.plainMethods.Count > 1)
                            {
                                foreach (var method in variant.plainMethods)
                                {
                                    cg.csharp.AppendLine("// {0}", method);
                                }
                                cg.csharp.AppendLine("break;");
                            }
                            else
                            {
                                var method = variant.plainMethods[0];
                                // cg.csharp.AppendLine("// [if match] {0}", method);
                                this.WriteCSMethodBinding(method, method.ReturnType, argc, false);
                            }
                            cg.csharp.DecTabLevel();
                            cg.csharp.AppendLine("}");
                        }
                        // 处理变参
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
                var error = this.cg.bindingManager.GetDuktapeGenericError("no matched method variant");
                cg.csharp.AppendLine($"return {error}");
            }
            else
            {
                // 没有重载的情况 (this.bindingInfo.variants.Count == 1)
                foreach (var variantKV in this.bindingInfo.variants)
                {
                    var args = variantKV.Key;
                    var variant = variantKV.Value;
                    var argc = cg.AppendGetArgCount(variant.isVararg);

                    if (variant.isVararg)
                    {
                        var method = variant.varargMethods[0];
                        WriteCSMethodBinding(method, method.ReturnType, argc, true);

                    }
                    else
                    {
                        var method = variant.plainMethods[0];
                        WriteCSMethodBinding(method, method.ReturnType, argc, false);
                    }
                }
            }

            //TODO: 如果产生了无法在 typescript 中声明的方法, 则作标记, 并输出一条万能声明 
            //      [key: string]: any
            foreach (var variantKV in this.bindingInfo.variants)
            {
                foreach (var method in variantKV.Value.plainMethods)
                {
                    _WriteTSReturn(method.ReturnType, WriteTSDeclaration(method, this.bindingInfo));
                }
                foreach (var method in variantKV.Value.varargMethods)
                {
                    _WriteTSReturn(method.ReturnType, WriteTSDeclaration(method, this.bindingInfo));
                }
            }
        }

        // 写入绑定代码
        //TODO: 如果是扩展方法且第一参数不是本类型, 则是因为目标类没有导出而降级的普通静态方法, 按普通静态方法处理
        private void WriteCSMethodBinding(MethodBase method, Type returnType, string argc, bool isVararg)
        {
            var parameters = method.GetParameters();
            var returnParameters = new List<ParameterInfo>();
            var caller = this.cg.AppendGetThisCS(method);
            var arglist = this.AppendGetParameters(isVararg, argc, parameters, returnParameters);

            if (returnType == typeof(void))
            {
                // 方法本身没有返回值
                cg.csharp.AppendLine($"{caller}.{method.Name}({arglist});");
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
                            cg.csharp.AppendLine($"duk_rebind_this(ctx, {caller});");
                        }
                    }
                    cg.csharp.AppendLine("return 0;");
                }
            }
            else
            {
                // 方法本身有返回值
                cg.csharp.AppendLine($"var ret = {caller}.{method.Name}({arglist});");
                if (returnParameters.Count > 0)
                {
                    cg.csharp.AppendLine("DuktapeDLL.duk_push_object(ctx);");
                    //TODO: 填充返回值组合
                    cg.csharp.AppendLine("// fill object properties here;");
                }
                else
                {
                    cg.AppendPushValue(returnType, "ret");
                }
                cg.csharp.AppendLine("return 1;");
            }
        }

        // 写入返回类型声明
        private void _WriteTSReturn(Type returnType, List<ParameterInfo> returnParameters)
        {
            //TODO: 如果存在 ref/out 参数， 则返回值改写为带定义的 object
            //      例如 foo(/*out*/ b: string): { b: string, ret: original_return_type }
            if (returnType != null)
            {
                var returnTypeTS = this.cg.bindingManager.GetTypeFullNameTS(returnType);
                if (returnParameters != null && returnParameters.Count > 0)
                {
                    this.cg.typescript.AppendL(": {");
                    this.cg.typescript.AppendLine();
                    this.cg.typescript.AddTabLevel();
                    this.cg.typescript.AppendLine($"ret: {returnTypeTS}, ");
                    for (var i = 0; i < returnParameters.Count; i++)
                    {
                        var parameter = returnParameters[i];
                        var parameterType = parameter.ParameterType;
                        var parameterTypeTS = this.cg.bindingManager.GetTypeFullNameTS(parameterType);
                        this.cg.typescript.AppendLine($"{parameter.Name}: {parameterTypeTS}, ");
                    }
                    this.cg.typescript.DecTabLevel();
                    this.cg.typescript.AppendLine("}");
                }
                else
                {
                    this.cg.typescript.AppendL($": {returnTypeTS}");
                    this.cg.typescript.AppendLine();
                }
            }
        }
    }
}
