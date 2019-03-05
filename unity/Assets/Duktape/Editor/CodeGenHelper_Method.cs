using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public abstract class MethodBaseCodeGen<T> : IDisposable
    where T : MethodBase
    {
        protected CodeGenerator cg;

        // 方法参数数比较, 用于列表排序
        public static int MethodComparer(T a, T b)
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

        public string GetParamArrayMatchType(T method)
        {
            var parameters = method.GetParameters();
            var parameter = parameters[parameters.Length - 1];
            var typename = this.cg.bindingManager.GetCSTypeFullName(parameter.ParameterType.GetElementType());
            return $"typeof({typename})";
        }

        // 生成定参部分 type 列表
        public string GetFixedMatchTypes(T method)
        {
            var snippet = "";
            var parameters = method.GetParameters();
            for (int i = 0, length = parameters.Length; i < length; i++)
            {
                var parameter = parameters[i];
                var typename = this.cg.bindingManager.GetCSTypeFullName(parameter.ParameterType);
                snippet += $"typeof({typename})";
                if (parameter.IsDefined(typeof(ParamArrayAttribute), false))
                {
                    break;
                }
                if (i != length - 1)
                {
                    snippet += ", ";
                }
            }
            return snippet;
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
                if (parameter.IsOut && parameter.ParameterType.IsByRef)
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
                var argType = this.cg.bindingManager.GetCSTypeFullName(parameter.ParameterType);
                if (hasParams && i == parameters.Length - 1)
                {
                    // 处理数组
                    var argElementType = this.cg.bindingManager.GetCSTypeFullName(parameter.ParameterType.GetElementType());
                    var argElementIndex = i == 0 ? nargs : nargs + " - " + i;
                    this.cg.csharp.AppendLine($"{argType} arg{i} = null;");
                    this.cg.csharp.AppendLine($"if ({argElementIndex} > 0)");
                    this.cg.csharp.AppendLine("{");
                    this.cg.csharp.AddTabLevel();
                    {
                        this.cg.csharp.AppendLine($"arg{i} = new {argElementType}[{argElementIndex}];");
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

        // 输出所有变体绑定
        // hasOverrides: 是否需要处理重载
        protected void WriteAllVariants(MethodBaseBindingInfo<T> bindingInfo) // SortedDictionary<int, MethodBaseVariant<T>> variants)
        {
            var variants = bindingInfo.variants;
            var hasOverrides = bindingInfo.count > 1;
            if (hasOverrides)
            {
                // 需要处理重载
                GenMethodVariants(variants);
            }
            else
            {
                // 没有重载的情况 (variants.Count == 1)
                foreach (var variantKV in variants)
                {
                    var args = variantKV.Key;
                    var variant = variantKV.Value;
                    var argc = cg.AppendGetArgCount(variant.isVararg);

                    if (variant.isVararg)
                    {
                        var method = variant.varargMethods[0];
                        // Debug.Log($"varargMethods {method}");
                        WriteCSMethodBinding(method, argc, true);
                    }
                    else
                    {
                        var method = variant.plainMethods[0];
                        // Debug.Log($"plainMethods {method}");
                        WriteCSMethodBinding(method, argc, false);
                    }
                }
            }

            //TODO: 如果产生了无法在 typescript 中声明的方法, 则作标记, 并输出一条万能声明 
            //      [key: string]: any
            foreach (var variantKV in variants)
            {
                foreach (var method in variantKV.Value.plainMethods)
                {
                    WriteTSDeclaration(method, bindingInfo);
                }
                foreach (var method in variantKV.Value.varargMethods)
                {
                    WriteTSDeclaration(method, bindingInfo);
                }
            }
        }

        // 写入返回类型声明
        protected virtual void WriteTSReturn(T method, List<ParameterInfo> returnParameters)
        {
            //TODO: 如果存在 ref/out 参数， 则返回值改写为带定义的 object
            //      例如 foo(/*out*/ b: string): { b: string, ret: original_return_type }
            var returnType = GetReturnType(method);
            if (returnType != null)
            {
                var returnTypeTS = this.cg.bindingManager.GetTSTypeFullName(returnType);
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
                        var parameterTypeTS = this.cg.bindingManager.GetTSTypeFullName(parameterType);
                        this.cg.typescript.AppendLine($"{BindingManager.GetTSVariable(parameter.Name)}: {parameterTypeTS}, ");
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
            else
            {
                this.cg.typescript.AppendLine();
            }
        }

        protected void GenMethodVariants(SortedDictionary<int, MethodBaseVariant<T>> variants)
        {
            var argc = cg.AppendGetArgCount(true);
            cg.csharp.AppendLine("do");
            cg.csharp.AppendLine("{");
            cg.csharp.AddTabLevel();
            {
                foreach (var variantKV in variants)
                {
                    var args = variantKV.Key;
                    var variant = variantKV.Value;
                    //variant.count > 1
                    var gecheck = args > 0 && variant.isVararg; // 最后一组分支且存在变参时才需要判断 >= 
                    if (gecheck)
                    {
                        cg.csharp.AppendLine("if (argc >= {0})", args);
                        cg.csharp.AppendLine("{");
                        cg.csharp.AddTabLevel();
                    }
                    // 处理定参
                    if (variant.plainMethods.Count > 0)
                    {
                        cg.csharp.AppendLine("if (argc == {0})", args);
                        cg.csharp.AppendLine("{");
                        cg.csharp.AddTabLevel();
                        if (variant.plainMethods.Count > 1)
                        {
                            foreach (var method in variant.plainMethods)
                            {
                                cg.csharp.AppendLine($"if (duk_match_types(ctx, argc, {GetFixedMatchTypes(method)}))");
                                cg.csharp.AppendLine("{");
                                cg.csharp.AddTabLevel();
                                this.WriteCSMethodBinding(method, argc, false);
                                cg.csharp.DecTabLevel();
                                cg.csharp.AppendLine("}");
                            }
                            cg.csharp.AppendLine("break;");
                        }
                        else
                        {
                            // 只有一个定参方法时, 不再判定类型匹配
                            var method = variant.plainMethods[0];
                            this.WriteCSMethodBinding(method, argc, false);
                        }
                        cg.csharp.DecTabLevel();
                        cg.csharp.AppendLine("}");
                    }
                    // 处理变参
                    if (variant.varargMethods.Count > 0)
                    {
                        foreach (var method in variant.varargMethods)
                        {
                            cg.csharp.AppendLine($"if (duk_match_types(ctx, argc, {GetFixedMatchTypes(method)})");
                            cg.csharp.AppendLine($" && duk_match_param_types(ctx, {args}, argc, {GetParamArrayMatchType(method)}))");
                            cg.csharp.AppendLine("{");
                            cg.csharp.AddTabLevel();
                            this.WriteCSMethodBinding(method, argc, true);
                            cg.csharp.DecTabLevel();
                            cg.csharp.AppendLine("}");
                        }
                    }
                    if (gecheck)
                    {
                        cg.csharp.DecTabLevel();
                        cg.csharp.AppendLine("}");
                    }
                }
            }
            cg.csharp.DecTabLevel();
            cg.csharp.AppendLine("} while(false);");
            var error = this.cg.bindingManager.GetDuktapeGenericError("no matched method variant");
            cg.csharp.AppendLine($"return {error}");
        }

        //TODO: 考虑将 ref/out 参数以额外增加一个参数的形式返回
        protected List<ParameterInfo> WriteTSDeclaration(T method, MethodBaseBindingInfo<T> bindingInfo)
        {
            var refParameters = new List<ParameterInfo>();
            string tsMethodDeclaration;
            if (this.cg.bindingManager.GetTSMethodDeclaration(method, out tsMethodDeclaration))
            {
                this.cg.typescript.AppendLine(tsMethodDeclaration);
                return refParameters;
            }
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
                    var elementTS = this.cg.bindingManager.GetTSTypeFullName(elementType);
                    var parameterVarName = BindingManager.GetTSVariable(parameter.Name);
                    this.cg.typescript.AppendL($"{parameter_prefix}...{parameterVarName}: {elementTS}[]");
                }
                else
                {
                    var parameterType = parameter.ParameterType;
                    var parameterTS = this.cg.bindingManager.GetTSTypeFullName(parameterType);
                    var parameterVarName = BindingManager.GetTSVariable(parameter.Name);
                    this.cg.typescript.AppendL($"{parameter_prefix}{parameterVarName}: {parameterTS}");
                }
                if (i != parameters.Length - 1)
                {
                    this.cg.typescript.AppendL(", ");
                }
            }
            this.cg.typescript.AppendL($")");
            WriteTSReturn(method, refParameters);
            return refParameters;
        }

        // 获取返回值类型
        protected abstract Type GetReturnType(T method);

        // 获取方法调用
        protected abstract string GetInvokeBinding(string caller, T method, string arglist);

        protected virtual void BeginInvokeBinding() { }

        protected virtual void EndInvokeBinding() { }

        // 写入绑定代码
        //TODO: 如果是扩展方法且第一参数不是本类型, 则是因为目标类没有导出而降级的普通静态方法, 按普通静态方法处理
        protected void WriteCSMethodBinding(T method, string argc, bool isVararg)
        {
            var parameters = method.GetParameters();
            var returnParameters = new List<ParameterInfo>();
            var caller = this.cg.AppendGetThisCS(method);
            var arglist = this.AppendGetParameters(isVararg, argc, parameters, returnParameters);
            var returnType = GetReturnType(method);

            if (returnType == null || returnType == typeof(void))
            {
                // 方法本身没有返回值
                this.BeginInvokeBinding();
                cg.csharp.AppendLine($"{this.GetInvokeBinding(caller, method, arglist)};");
                this.EndInvokeBinding();
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
                this.BeginInvokeBinding();
                cg.csharp.AppendLine($"var ret = {this.GetInvokeBinding(caller, method, arglist)};");
                this.EndInvokeBinding();
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
    }

    public class ConstructorCodeGen : MethodBaseCodeGen<ConstructorInfo>
    {
        private ConstructorBindingInfo bindingInfo;

        protected override Type GetReturnType(ConstructorInfo method)
        {
            return null;
        }

        protected override string GetInvokeBinding(string caller, ConstructorInfo method, string arglist)
        {
            var decalringTypeName = this.cg.bindingManager.GetCSTypeFullName(this.bindingInfo.decalringType);
            return $"var o = new {decalringTypeName}({arglist})";
        }

        protected override void EndInvokeBinding()
        {
            this.cg.csharp.AppendLine("duk_bind_native(ctx, o);");
        }

        public ConstructorCodeGen(CodeGenerator cg, ConstructorBindingInfo bindingInfo)
        : base(cg)
        {
            this.bindingInfo = bindingInfo;
            if (this.bindingInfo.count > 0)
            {
                WriteAllVariants(this.bindingInfo);
            }
            else
            {
                WriteDefaultConstructorBinding();
            }
        }

        // 写入默认构造函数 (struct 无参构造)
        private void WriteDefaultConstructorBinding()
        {
            var decalringTypeName = this.cg.bindingManager.GetCSTypeFullName(this.bindingInfo.decalringType);
            this.cg.csharp.AppendLine($"var o = new {decalringTypeName}();");
            this.cg.csharp.AppendLine("duk_bind_native(ctx, o);");
            this.cg.csharp.AppendLine("return 0;");

            this.cg.typescript.AppendLine($"{this.bindingInfo.regName}()");
        }
    }

    // 生成成员方法绑定代码
    public class MethodCodeGen : MethodBaseCodeGen<MethodInfo>
    {
        protected MethodBindingInfo bindingInfo;

        protected override Type GetReturnType(MethodInfo method)
        {
            return method.ReturnType;
        }

        protected override string GetInvokeBinding(string caller, MethodInfo method, string arglist)
        {
            return $"{caller}.{method.Name}({arglist})";
        }

        public MethodCodeGen(CodeGenerator cg, MethodBindingInfo bindingInfo)
        : base(cg)
        {
            this.bindingInfo = bindingInfo;
            WriteAllVariants(this.bindingInfo);
        }
    }
}
