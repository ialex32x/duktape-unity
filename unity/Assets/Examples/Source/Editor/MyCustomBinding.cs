
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
        public override void OnPreCollectTypes(BindingManager bindingManager)
        {
            // 添加导出
            bindingManager.AddExportedType(typeof(List<String>));
            // bindingManager.AddExportedType(typeof(Dictionary<String, String>))
            //     .SetMethodBlocked("Remove", typeof(string), typeof(string));
            //     ...
        }

        public override void OnCleanup(BindingManager bindingManager)
        {
            Debug.Log($"finish @ {DateTime.Now}");
        }
    }
}
