using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public interface IBindingProcess
    {
        // 初始化阶段回调, 可以调用 AddTSMethodDeclaration, AddTSKeywords 等进行定制
        void OnInitialize(BindingManager bindingManager);

        // 收集类型阶段开始, 可在该阶段 AddExported 增加导出类型
        void OnPreCollect(BindingManager bindingManager);

        //
        void OnPostCollect(BindingManager bindingManager);

        // 生成指定类型绑定代码前
        void OnPreGenerateType(BindingManager bindingManager, TypeBindingInfo bindingInfo);
        
        // 生成指定类型绑定代码后
        void OnPostGenerateType(BindingManager bindingManager, TypeBindingInfo bindingInfo);
        
        // 生成指定Delegate类型的绑定代码前
        void OnPreGenerateDelegate(BindingManager bindingManager, DelegateBindingInfo bindingInfo);

        // 生成指定Delegate类型的绑定代码后
        void OnPostGenerateDelegate(BindingManager bindingManager, DelegateBindingInfo bindingInfo);
        
        // 完成默认清理行为后 
        void OnCleanup(BindingManager bindingManager);
    }

    public abstract class AbstractBindingProcess : IBindingProcess
    {
        public virtual void OnInitialize(BindingManager bindingManager)
        {
        }

        public virtual void OnPreCollect(BindingManager bindingManager)
        {
        }

        public virtual void OnPostCollect(BindingManager bindingManager)
        {
        }

        public virtual void OnPreGenerateType(BindingManager bindingManager, TypeBindingInfo bindingInfo)
        {
        }

        public virtual void OnPostGenerateType(BindingManager bindingManager, TypeBindingInfo bindingInfo)
        {
        }

        public virtual void OnPreGenerateDelegate(BindingManager bindingManager, DelegateBindingInfo bindingInfo)
        {
        }

        public virtual void OnPostGenerateDelegate(BindingManager bindingManager, DelegateBindingInfo bindingInfo)
        {
        }

        public virtual void OnCleanup(BindingManager bindingManager)
        {
        }
    }
}
