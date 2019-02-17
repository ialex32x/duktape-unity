using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public partial class CodeGenerator
    {
        public TextGenerator csharp;
        public TextGenerator typescript;

        public CodeGenerator()
        {
            var tab = Prefs.GetPrefs().tab;
            string newline;
            switch (Prefs.GetPrefs().newLineStyle)
            {
                case NewLineStyle.CR: newline = "\r"; break;
                case NewLineStyle.LF: newline = "\n"; break;
                case NewLineStyle.CRLF: newline = "\r\n"; break;
                default: newline = Environment.NewLine; break;
            }
            csharp = new TextGenerator(newline, tab);
            typescript = new TextGenerator(newline, tab);
        }

        public void Generate(Type type)
        {
            // Prefs.GetPrefs().outDir
            csharp.Clear();
            typescript.Clear();
            using (new PlatformCodeGen(this))
            {
                using (new TopLevelCodeGen(this))
                {
                    using (new NamespaceCodeGen(this, Prefs.GetPrefs().ns, type))
                    {
                        if (type.IsEnum)
                        {
                            using (new EnumCodeGen(this, type))
                            {
                            }
                        }
                        else
                        {
                            using (var ccg = new ClassCodeGen(this, type))
                            {
                            }
                        }
                    }
                }
            }
            Debug.Log(csharp.ToString());
            Debug.Log(typescript.ToString());
        }
    }
}