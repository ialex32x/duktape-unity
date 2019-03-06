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
        public Prefs prefs;
        private HashSet<Type> blacklist;
        private HashSet<Type> whitelist;
        private List<string> typePrefixBlacklist;
        private Dictionary<Type, TypeBindingInfo> exportedTypes = new Dictionary<Type, TypeBindingInfo>();
        private Dictionary<Type, DelegateBindingInfo> exportedDelegates = new Dictionary<Type, DelegateBindingInfo>();
        private Dictionary<Type, Type> redirectDelegates = new Dictionary<Type, Type>();
        private List<string> outputFiles = new List<string>();
        private List<string> removedFiles = new List<string>();

        private Dictionary<Type, string> _tsTypeNameMap = new Dictionary<Type, string>();
        private Dictionary<Type, string> _csTypeNameMap = new Dictionary<Type, string>();
        private Dictionary<string, string> _csTypeNameMapS = new Dictionary<string, string>();
        private static HashSet<string> _tsKeywords = new HashSet<string>();

        // 自定义的处理流程
        private List<IBindingProcess> _bindingProcess = new List<IBindingProcess>();

        // 针对特定方法的 ts 声明优化
        private Dictionary<MethodBase, string> _tsMethodDeclarations = new Dictionary<MethodBase, string>();

        static BindingManager()
        {
            AddTSKeywords(
                "function",
                "interface",
                "class",
                "let",
                "break",
                "as",
                "any",
                "switch",
                "case",
                "if",
                "throw",
                "else",
                "var",
                "number",
                "string",
                "get",
                "module",
                // "type",
                "instanceof",
                "typeof",
                "public",
                "private",
                "enum",
                "export",
                "finally",
                "for",
                "while",
                "void",
                "null",
                "super",
                "this",
                "new",
                "in",
                "extends",
                "static",
                "package",
                "implements",
                "interface",
                "continue",
                "yield",
                "const"
            );
        }

        public BindingManager(Prefs prefs)
        {
            this.prefs = prefs;
            this.dateTime = DateTime.Now;
            var tab = prefs.tab;
            var newline = prefs.newline;
            typePrefixBlacklist = prefs.typePrefixBlacklist;
            log = new TextGenerator(newline, tab);
            blacklist = new HashSet<Type>(new Type[]
            {
                typeof(AOT.MonoPInvokeCallbackAttribute),
            });
            whitelist = new HashSet<Type>(new Type[]
            {
            });

            AddTSMethodDeclaration("AddComponent<T extends UnityEngine.Component>(type: { new(): T }): T",
                typeof(GameObject), "AddComponent", typeof(Type));

            AddTSMethodDeclaration("GetComponent<T extends UnityEngine.Component>(type: { new(): T }): T",
                typeof(GameObject), "GetComponent", typeof(Type));

            AddTSMethodDeclaration("GetComponentInChildren<T extends UnityEngine.Component>(type: { new(): T }, includeInactive: boolean): T",
                typeof(GameObject), "GetComponentInChildren", typeof(Type), typeof(bool));

            AddTSMethodDeclaration("GetComponentInChildren<T extends UnityEngine.Component>(type: { new(): T }): T",
                typeof(GameObject), "GetComponentInChildren", typeof(Type));

            AddTSMethodDeclaration("GetComponentInParent<T extends UnityEngine.Component>(type: { new(): T }): T",
                typeof(GameObject), "GetComponentInParent", typeof(Type));

            // AddTSMethodDeclaration("GetComponents<T extends UnityEngine.Component>(type: { new(): T }, results: any): void", 
            //     typeof(GameObject), "GetComponents", typeof(Type));

            AddTSMethodDeclaration("GetComponents<T extends UnityEngine.Component>(type: { new(): T }): T[]",
                typeof(GameObject), "GetComponents", typeof(Type));

            AddTSMethodDeclaration("GetComponentsInChildren<T extends UnityEngine.Component>(type: { new(): T }, includeInactive: boolean): T[]",
                typeof(GameObject), "GetComponentsInChildren", typeof(Type), typeof(bool));

            AddTSMethodDeclaration("GetComponentsInChildren<T extends UnityEngine.Component>(type: { new(): T }): T[]",
                typeof(GameObject), "GetComponentsInChildren", typeof(Type));

            AddTSMethodDeclaration("GetComponentsInParent<T extends UnityEngine.Component>(type: { new(): T }, includeInactive: boolean): T[]",
                typeof(GameObject), "GetComponentsInParent", typeof(Type), typeof(bool));

            AddTSMethodDeclaration("GetComponentsInParent<T extends UnityEngine.Component>(type: { new(): T }): T[]",
                typeof(GameObject), "GetComponentsInParent", typeof(Type));

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
            _tsTypeNameMap[typeof(LayerMask)] = "(UnityEngine.LayerMask | number)";
            _tsTypeNameMap[typeof(Color)] = "(UnityEngine.Color | Array<number>)";
            _tsTypeNameMap[typeof(Color32)] = "(UnityEngine.Color32 | Array<number>)";
            _tsTypeNameMap[typeof(Vector2)] = "(UnityEngine.Vector2 | Array<number>)";
            _tsTypeNameMap[typeof(Vector2Int)] = "(UnityEngine.Vector2Int | Array<number>)";
            _tsTypeNameMap[typeof(Vector3)] = "(UnityEngine.Vector3 | Array<number>)";
            _tsTypeNameMap[typeof(Vector3Int)] = "(UnityEngine.Vector3Int | Array<number>)";
            _tsTypeNameMap[typeof(Vector4)] = "(UnityEngine.Vector4 | Array<number>)";
            _tsTypeNameMap[typeof(Quaternion)] = "(UnityEngine.Quaternion | Array<number>)";

            AddCSTypeNameMap(typeof(sbyte), "sbyte");
            AddCSTypeNameMap(typeof(byte), "byte");
            AddCSTypeNameMap(typeof(int), "int");
            AddCSTypeNameMap(typeof(uint), "uint");
            AddCSTypeNameMap(typeof(short), "short");
            AddCSTypeNameMap(typeof(ushort), "ushort");
            AddCSTypeNameMap(typeof(long), "long");
            AddCSTypeNameMap(typeof(ulong), "ulong");
            AddCSTypeNameMap(typeof(float), "float");
            AddCSTypeNameMap(typeof(double), "double");
            AddCSTypeNameMap(typeof(bool), "bool");
            AddCSTypeNameMap(typeof(string), "string");
            AddCSTypeNameMap(typeof(char), "char");
            AddCSTypeNameMap(typeof(System.Object), "object");
            AddCSTypeNameMap(typeof(void), "void");

            Initialize();
        }

        private static bool _FindFilterBindingProcess(Type type, object l)
        {
            return type == typeof(IBindingProcess);
        }

        private void Initialize()
        {
            var assembly = Assembly.Load("Assembly-CSharp-Editor");
            var types = assembly.GetExportedTypes();
            for (int i = 0, size = types.Length; i < size; i++)
            {
                var type = types[i];
                if (type.IsAbstract)
                {
                    continue;
                }
                try
                {
                    var interfaces = type.FindInterfaces(_FindFilterBindingProcess, null);
                    if (interfaces != null && interfaces.Length > 0)
                    {
                        var ctor = type.GetConstructor(Type.EmptyTypes);
                        var inst = ctor.Invoke(null) as IBindingProcess;
                        inst.OnInitialize(this);
                        _bindingProcess.Add(inst);
                        Debug.Log($"add binding process: {type}");
                        // _bindingProcess.Add
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"failed to add binding process: {type}\n{exception}");
                }
            }
        }

        // TS: 为指定类型的匹配方法添加声明映射 (仅用于优化代码提示体验)
        public void AddTSMethodDeclaration(string spec, Type type, string name, params Type[] parameters)
        {
            var method = type.GetMethod(name, parameters);
            if (method != null)
            {
                _tsMethodDeclarations[method] = spec;
            }
        }

        // TS: 添加保留字, CS中相关变量名等会自动重命名注册到js中
        public static void AddTSKeywords(params string[] keywords)
        {
            foreach (var keyword in keywords)
            {
                _tsKeywords.Add(keyword);
            }
        }

        public bool GetTSMethodDeclaration(MethodBase method, out string code)
        {
            return _tsMethodDeclarations.TryGetValue(method, out code);
        }

        // CS, 添加类型名称映射, 用于简化导出时的常用类型名
        public void AddCSTypeNameMap(Type type, string name)
        {
            _csTypeNameMap[type] = name;
            _csTypeNameMapS[type.FullName] = name;
            _csTypeNameMapS[GetCSNamespace(type) + type.Name] = name;
        }

        // 增加导出类型 (需要在 Collect 阶段进行)
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

        public static bool ContainsPointer(MethodBase method)
        {
            var parameters = method.GetParameters();
            for (int i = 0, size = parameters.Length; i < size; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsPointer)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsGenericMethod(MethodBase method)
        {
            return method.GetGenericArguments().Length > 0;
        }

        // 收集所有 delegate 类型
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
                if (ContainsPointer(invoke))
                {
                    log.AppendLine("skip unsafe (pointer) delegate: [{0}] {1}", type, invoke);
                    return;
                }
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
        public string GetTSTypeFullName(Type type)
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
                return GetTSTypeFullName(elementType) + "[]";
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
                    var ret = GetTSTypeFullName(delegateBindingInfo.returnType);
                    var arglist = (nargs > 0 ? ", " : "") + GetArglistTypesTS(delegateBindingInfo.parameters);
                    return $"Delegate{nargs}<{ret}{arglist}>";
                }
            }
            return "any";
        }

        public string GetCSNamespace(Type type)
        {
            return GetCSNamespace(type.Namespace);
        }

        public string GetCSNamespace(string ns)
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
                var typename = GetTSTypeFullName(parameter.ParameterType);
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

        public string GetDuktapeGenericError(string err)
        {
            return $"DuktapeDLL.duk_generic_error(ctx, \"{err}\");";
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
            if (type == typeof(Type))
            {
                return "duk_get_type";
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

        public static string GetTSVariable(string name)
        {
            if (_tsKeywords.Contains(name))
            {
                return name + "_";
            }
            return name;
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

        public string GetTSSuperName(TypeBindingInfo typeBindingInfo)
        {
            var super = typeBindingInfo.super;
            while (super != null)
            {
                var superBindingInfo = GetExportedType(super);
                if (superBindingInfo != null)
                {
                    return GetTSTypeFullName(superBindingInfo.type);
                }
                super = super.BaseType;
            }
            return "";
        }

        // 生成参数对应的字符串形式参数列表 (csharp)
        public string GetCSArglistDecl(ParameterInfo[] parameters)
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
                var typename = GetCSTypeFullName(parameter.ParameterType);
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
        public string GetCSTypeFullName(Type type)
        {
            // Debug.LogFormat("{0} Array {1} ByRef {2} GetElementType {3}", type, type.IsArray, type.IsByRef, type.GetElementType());
            if (type.IsGenericType)
            {
                var purename = GetCSNamespace(type) + type.Name.Substring(0, type.Name.Length - 2);
                var gargs = type.GetGenericArguments();
                purename += "<";
                for (var i = 0; i < gargs.Length; i++)
                {
                    var garg = gargs[i];
                    purename += GetCSTypeFullName(garg);
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
                return GetCSTypeFullName(type.GetElementType()) + "[]";
            }
            if (type.IsByRef)
            {
                return GetCSTypeFullName(type.GetElementType());
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
            if (type.BaseType == typeof(Attribute))
            {
                return true;
            }
            if (type.BaseType == typeof(MulticastDelegate))
            {
                return true;
            }
            if (type.IsPointer)
            {
                return true;
            }
            var encloser = type;
            while (encloser != null)
            {
                if (encloser.IsDefined(typeof(ObsoleteAttribute), false))
                {
                    return true;
                }
                encloser = encloser.DeclaringType;
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

        private void OnPreCollect()
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPreCollect(this);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPreCollect]: {exception}");
                }
            }
        }

        private void OnPostCollect()
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPostCollect(this);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPostCollect]: {exception}");
                }
            }
        }

        private void OnPreGenerateType(TypeBindingInfo bindingInfo)
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPreGenerateType(this, bindingInfo);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPreGenerateType]: {exception}");
                }
            }
        }

        private void OnPostGenerateType(TypeBindingInfo bindingInfo)
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPostGenerateType(this, bindingInfo);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPostGenerateType]: {exception}");
                }
            }
        }

        public void OnPreGenerateDelegate(DelegateBindingInfo bindingInfo)
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPreGenerateDelegate(this, bindingInfo);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPreGenerateDelegate]: {exception}");
                }
            }
        }

        public void OnPostGenerateDelegate(DelegateBindingInfo bindingInfo)
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnPostGenerateDelegate(this, bindingInfo);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnPostGenerateDelegate]: {exception}");
                }
            }
        }

        private void OnCleanup()
        {
            for (int i = 0, size = _bindingProcess.Count; i < size; i++)
            {
                var bp = _bindingProcess[i];
                try
                {
                    bp.OnCleanup(this);
                }
                catch (Exception exception)
                {
                    this.Error($"process failed [{bp}][OnCleanup]: {exception}");
                }
            }
        }



        public void Collect()
        {
            // 收集直接类型, 加入 exportedTypes
            Collect(prefs.explicitAssemblies, false);
            Collect(prefs.implicitAssemblies, true);

            log.AppendLine("collecting members");
            log.AddTabLevel();
            OnPreCollect();
            foreach (var typeBindingInfoKV in exportedTypes)
            {
                var typeBindingInfo = typeBindingInfoKV.Value;
                log.AppendLine("type: {0}", typeBindingInfo.type);
                log.AddTabLevel();
                typeBindingInfo.Collect();
                log.DecTabLevel();
            }
            OnPostCollect();
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
            Cleanup(prefs.outDir, outputFiles, file =>
            {
                removedFiles.Add(file);
                log.AppendLine("remove unused file {0}", file);
            });
            OnCleanup();
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
            var outDir = prefs.outDir;
            var tx = prefs.extraExt;
            // var tx = "";
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }
            var cancel = false;
            var current = 0;
            var total = exportedTypes.Count;
            foreach (var typeKV in exportedTypes)
            {
                var typeBindingInfo = typeKV.Value;
                try
                {
                    current++;
                    cancel = EditorUtility.DisplayCancelableProgressBar(
                        "Generating",
                        $"{current}/{total}: {typeBindingInfo.FullName}",
                        (float)current / total);
                    if (cancel)
                    {
                        Warn("operation canceled");
                        break;
                    }
                    cg.Clear();
                    OnPreGenerateType(typeBindingInfo);
                    cg.Generate(typeBindingInfo);
                    OnPostGenerateType(typeBindingInfo);
                    cg.WriteTo(outDir, typeBindingInfo.GetFileName(), tx);
                }
                catch (Exception exception)
                {
                    Error($"generate failed {typeBindingInfo.type.FullName}: {exception.Message}");
                    Debug.LogError(exception.StackTrace);
                }
            }

            if (!cancel)
            {
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
                    Error($"generate delegates failed: {exception.Message}");
                    Debug.LogError(exception.StackTrace);
                }
            }

            var logPath = prefs.logPath;
            File.WriteAllText(logPath, log.ToString());
            EditorUtility.ClearProgressBar();
        }

        public void Report()
        {
            var now = DateTime.Now;
            var ts = now.Subtract(dateTime);
            Debug.LogFormat("generated {0} type(s), {1} delegate(s), {2} deletion(s) in {3:0.##} seconds.",
                exportedTypes.Count,
                exportedDelegates.Count,
                removedFiles.Count,
                ts.TotalSeconds);
        }
    }
}