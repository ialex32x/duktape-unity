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
        private HashSet<Type> blacklist;
        private HashSet<Type> whitelist;
        private List<Type> types = new List<Type>();
        private List<string> outputFiles = new List<string>();

        public BindingManager()
        {
            blacklist = new HashSet<Type>(new Type[]
            {
                typeof(AOT.MonoPInvokeCallbackAttribute),
            });
            whitelist = new HashSet<Type>(new Type[]
            {
            });
        }

        public void AddExport(Type type)
        {
            types.Add(type);
        }

        public bool IsExported(Type type)
        {
            return types.Contains(type);
        }

        // 是否在黑名单中屏蔽, 或者已知无需导出的类型
        public bool IsExportingBlocked(Type type)
        {
            if (blacklist.Contains(type))
            {
                return true;
            }
            if (type.IsGenericType)
            {
                return true;
            }
            return false;
        }

        // 是否显式要求导出
        public bool IsExportingExplicit(Type type)
        {
            if (whitelist.Contains(type))
            {
                return true;
            }
            return false;
        }

        // 将类型名转换成简单字符串 (比如用于文件名)
        public string GetFileName(Type type)
        {
            return type.FullName.Replace(".", "_");
        }

        public void Collect()
        {
            Collect(Prefs.GetPrefs().explicitAssemblies, false);
            Collect(Prefs.GetPrefs().implicitAssemblies, true);
        }

        // implicitExport: 默认进行导出(黑名单例外), 否则根据导出标记或手工添加
        public void Collect(List<string> assemblyNames, bool implicitExport)
        {
            foreach (var assemblyName in assemblyNames)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    var types = assembly.GetExportedTypes();

                    // Debug.LogFormat("collecting assembly {0}: {1}", assemblyName, types.Length);
                    foreach (var type in types)
                    {
                        if (IsExportingBlocked(type))
                        {
                            continue;
                        }
                        if (implicitExport || IsExportingExplicit(type))
                        {
                            this.AddExport(type);
                        }
                    }
                }
                catch (Exception)
                {
                    // Debug.LogWarning(exception);
                }
            }
        }

        // 清理多余文件
        public void Cleanup()
        {
            var outDir = Prefs.GetPrefs().outDir;
            foreach (var file in Directory.GetFiles(outDir))
            {
                var nfile = file;
                if (file.EndsWith(".meta"))
                {
                    nfile = file.Substring(0, file.Length - 5);
                }
                // Debug.LogFormat("checking file {0}", nfile);
                if (!outputFiles.Contains(nfile))
                {
                    File.Delete(file);
                    Debug.LogFormat("cleanup unused file {0}", file);
                }
            }
        }

        public void AddOutputFile(string filename)
        {
            outputFiles.Add(filename);
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