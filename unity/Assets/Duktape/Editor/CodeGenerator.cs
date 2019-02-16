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
        private string newline;
        private string tab;
        private StringBuilder sb = new StringBuilder();
        private int tabLevel;

        public CodeGenerator()
        {
            this.tab = Prefs.GetPrefs().tab;
            switch (Prefs.GetPrefs().newLineStyle)
            {
                case NewLineStyle.CR: newline = "\r"; break;
                case NewLineStyle.LF: newline = "\n"; break;
                case NewLineStyle.CRLF: newline = "\r\n"; break;
                default: newline = Environment.NewLine; break;
            }
        }

        public void Generate(Type type)
        {
            // Prefs.GetPrefs().outDir
            Clear();
            using (new PlatformCodeGen(this))
            {
                AppendLine("// {0} {1}", Environment.UserName, DateTime.Now);
                AppendLine("using System;");
                AppendLine("using System.Collections.Generic;");
                AppendLine();

                using (new NamespaceCodeGen(this, Prefs.GetPrefs().ns))
                {
                    AppendLine("using Duktape;");
                    // AppendLine("using UnityEngine;");

                    if (type.IsEnum)
                    {
                        using (new EnumCodeGen(this, type))
                        {
                            //
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
            Debug.Log(sb.ToString());
        }
    }
}