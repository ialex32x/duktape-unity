using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public partial class BindingManager
    {
        public DateTime dateTime;
        public TextGenerator log;
        private HashSet<Type> blacklist;
        private HashSet<Type> whitelist;
        private List<string> typePrefixBlacklist;
        private Dictionary<Type, TypeBindingInfo> exportedTypes = new Dictionary<Type, TypeBindingInfo>();
        private List<string> outputFiles = new List<string>();

        public BindingManager()
        {
            this.dateTime = DateTime.Now;
            var tab = Prefs.GetPrefs().tab;
            var newline = Prefs.GetPrefs().newline;
            typePrefixBlacklist = Prefs.GetPrefs().typePrefixBlacklist;
            log = new TextGenerator(newline, tab);
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
            var typeBindingInfo = new TypeBindingInfo(this, type);
            exportedTypes.Add(type, typeBindingInfo);
        }

        public bool IsExported(Type type)
        {
            return exportedTypes.ContainsKey(type);
        }

        // 获取在typescript中的完整名类型
        public string GetTypeFullNameTS(Type type)
        {
            if (type == null)
            {
                return "void";
            }
            var info = GetExportedType(type);
            if (info != null)
            {
                return info.FullName;
            }
            if (type == typeof(void))
            {
                return "void";
            }
            if (type.IsPrimitive && type.IsValueType)
            {
                if (type == typeof(sbyte) || type == typeof(byte)
                || type == typeof(int) || type == typeof(uint)
                || type == typeof(short) || type == typeof(ushort)
                || type == typeof(long) || type == typeof(ulong)
                || type == typeof(float) || type == typeof(double))
                {
                    return "number";
                }
            }
            if (type == typeof(string) || type == typeof(char))
            {
                return "string";
            }
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return GetTypeFullNameTS(elementType) + "[]";
            }
            return "any";
        }

        public TypeBindingInfo GetExportedType(Type type)
        {
            if (type == null)
            {
                return null;
            }
            TypeBindingInfo typeBindingInfo;
            return exportedTypes.TryGetValue(type, out typeBindingInfo) ? typeBindingInfo : null;
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
            if (type.Name.Contains("<"))
            {
                return true;
            }
            for (int i = 0, size = typePrefixBlacklist.Count; i < size; i++)
            {
                if (type.FullName.StartsWith(typePrefixBlacklist[i]))
                {
                    return true;
                }
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
            if (type.IsDefined(typeof(JSTypeAttribute), false))
            {
                return true;
            }
            return false;
        }

        public void Collect()
        {
            Collect(Prefs.GetPrefs().explicitAssemblies, false);
            Collect(Prefs.GetPrefs().implicitAssemblies, true);
            foreach (var typeBindingInfoKV in exportedTypes)
            {
                typeBindingInfoKV.Value.Collect();
            }
        }

        // implicitExport: 默认进行导出(黑名单例外), 否则根据导出标记或手工添加
        private void Collect(List<string> assemblyNames, bool implicitExport)
        {
            foreach (var assemblyName in assemblyNames)
            {
                log.AppendLine("assembly: {0}", assemblyName);
                log.AddTabLevel();
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    var types = assembly.GetExportedTypes();

                    log.AppendLine("types {0}", types.Length);
                    foreach (var type in types)
                    {
                        if (IsExportingBlocked(type))
                        {
                            log.AppendLine("blocked: {0}", type.FullName);
                            continue;
                        }
                        if (implicitExport || IsExportingExplicit(type))
                        {
                            log.AppendLine("export: {0}", type.FullName);
                            this.AddExport(type);
                        }
                    }
                }
                catch (Exception exception)
                {
                    log.AppendLine(exception.ToString());
                }
                log.DecTabLevel();
            }
        }

        // 清理多余文件
        public void Cleanup()
        {
            log.AppendLine("cleanup");
            log.AddTabLevel();
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
                    log.AppendLine("remove unused file {0}", file);
                }
            }
            log.DecTabLevel();
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
            foreach (var typeKV in exportedTypes)
            {
                var typeBindingInfo = typeKV.Value;
                try
                {
                    cg.Generate(typeBindingInfo);
                    cg.WriteTo(outDir, typeBindingInfo.GetFileName(), tx);
                }
                catch (Exception exception)
                {
                    Error(string.Format("generate failed {0}: {1}", typeBindingInfo.Name, exception.Message));
                    Debug.LogError(exception.StackTrace);
                }
            }

            var logPath = Prefs.GetPrefs().logPath;
            File.WriteAllText(logPath, log.ToString());
            Debug.LogFormat("generated {0} types", exportedTypes.Count);
        }
    }
}