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

            if (bindingInfo.isStatic)
            {
                this.cg.csharp.AppendLine("var ret = {0}.{1};", bindingInfo.fieldInfo.DeclaringType, bindingInfo.fieldInfo.Name);
                this.cg.csharp.AppendLine("duk_push_any(ctx, ret);");
                this.cg.csharp.AppendLine("return 1;");
            }
            else
            {
                this.cg.csharp.AppendLine("var self = ({0})duk_get_this(ctx);", bindingInfo.fieldInfo.DeclaringType);
                this.cg.csharp.AppendLine("var ret = self.{0};", bindingInfo.fieldInfo.Name);
                this.cg.csharp.AppendLine("duk_push_any(ctx, ret);");
                this.cg.csharp.AppendLine("return 1;");
            }
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

            if (bindingInfo.isStatic)
            {
                this.cg.csharp.AppendLine("var value = ({0})duk_get_primitive(ctx, 0);", bindingInfo.fieldInfo.FieldType);
                this.cg.csharp.AppendLine("{0}.{1} = value;", bindingInfo.fieldInfo.DeclaringType, bindingInfo.fieldInfo.Name);
                this.cg.csharp.AppendLine("return 0;");
            }
            else
            {
                this.cg.csharp.AppendLine("var value = ({0})duk_get_primitive(ctx, 0);", bindingInfo.fieldInfo.FieldType);
                this.cg.csharp.AppendLine("var self = ({0})duk_get_this(ctx);", bindingInfo.fieldInfo.DeclaringType);
                this.cg.csharp.AppendLine("self.{0} = value;", bindingInfo.fieldInfo.Name);
                this.cg.csharp.AppendLine("return 0;");
            }
        }

        public virtual void Dispose()
        {
        }
    }
}
