using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class UnityHelper
    {
        #region All Menu Items
        [MenuItem("Duktape/Generate Bindings")]
        public static void GenerateBindings()
        {
            var bm = new BindingManager();
            bm.Collect();
            // temp
            // bm.AddExport(typeof(GameObject));
            // bm.AddExport(typeof(Transform));
            bm.Generate();
            bm.Cleanup();
            AssetDatabase.Refresh();
        }

        [MenuItem("Duktape/Compile TypeScript (tsc)")]
        public static void ExecTypeScriptCompilation()
        {
            var tsc = Application.platform == RuntimePlatform.WindowsEditor ? "tsc.cmd" : "tsc";
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.FileName = tsc;
            proc.StartInfo.Arguments = "";
            proc.StartInfo.CreateNoWindow = true;

            proc.Start();
            proc.WaitForExit();

            var output = proc.StandardOutput.ReadToEnd();
            var error = proc.StandardError.ReadToEnd();

            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogErrorFormat("tsc: {0}", error);
            }
            else
            {
                Debug.Log("tsc: done");
            }
        }

        [MenuItem("Duktape/Prefs ...")]
        public static void OpenPrefsEditor()
        {
            EditorWindow.GetWindow<PrefsEditor>().Show();
        }
        #endregion
    }
}
