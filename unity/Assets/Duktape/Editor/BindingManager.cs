using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Duktape
{
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