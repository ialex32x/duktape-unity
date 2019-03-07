
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace MyProject
{
    using UnityEngine;
    using UnityEditor;
    using Duktape;
    
    public class MyCustomBinding : AbstractBindingProcess
    {
        public override void OnPreCollectMembers(BindingManager bindingManager)
        {
            // 添加导出
            // bindingManager.AddExport(typeof(MyCustomClass));
        }

        public override void OnCleanup(BindingManager bindingManager)
        {
            Debug.Log($"finish @ {DateTime.Now}");
        }
    }
}
