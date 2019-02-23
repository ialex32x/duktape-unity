using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class PropertyGetterCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected PropertyBindingInfo bindingInfo;

        public PropertyGetterCodeGen(CodeGenerator cg, PropertyBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            if (bindingInfo.propertyInfo.GetMethod.IsStatic)
            {
                this.cg.csharp.AppendLine("var ret = {0}.{1};", bindingInfo.propertyInfo.DeclaringType, bindingInfo.propertyInfo.Name);
                this.cg.csharp.AppendLine("duk_push_any(ctx, ret);");
                this.cg.csharp.AppendLine("return 1;");
            }
            else
            {
                this.cg.csharp.AppendLine("var self = ({0})duk_get_this(ctx);", bindingInfo.propertyInfo.DeclaringType);
                this.cg.csharp.AppendLine("var ret = self.{0};", bindingInfo.propertyInfo.Name);
                this.cg.csharp.AppendLine("duk_push_any(ctx, ret);");
                this.cg.csharp.AppendLine("return 1;");
            }
        }

        public virtual void Dispose()
        {
        }
    }

    public class PropertySetterCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected PropertyBindingInfo bindingInfo;

        public PropertySetterCodeGen(CodeGenerator cg, PropertyBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            if (bindingInfo.propertyInfo.SetMethod.IsStatic)
            {
                this.cg.csharp.AppendLine("var value = ({0})duk_get_primitive(ctx, 0);", bindingInfo.propertyInfo.PropertyType);
                this.cg.csharp.AppendLine("{0}.{1} = value;", bindingInfo.propertyInfo.DeclaringType, bindingInfo.propertyInfo.Name);
                this.cg.csharp.AppendLine("return 0;");
            }
            else
            {
                this.cg.csharp.AppendLine("var value = ({0})duk_get_primitive(ctx, 0);", bindingInfo.propertyInfo.PropertyType);
                this.cg.csharp.AppendLine("var self = ({0})duk_get_this(ctx);", bindingInfo.propertyInfo.DeclaringType);
                this.cg.csharp.AppendLine("self.{0} = value;", bindingInfo.propertyInfo.Name);
                this.cg.csharp.AppendLine("return 0;");
            }
        }

        public virtual void Dispose()
        {
        }
    }
}
