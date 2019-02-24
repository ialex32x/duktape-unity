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

            var caller = this.cg.AppendGetThisCS(bindingInfo.propertyInfo.GetMethod);

            this.cg.csharp.AppendLine("var ret = {0}.{1};", caller, bindingInfo.propertyInfo.Name);
            this.cg.csharp.AppendLine("{0}(ctx, ret);", this.cg.GetDuktapePusher(bindingInfo.propertyInfo.PropertyType));
            this.cg.csharp.AppendLine("return 1;");
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

            var caller = this.cg.AppendGetThisCS(bindingInfo.propertyInfo.GetMethod);
            var propertyInfo = this.bindingInfo.propertyInfo;
            
            this.cg.csharp.AppendLine("{0} value;", this.cg.bindingManager.GetTypeFullNameCS(propertyInfo.PropertyType));
            this.cg.csharp.AppendLine("{0}(ctx, 0, out value);", this.cg.GetDuktapeGetter(propertyInfo.PropertyType));
            this.cg.csharp.AppendLine("{0}.{1} = value;", caller, propertyInfo.Name);
            this.cg.csharp.AppendLine("return 0;");
        }

        public virtual void Dispose()
        {
        }
    }
}
