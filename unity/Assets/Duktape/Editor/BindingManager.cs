﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

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
        private Dictionary<Type, DelegateBindingInfo> exportedDelegates = new Dictionary<Type, DelegateBindingInfo>();
        private Dictionary<Type, Type> redirectDelegates = new Dictionary<Type, Type>();
        private List<string> outputFiles = new List<string>();

        private Dictionary<Type, string> _tsTypeNameMap = new Dictionary<Type, string>();
        private Dictionary<Type, string> _csTypeNameMap = new Dictionary<Type, string>();
        private Dictionary<string, string> _csTypeNameMapS = new Dictionary<string, string>();

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

            _tsTypeNameMap[typeof(sbyte)] = "number";
            _tsTypeNameMap[typeof(byte)] = "number";
            _tsTypeNameMap[typeof(int)] = "number";
            _tsTypeNameMap[typeof(uint)] = "number";
            _tsTypeNameMap[typeof(short)] = "number";
            _tsTypeNameMap[typeof(ushort)] = "number";
            _tsTypeNameMap[typeof(long)] = "number";
            _tsTypeNameMap[typeof(ulong)] = "number";
            _tsTypeNameMap[typeof(float)] = "number";
            _tsTypeNameMap[typeof(double)] = "number";
            _tsTypeNameMap[typeof(bool)] = "boolean";
            _tsTypeNameMap[typeof(string)] = "string";
            _tsTypeNameMap[typeof(char)] = "string";
            _tsTypeNameMap[typeof(void)] = "void";

            AddTypeNameMapCS(typeof(sbyte), "sbyte");
            AddTypeNameMapCS(typeof(byte), "byte");
            AddTypeNameMapCS(typeof(int), "int");
            AddTypeNameMapCS(typeof(uint), "uint");
            AddTypeNameMapCS(typeof(short), "short");
            AddTypeNameMapCS(typeof(ushort), "ushort");
            AddTypeNameMapCS(typeof(long), "long");
            AddTypeNameMapCS(typeof(ulong), "ulong");
            AddTypeNameMapCS(typeof(float), "float");
            AddTypeNameMapCS(typeof(double), "double");
            AddTypeNameMapCS(typeof(bool), "bool");
            AddTypeNameMapCS(typeof(string), "string");
            AddTypeNameMapCS(typeof(char), "char");
            AddTypeNameMapCS(typeof(System.Object), "object");
            AddTypeNameMapCS(typeof(void), "void");
        }

        private void AddTypeNameMapCS(Type type, string name)
        {
            _csTypeNameMap[type] = name;
            _csTypeNameMapS[type.FullName] = name;
            _csTypeNameMapS[WithNamespaceCS(type) + type.Name] = name;
        }

        public void AddExport(Type type)
        {
            var typeBindingInfo = new TypeBindingInfo(this, type);
            exportedTypes.Add(type, typeBindingInfo);
        }

        public DelegateBindingInfo GetDelegateBindingInfo(Type type)
        {
            Type target;
            if (redirectDelegates.TryGetValue(type, out target))
            {
                type = target;
            }
            DelegateBindingInfo delegateBindingInfo;
            if (exportedDelegates.TryGetValue(type, out delegateBindingInfo))
            {
                return delegateBindingInfo;
            }
            return null;
        }

        public void CollectDelegate(Type type)
        {
            if (type == null || type.BaseType != typeof(MulticastDelegate))
            {
                return;
            }
            if (!exportedDelegates.ContainsKey(type))
            {
                var invoke = type.GetMethod("Invoke");
                var returnType = invoke.ReturnType;
                var parameters = invoke.GetParameters();
                // 是否存在等价 delegate
                foreach (var kv in exportedDelegates)
                {
                    if (kv.Value.Equals(returnType, parameters))
                    {
                        log.AppendLine("skip delegate: {0} && {1}", kv.Value, type);
                        kv.Value.types.Add(type);
                        redirectDelegates[type] = kv.Key;
                        return;
                    }
                }
                var delegateBindingInfo = new DelegateBindingInfo(returnType, parameters);
                delegateBindingInfo.types.Add(type);
                exportedDelegates.Add(type, delegateBindingInfo);
                log.AppendLine("add delegate: {0}", type);
                for (var i = 0; i < parameters.Length; i++)
                {
                    CollectDelegate(parameters[i].ParameterType);
                }
            }
        }

        public bool IsExported(Type type)
        {
            return exportedTypes.ContainsKey(type);
        }

        // 获取 type 在 typescript 中对应类型名
        public string GetTypeFullNameTS(Type type)
        {
            if (type == null || type == typeof(void))
            {
                return "void";
            }
            string name;
            if (_tsTypeNameMap.TryGetValue(type, out name))
            {
                return name;
            }
            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                return GetTypeFullNameTS(elementType) + "[]";
            }
            var info = GetExportedType(type);
            if (info != null)
            {
                return info.FullName.Replace('+', '.');
            }
            if (type.BaseType == typeof(MulticastDelegate))
            {
                var delegateBindingInfo = GetDelegateBindingInfo(type);
                if (delegateBindingInfo != null)
                {
                    var nargs = delegateBindingInfo.parameters.Length;
                    var ret = GetTypeFullNameTS(delegateBindingInfo.returnType);
                    var arglist = (nargs > 0 ? ", " : "") + GetArglistTypesTS(delegateBindingInfo.parameters);
                    return $"Delegate{nargs}<{ret}{arglist}>";
                }
            }
            return "any";
        }

        public string WithNamespaceCS(Type type)
        {
            return WithNamespaceCS(type.Namespace);
        }

        public string WithNamespaceCS(string ns)
        {
            return string.IsNullOrEmpty(ns) ? "" : (ns + ".");
        }

        // 生成参数对应的字符串形式参数列表定义 (typescript)
        public string GetArglistTypesTS(ParameterInfo[] parameters)
        {
            var size = parameters.Length;
            var arglist = "";
            if (size == 0)
            {
                return arglist;
            }
            for (var i = 0; i < size; i++)
            {
                var parameter = parameters[i];
                var typename = GetTypeFullNameTS(parameter.ParameterType);
                // if (parameter.IsOut && parameter.ParameterType.IsByRef)
                // {
                //     arglist += "out ";
                // }
                // else if (parameter.ParameterType.IsByRef)
                // {
                //     arglist += "ref ";
                // }
                arglist += typename;
                // arglist += " ";
                // arglist += parameter.Name;
                if (i != size - 1)
                {
                    arglist += ", ";
                }
            }
            return arglist;
        }

        public string GetDuktapeThisGetter(Type type)
        {
            return "duk_get_this";
        }

        public string GetDuktapeGetter(Type type)
        {
            if (type.IsByRef)
            {
                return GetDuktapeGetter(type.GetElementType());
            }
            if (type.IsArray)
            {
                //TODO: 处理数组取参操作函数指定
                var elementType = type.GetElementType();
                return GetDuktapeGetter(elementType) + "_array"; //TODO: 嵌套数组的问题
            }
            if (type.IsValueType)
            {
                if (type.IsPrimitive)
                {
                    return "duk_get_primitive";
                }
                if (type.IsEnum)
                {
                    return "duk_get_enumvalue";
                }
                return "duk_get_structvalue";
            }
            if (type == typeof(string))
            {
                return "duk_get_primitive";
            }
            if (type.BaseType == typeof(MulticastDelegate))
            {
                return "duk_get_delegate";
            }
            return "duk_get_classvalue";
        }

        public string GetDuktapePusher(Type type)
        {
            if (type.BaseType == typeof(MulticastDelegate))
            {
                return "duk_push_delegate";
            }
            return "duk_push_any";
        }

        // 保证生成一个以 prefix 为前缀, 与参数列表中所有参数名不同的名字
        public string GetUniqueName(ParameterInfo[] parameters, string prefix)
        {
            return GetUniqueName(parameters, prefix, 0);
        }

        public string GetUniqueName(ParameterInfo[] parameters, string prefix, int index)
        {
            var size = parameters.Length;
            var name = prefix + index;
            for (var i = 0; i < size; i++)
            {
                var parameter = parameters[i];
                if (parameter.Name == prefix)
                {
                    return GetUniqueName(parameters, prefix, index + 1);
                }
            }
            return name;
        }

        // 生成参数对应的字符串形式参数列表 (csharp)
        public string GetArglistDeclCS(ParameterInfo[] parameters)
        {
            var size = parameters.Length;
            var arglist = "";
            if (size == 0)
            {
                return arglist;
            }
            for (var i = 0; i < size; i++)
            {
                var parameter = parameters[i];
                var typename = GetTypeFullNameCS(parameter.ParameterType);
                if (parameter.IsOut && parameter.ParameterType.IsByRef)
                {
                    arglist += "out ";
                }
                else if (parameter.ParameterType.IsByRef)
                {
                    arglist += "ref ";
                }
                arglist += typename;
                arglist += " ";
                arglist += parameter.Name;
                if (i != size - 1)
                {
                    arglist += ", ";
                }
            }
            return arglist;
        }

        // 获取 type 在 绑定代码 中对应类型名
        public string GetTypeFullNameCS(Type type)
        {
            // Debug.LogFormat("{0} Array {1} ByRef {2} GetElementType {3}", type, type.IsArray, type.IsByRef, type.GetElementType());
            if (type.IsGenericType)
            {
                var purename = WithNamespaceCS(type) + type.Name.Substring(0, type.Name.Length - 2);
                var gargs = type.GetGenericArguments();
                purename += "<";
                for (var i = 0; i < gargs.Length; i++)
                {
                    var garg = gargs[i];
                    purename += GetTypeFullNameCS(garg);
                    if (i != gargs.Length - 1)
                    {
                        purename += ", ";
                    }
                }
                purename += ">";
                return purename;
            }
            if (type.IsArray)
            {
                return GetTypeFullNameCS(type.GetElementType()) + "[]";
            }
            if (type.IsByRef)
            {
                return GetTypeFullNameCS(type.GetElementType());
            }
            string name;
            if (_csTypeNameMap.TryGetValue(type, out name))
            {
                return name;
            }
            var fullname = type.FullName.Replace('+', '.');
            if (fullname.Contains("`"))
            {
                fullname = new Regex(@"`\d", RegexOptions.None).Replace(fullname, "");
                //TODO: maybe conflict?
                fullname = fullname.Replace("[", "<");
                fullname = fullname.Replace("]", ">");
            }
            if (_csTypeNameMapS.TryGetValue(fullname, out name))
            {
                return name;
            }
            return fullname;
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
            if (type.IsDefined(typeof(JSBindingAttribute), false))
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
            // 收集直接类型, 加入 exportedTypes
            Collect(Prefs.GetPrefs().explicitAssemblies, false);
            Collect(Prefs.GetPrefs().implicitAssemblies, true);

            log.AppendLine("collecting members");
            log.AddTabLevel();
            foreach (var typeBindingInfoKV in exportedTypes)
            {
                var typeBindingInfo = typeBindingInfoKV.Value;
                log.AppendLine("type: {0}", typeBindingInfo.type);
                log.AddTabLevel();
                typeBindingInfo.Collect();
                log.DecTabLevel();
            }
            log.DecTabLevel();
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
                            continue;
                        }
                        log.AppendLine("skip: {0}", type.FullName);
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
            Cleanup(Prefs.GetPrefs().outDir, outputFiles, file =>
            {
                log.AppendLine("remove unused file {0}", file);
            });
            log.DecTabLevel();
        }

        public static void Cleanup(string outDir, List<string> excludedFiles, Action<string> ondelete)
        {
            foreach (var file in Directory.GetFiles(outDir))
            {
                var nfile = file;
                if (file.EndsWith(".meta"))
                {
                    nfile = file.Substring(0, file.Length - 5);
                }
                // Debug.LogFormat("checking file {0}", nfile);
                if (excludedFiles == null || !excludedFiles.Contains(nfile))
                {
                    File.Delete(file);
                    if (ondelete != null)
                    {
                        ondelete(file);
                    }
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
            var tx = Prefs.GetPrefs().extraExt;
            // var tx = "";
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }
            foreach (var typeKV in exportedTypes)
            {
                var typeBindingInfo = typeKV.Value;
                try
                {
                    cg.Clear();
                    cg.Generate(typeBindingInfo);
                    cg.WriteTo(outDir, typeBindingInfo.GetFileName(), tx);
                }
                catch (Exception exception)
                {
                    Error(string.Format("generate failed {0}: {1}", typeBindingInfo.type.Name, exception.Message));
                    Debug.LogError(exception.StackTrace);
                }
            }

            try
            {
                var exportedDelegatesArray = new DelegateBindingInfo[this.exportedDelegates.Count];
                this.exportedDelegates.Values.CopyTo(exportedDelegatesArray, 0);

                cg.Clear();
                cg.Generate(exportedDelegatesArray);
                cg.WriteTo(outDir, DuktapeVM._DuktapeDelegates, tx);
            }
            catch (Exception exception)
            {
                Error(string.Format("generate failed: {0}", exception.Message));
                Debug.LogError(exception.StackTrace);
            }

            var logPath = Prefs.GetPrefs().logPath;
            File.WriteAllText(logPath, log.ToString());
            var now = DateTime.Now;
            var ts = now.Subtract(dateTime);
            Debug.LogFormat("generated {0} type(s), {1} delegate(s) in {2:0.##} seconds.", exportedTypes.Count, exportedDelegates.Count, ts.TotalSeconds);
        }
    }
}