using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class DuktapeStatsWindow : EditorWindow
    {
        [MenuItem("Duktape/Stats Viewer")]
        static void OpenThis()
        {
            GetWindow<DuktapeStatsWindow>().Show();
        }

        void OnEnable()
        {
            titleContent = new GUIContent("Duktape Stats");
        }

        void OnGUI()
        {
            var vm = DuktapeVM.GetInstance();

            if (vm == null)
            {
                EditorGUILayout.HelpBox("No Running VM", MessageType.Info);
                return;
            }

            uint objectCount;
            uint allocBytes;
            uint poolBytes;
            vm.GetMemoryState(out objectCount, out allocBytes, out poolBytes);
            EditorGUILayout.IntField("Objects", (int)objectCount);
            if (allocBytes > 1024 * 1024 * 2)
            {
                EditorGUILayout.FloatField("Allocated Memory (MB)", (float)allocBytes / 1024f / 1024f);
            }
            else if (allocBytes > 1024 * 2)
            {
                EditorGUILayout.FloatField("Allocated Memory (KB)", (float)allocBytes / 1024f);
            }
            else
            {
                EditorGUILayout.IntField("Allocated Memory", (int)allocBytes);
            }

            if (poolBytes != 0)
            {
                EditorGUILayout.FloatField("Used (%)", (float)allocBytes * 100f / poolBytes);
            }
        }
    }
}
