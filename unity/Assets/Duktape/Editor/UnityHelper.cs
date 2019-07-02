using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    [Serializable]
    public class TSConfig
    {
        [Serializable]
        public class CompilerOptions
        {
            public string module;
            public string target;
            public string sourceRoot;
            public string outDir;
            public string outFile;
            public string[] typeRoots;
            public string moduleResolution;
            public string[] types;
            public bool listEmittedFiles;
            public bool experimentalDecorators;
            public bool noImplicitAny;
            public bool allowJs;
            public bool inlineSourceMap;
            public bool sourceMap;
        }
        public CompilerOptions compilerOptions;
        public bool compileOnSave;
        public string[] include;
        public string[] exclude;
    }

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

        //[MenuItem("Duktape/Compile TypeScript")]
        public static void CompileScripts()
        {
            Debug.Log("compiling typescript source...");
            EditorApplication.delayCall += () =>
            {
                EditorApplication.delayCall += () =>
                {
#if UNITY_EDITOR_WIN
                    string command = "tsc.cmd";
#else
                    string command = "tsc";
#endif
                    var exitCode = ShellHelper.Run(command, "", 30);
                    Debug.Log($"{command}: {exitCode}");
                };
            };
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

            private static bool CheckAssets(string outDir, string[] assetPaths)
            {
                foreach (var assetPath in assetPaths)
                {
                    if (outDir == null || !assetPath.StartsWith(outDir, StringComparison.OrdinalIgnoreCase)) // skip output files
                    {
                        if (IsScriptSourceFile(assetPath))
                        {
                            // Debug.Log(assetPath);
                            return true;
                        }
                    }
                }
                return false;
            }

            // 剔除行注释
            private static string NormalizeJson(string json)
            {
                var outstr = new StringBuilder();
                var state = 0;
                for (int i = 0; i < json.Length; i++)
                {
                    if (state == 0)
                    {
                        if (json[i] == '/')
                        {
                            state = 1;
                            continue;
                        }
                    }
                    else if (state == 1)
                    {
                        if (json[i] == '/')
                        {
                            state = 2;
                            continue;
                        }
                        state = 0;
                        outstr.Append('/');
                    }
                    else if (state == 2)
                    {
                        if (json[i] != '\n')
                        {
                            continue;
                        }
                        state = 0;
                    }
                    outstr.Append(json[i]);
                }
                return outstr.ToString();
            }

            private static void OnPostprocessAllAssets(
                string[] importedAssets,
                string[] deletedAssets,
                string[] movedAssets,
                string[] movedFromAssetPaths)
            {
                if (!File.Exists("tsconfig.json"))
                {
                    // no typescript context
                    return;
                }
                string outDir = null;
                try
                {
                    var text = NormalizeJson(File.ReadAllText("tsconfig.json"));
                    var tsconfig = JsonUtility.FromJson<TSConfig>(text);
                    outDir = tsconfig.compilerOptions.outDir;
                }
                catch (Exception exception) { Debug.LogWarning(exception); }
                if (CheckAssets(outDir, importedAssets) ||
                    CheckAssets(outDir, deletedAssets) ||
                    CheckAssets(outDir, movedAssets) ||
                    CheckAssets(outDir, movedFromAssetPaths))
                {
                    UnityHelper.CompileScripts();
                }
            }
        }
    }
}
