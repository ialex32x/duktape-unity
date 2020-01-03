
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace MyProject
{
    using UnityEngine;
    using UnityEditor;
    using Duktape;

    [CustomEditor(typeof(Sample))]
    public class SampleEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Reload"))
            {
                (target as Sample).OnSourceModified();
            }
        }
    }
}
