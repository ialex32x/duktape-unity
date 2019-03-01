using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class DelegateWrapperCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected DelegateBindingInfo[] delegateBindingInfos;

        public DelegateWrapperCodeGen(CodeGenerator cg, DelegateBindingInfo[] delegateBindingInfos)
        {
            this.cg = cg;
            this.delegateBindingInfos = delegateBindingInfos;
            this.cg.csharp.AppendLine("[{0}({1})]", typeof(JSBindingAttribute).Name, DuktapeVM.VERSION);
            this.cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.csharp.AppendLine("public class {0} : {1} {{", DuktapeVM._DuktapeDelegates, typeof(DuktapeBinding).Name);
            this.cg.csharp.AddTabLevel();
        }

        public void Dispose()
        {
            using (new PreservedCodeGen(cg))
            {
                using (new RegFuncCodeGen(cg))
                {
                    this.cg.csharp.AppendLine("var type = typeof({0});", DuktapeVM._DuktapeDelegates);
                    this.cg.csharp.AppendLine("var vm = DuktapeVM.GetVM(ctx);");
                    this.cg.csharp.AppendLine("var methods = type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);");
                    this.cg.csharp.AppendLine("for (int i = 0, size = methods.Length; i < size; i++)");
                    this.cg.csharp.AppendLine("{");
                    {
                        this.cg.csharp.AddTabLevel();
                        this.cg.csharp.AppendLine("var method = methods[i];");
                        this.cg.csharp.AppendLine("var attributes = method.GetCustomAttributes(typeof(JSDelegateAttribute), false);");
                        this.cg.csharp.AppendLine("var attributesLength = attributes.Length;");
                        this.cg.csharp.AppendLine("if (attributesLength > 0)");
                        this.cg.csharp.AppendLine("{");
                        {
                            this.cg.csharp.AddTabLevel();
                            this.cg.csharp.AppendLine("for (var a = 0; a < attributesLength; a++)");
                            this.cg.csharp.AppendLine("{");
                            {
                                this.cg.csharp.AddTabLevel();
                                this.cg.csharp.AppendLine("var attribute = attributes[a] as JSDelegateAttribute;");
                                this.cg.csharp.AppendLine("vm.AddDelegate(attribute.target, method);");
                                this.cg.csharp.DecTabLevel();
                            }
                            this.cg.csharp.AppendLine("}");
                            this.cg.csharp.DecTabLevel();
                        }
                        this.cg.csharp.AppendLine("}");
                        this.cg.csharp.DecTabLevel();
                    }
                    this.cg.csharp.AppendLine("}");


                    // for (var i = 0; i < delegateBindingInfos.Length; i++)
                    // {
                    //     var delegateBindingInfo = delegateBindingInfos[i];
                    // }
                }
            }
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }
    }

    public class DelegateCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        public DelegateCodeGen(CodeGenerator cg, DelegateBindingInfo delegateBindingInfo, int index)
        {
            this.cg = cg;
            var nargs = delegateBindingInfo.parameters.Length;
            var retName = this.cg.bindingManager.GetUniqueName(delegateBindingInfo.parameters, "ret");
            var firstArgument = typeof(DuktapeDelegate) + " fn";
            var returnTypeName = this.cg.bindingManager.GetTypeFullNameCS(delegateBindingInfo.returnType);
            var delegateName = DuktapeVM._DuktapeDelegates + index;
            var arglist = this.cg.bindingManager.GetArglistDeclCS(delegateBindingInfo.parameters);
            foreach (var target in delegateBindingInfo.types)
            {
                this.cg.csharp.AppendLine("[{0}(typeof({1}))]", typeof(JSDelegateAttribute).FullName, this.cg.bindingManager.GetTypeFullNameCS(target));
            }
            if (!string.IsNullOrEmpty(arglist))
            {
                arglist = ", " + arglist;
            }
            this.cg.csharp.AppendLine($"public static {returnTypeName} {delegateName}({firstArgument}{arglist}) {{");
            this.cg.csharp.AddTabLevel();

            this.cg.csharp.AppendLine("var ctx = fn.GetContext().rawValue;");
            if (nargs > 0)
            {
                this.cg.csharp.AppendLine("fn.BeginInvoke(ctx);");
                for (var i = 0; i < nargs; i++)
                {
                    var parameter = delegateBindingInfo.parameters[i];
                    this.cg.AppendPushValue(parameter.ParameterType, parameter.Name);
                }
                this.cg.csharp.AppendLine("fn.EndInvoke(ctx);");
            }
            else
            {
                this.cg.csharp.AppendLine("fn.Invoke(ctx);");
            }
            if (delegateBindingInfo.returnType != typeof(void))
            {
                this.cg.csharp.AppendLine($"{this.cg.bindingManager.GetTypeFullNameCS(delegateBindingInfo.returnType)} {retName};");
                this.cg.csharp.AppendLine($"{this.cg.bindingManager.GetDuktapeGetter(delegateBindingInfo.returnType)}(ctx, -1, out {retName});");
                this.cg.csharp.AppendLine($"return {retName};");
            }
        }

        public void Dispose()
        {
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }
    }
}
