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
            var bm = new BindingManager(Prefs.Load(Prefs.PATH));
            bm.Collect();
            // temp
            // bm.AddExport(typeof(GameObject));
            // bm.AddExport(typeof(Transform));
            bm.Generate();
            bm.Cleanup();
            bm.Report();
            AssetDatabase.Refresh();
        }

        [MenuItem("Duktape/Compile TypeScript")]
        public static void CompileScripts()
        {
#if UNITY_EDITOR_WIN
            string command = "tsc.cmd";
#else
            string command = "tsc";
#endif

            var exitCode = ShellHelper.Run(command, "", 30);
            Debug.Log($"{command}: {exitCode}");
        }

        [MenuItem("Duktape/Clear")]
        public static void ClearBindings()
        {
            BindingManager.Cleanup(Prefs.Load(Prefs.PATH).outDir, null, null);
            AssetDatabase.Refresh();
        }

        [MenuItem("Duktape/Prefs ...")]
        public static void OpenPrefsEditor()
        {
            EditorWindow.GetWindow<PrefsEditor>().Show();
        }
        #endregion

        public class TypeScriptPostProcessor : AssetPostprocessor
        {
            private static bool IsScriptSourceFile(string filename)
            {
                return filename.EndsWith(".ts") || filename.EndsWith(".js") || filename.EndsWith(".js.txt");
            }

            private static bool CheckAssets(string[] assetPaths)
            {
                foreach (var assetPath in assetPaths)
                {
                    if (IsScriptSourceFile(assetPath))
                    {
                        return true;
                    }
                }
                return false;
            }

            private static void OnPostprocessAllAssets(
                string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                if (CheckAssets(importedAssets) ||
                    CheckAssets(deletedAssets) ||
                    CheckAssets(movedAssets) ||
                    CheckAssets(movedFromAssetPaths))
                {
                    UnityHelper.CompileScripts();
                }
            }
        }
    }
}
