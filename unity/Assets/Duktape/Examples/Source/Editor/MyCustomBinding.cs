
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
        public override void OnPreExporting(BindingManager bindingManager)
        {
            // 添加导出
            bindingManager.AddExportedType(typeof(List<String>));

            bindingManager.AddExportedType(typeof(UnityEngine.Vector2));
            bindingManager.AddExportedType(typeof(UnityEngine.Vector3));
            bindingManager.AddExportedType(typeof(UnityEngine.Quaternion));
            bindingManager.AddExportedType(typeof(UnityEngine.GameObject), true);
            bindingManager.AddExportedType(typeof(UnityEngine.Mathf));
            bindingManager.AddExportedType(typeof(UnityEngine.PrimitiveType));
            bindingManager.AddExportedType(typeof(UnityEngine.Color));
            bindingManager.AddExportedType(typeof(UnityEngine.MonoBehaviour), true)
                .SetMemberBlocked("runInEditMode");
            bindingManager.AddExportedType(typeof(UnityEngine.Transform), true);
            bindingManager.AddExportedType(typeof(UnityEngine.UI.Text), true)
                .SetMemberBlocked("OnRebuildRequested");
            bindingManager.AddExportedType(typeof(UnityEngine.UI.Graphic))
                .SetMemberBlocked("OnRebuildRequested");
            bindingManager.AddExportedType(typeof(UnityEngine.UI.Button), true);
            bindingManager.AddExportedType(typeof(UnityEngine.UI.Button.ButtonClickedEvent), true);
            bindingManager.AddExportedType(typeof(UnityEngine.Random));
            bindingManager.AddExportedType(typeof(UnityEngine.Camera), true);
            bindingManager.AddExportedType(typeof(UnityEngine.Time));
            bindingManager.AddExportedType(typeof(UnityEngine.Input))
                .SetMemberBlocked("IsJoystickPreconfigured");
            
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
