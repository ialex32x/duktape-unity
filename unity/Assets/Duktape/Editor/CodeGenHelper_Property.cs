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

            var method = bindingInfo.propertyInfo.SetMethod;
            var caller = this.cg.AppendGetThisCS(method);
            var propertyInfo = this.bindingInfo.propertyInfo;
            var declaringType = propertyInfo.DeclaringType;
            
            this.cg.csharp.AppendLine("{0} value;", this.cg.bindingManager.GetTypeFullNameCS(propertyInfo.PropertyType));
            this.cg.csharp.AppendLine("{0}(ctx, 0, out value);", this.cg.GetDuktapeGetter(propertyInfo.PropertyType));
            this.cg.csharp.AppendLine("{0}.{1} = value;", caller, propertyInfo.Name);
            if (declaringType.IsValueType && !method.IsStatic)
            {
                // 非静态结构体属性修改, 尝试替换实例
                this.cg.csharp.AppendLine("duk_rebind_this(ctx, {0});", caller);
            }
            this.cg.csharp.AppendLine("return 0;");
        }

        public virtual void Dispose()
        {
        }
    }
}
