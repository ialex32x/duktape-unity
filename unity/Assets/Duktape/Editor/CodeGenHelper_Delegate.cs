using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class DelegateCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        public DelegateCodeGen(CodeGenerator cg, DelegateBindingInfo delegateBindingInfo, int index)
        {
            this.cg = cg;
            var returnTypeName = this.cg.bindingManager.GetTypeFullNameCS(delegateBindingInfo.returnType);
            var delegateName = DuktapeVM._DuktapeDelegates + index;
            var arglist = this.cg.GetParametersDeclCS(delegateBindingInfo.parameters);
            foreach (var target in delegateBindingInfo.types)
            {
                this.cg.csharp.AppendLine("[{0}(typeof({1}))]", typeof(JSDelegateAttribute).FullName, target.FullName.Replace('+', '.'));
            }
            this.cg.csharp.AppendLine("public static {0} {1}(DuktapeFunction fn{2}) {{", returnTypeName, delegateName, string.IsNullOrEmpty(arglist) ? "" : ", " + arglist);
            this.cg.csharp.AddTabLevel();

            this.cg.csharp.AppendLine("// generate binding code here");
            this.cg.csharp.AppendLine("// var ctx = fn.GetContext().rawValue;");
            this.cg.csharp.AppendLine("// fn.Push(ctx);");
            this.cg.csharp.AppendLine("// push arguments here...");
            this.cg.csharp.AppendLine("// fn._InternalCall(ctx, {0});", delegateBindingInfo.parameters.Length);

            if (delegateBindingInfo.returnType != typeof(void))
            {
                this.cg.csharp.AppendLine("return ret;");
            }
        }

        public void Dispose()
        {
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }
    }
}
