using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    // 配置编辑器
    public class PrefsEditor : EditorWindow
    {
        void OnGUI()
        {
            if (GUILayout.Button("Generate"))
            {
                UnityHelper.GenerateBindings();
            }
            if(GUILayout.Button("Clear"))
            {
                UnityHelper.ClearBindings();
            }
        }
    }
}