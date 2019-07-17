using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class EventAdderCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected EventBindingInfo bindingInfo;

        public EventAdderCodeGen(CodeGenerator cg, EventBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            // var caller = this.cg.AppendGetThisCS(bindingInfo);
            // this.cg.cs.AppendLine("var ret = {0}.{1};", caller, bindingInfo.eventInfo.Name);
            // this.cg.AppendPushValue(bindingInfo.eventInfo.EventHandlerType, "ret");
            // this.cg.cs.AppendLine("return 1;");
            //TODO: 没完成
            this.cg.cs.AppendLine("//TODO: 没完成");
            this.cg.cs.AppendLine("return 0;");
        }

        public virtual void Dispose()
        {
        }
    }

    public class EventRemoverCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected EventBindingInfo bindingInfo;

        public EventRemoverCodeGen(CodeGenerator cg, EventBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;

            //TODO: 没完成
            // var caller = this.cg.AppendGetThisCS(bindingInfo);
            // var eventInfo = bindingInfo.eventInfo;
            // var declaringType = bindingInfo.declaringType;
            // this.cg.cs.AppendLine("{0} value;", this.cg.bindingManager.GetCSTypeFullName(eventInfo.EventHandlerType));
            // this.cg.cs.AppendLine("{0}(ctx, 0, out value);", this.cg.bindingManager.GetDuktapeGetter(eventInfo.EventHandlerType));
            // this.cg.cs.AppendLine("{0}.{1} = value;", caller, eventInfo.Name);
            // if (declaringType.IsValueType && !bindingInfo.isStatic)
            // {
            //     // 非静态结构体字段修改, 尝试替换实例
            //     this.cg.cs.AppendLine($"duk_rebind_this(ctx, {caller});");
            // }
            this.cg.cs.AppendLine("//TODO: 没完成");
            this.cg.cs.AppendLine("return 0;");
        }

        public virtual void Dispose()
        {
        }
    }
}
