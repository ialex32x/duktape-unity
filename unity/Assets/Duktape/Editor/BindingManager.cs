using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class BindingManager
    {
        private List<Type> types = new List<Type>();

        public void AddExport(Type type)
        {
            types.Add(type);
        }

        public bool IsExported(Type type)
        {
            return types.Contains(type);
        }

        // 将类型名转换成简单字符串 (比如用于文件名)
        public string GetFileName(Type type)
        {
            return type.FullName.Replace(".", "_");
        }

        public void Collect()
        {
            // project assembly
            Collect(new string[]{
                "Assembly-CSharp-firstpass",
                "Assembly-CSharp",
            }, false);
            // unity assembly
            Collect(new string[]{
                "UnityEngine",
                "UnityEngine.CoreModule",
                "UnityEngine.UIModule",
                "UnityEngine.TextRenderingModule",
                "UnityEngine.TextRenderingModule",
                "UnityEngine.UnityWebRequestWWWModule",
                "UnityEngine.Physics2DModule",
                "UnityEngine.AnimationModule",
                "UnityEngine.TextRenderingModule",
                "UnityEngine.IMGUIModule",
                "UnityEngine.UnityWebRequestModule",
                "UnityEngine.PhysicsModule",
                "UnityEngine.UI",
            }, true);
        }

        // implicitExport: 默认进行导出(黑名单例外), 否则根据导出标记或手工添加
        public void Collect(string[] assemblyNames, bool implicitExport)
        {
            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    var types = assembly.GetExportedTypes();

                    Debug.LogFormat("collecting assembly {0}: {1}", assemblyName, types.Length);
                    foreach (var type in types)
                    {
                        //TODO: filter for exporting
                        if (implicitExport)
                        {
                            
                        }
                        this.AddExport(type);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(exception);
                }
            }
        }

        public void Generate()
        {
            var cg = new CodeGenerator(this);
            var outDir = Prefs.GetPrefs().outDir;
            var tx = ".txt";
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }
            foreach (var type in types)
            {
                cg.Generate(type);
                cg.WriteTo(outDir, GetFileName(type), tx);
            }
        }
    }
}