using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class FieldGetterCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected FieldBindingInfo bindingInfo;

        public FieldGetterCodeGen(CodeGenerator cg, FieldBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            var caller = this.cg.AppendGetThisCS(bindingInfo);

            this.cg.csharp.AppendLine("var ret = {0}.{1};", caller, bindingInfo.fieldInfo.Name);
            this.cg.csharp.AppendLine("{0}(ctx, ret);", this.cg.GetDuktapePusher(bindingInfo.fieldInfo.FieldType));
            this.cg.csharp.AppendLine("return 1;");
        }

        public virtual void Dispose()
        {
        }
    }

    public class FieldSetterCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected FieldBindingInfo bindingInfo;

        public FieldSetterCodeGen(CodeGenerator cg, FieldBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            var caller = this.cg.AppendGetThisCS(bindingInfo);
            var fieldInfo = bindingInfo.fieldInfo;

            this.cg.csharp.AppendLine("{0} value;", this.cg.bindingManager.GetTypeFullNameCS(fieldInfo.FieldType));
            this.cg.csharp.AppendLine("{0}(ctx, 0, out value);", this.cg.GetDuktapeGetter(fieldInfo.FieldType));
            this.cg.csharp.AppendLine("{0}.{1} = value;", caller, fieldInfo.Name);
            this.cg.csharp.AppendLine("return 0;");
        }

        public virtual void Dispose()
        {
        }
    }
}
