using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class ConstructorCodeGen : IDisposable
    {
        private CodeGenerator cg;
        private ConstructorBindingInfo bindingInfo;

        public ConstructorCodeGen(CodeGenerator cg, ConstructorBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            if (this.bindingInfo.variants.Count > 0)
            {
                foreach (var constructor in this.bindingInfo.variants)
                {
                    WriteCSConstructor(constructor);
                    WriteTSDeclaration(constructor);
                }
            }
            else
            {
                WriteDefaultCSConstructor();
                WriteDefaultTSDeclaration();
            }
            this.cg.csharp.AppendLine("return 0;");
        }

        public void Dispose()
        {

        }

        private void WriteCSConstructor(ConstructorInfo constructor)
        {
            //TODO: 写入构造函数
            var parameters = constructor.GetParameters();
            if (parameters.Length == 0)
            {
                WriteDefaultCSConstructor();
            }
            else
            {
                var arglist = this.cg.AppendGetParameters(parameters, null);

                this.cg.csharp.AppendLine("var o = new {0}({1});", this.bindingInfo.decalringType.FullName, arglist);
                this.cg.csharp.AppendLine("DuktapeDLL.duk_push_this(ctx);");
                this.cg.csharp.AppendLine("duk_bind_native(ctx, -1, o);");
                this.cg.csharp.AppendLine("DuktapeDLL.duk_pop(ctx);");
            }
        }

        private void WriteDefaultCSConstructor()
        {
            //TODO: 写入默认构造函数 (struct 无参构造)
            this.cg.csharp.AppendLine("var o = new {0}();", this.bindingInfo.decalringType.FullName);
            this.cg.csharp.AppendLine("DuktapeDLL.duk_push_this(ctx);");
            this.cg.csharp.AppendLine("duk_bind_native(ctx, -1, o);");
            this.cg.csharp.AppendLine("DuktapeDLL.duk_pop(ctx);");
        }

        private void WriteDefaultTSDeclaration()
        {
            this.cg.typescript.AppendLine("{0}()", this.bindingInfo.regName);
        }

        private void WriteTSDeclaration(ConstructorInfo constructor)
        {
            //TODO: 需要处理参数类型归并问题, 因为如果类型没有导入 ts 中, 可能会在声明中出现相同参数列表的定义
            //      在 MethodVariant 中创建每个方法对应的TS类型名参数列表, 完全相同的不再输出
            var prefix = "";
            this.cg.typescript.Append("{0}{1}(", prefix, this.bindingInfo.regName);
            var parameters = constructor.GetParameters();
            var returnParameters = new List<ParameterInfo>();
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                var parameter_prefix = "";
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
            this.cg.typescript.AppendL(")");
            this.cg.typescript.AppendLine();
        }
    }
}