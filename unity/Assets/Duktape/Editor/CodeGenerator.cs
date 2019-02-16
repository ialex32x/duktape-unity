using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class PlatformCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public PlatformCodeGen(CodeGenerator cg)
        {
            this.cg = cg;

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    cg.AppendLine("#if UNITY_ANDROID");
                    break;
                case BuildTarget.iOS:
                    cg.AppendLine("#if UNITY_IOS");
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    cg.AppendLine("#if UNITY_STANDALONE_WIN");
                    break;
                case BuildTarget.StandaloneOSX:
                    cg.AppendLine("#if UNITY_STANDALONE_OSX");
                    break;
                default:
                    cg.AppendLine("#if false");
                    break;
            }
        }


        public void Dispose()
        {
            cg.AppendLine("#endif");
        }
    }

    public class CodeGenerator
    {
        private string newline = "\r";
        private StringBuilder sb = new StringBuilder();

        public CodeGenerator()
        {
            switch (Prefs.GetPrefs().newLineStyle)
            {
                case NewLineStyle.CR: newline = "\r"; break;
                case NewLineStyle.LF: newline = "\n"; break;
                case NewLineStyle.CRLF: newline = "\r\n"; break;
                default: newline = Environment.NewLine; break;
            }
        }

        public void AppendLine()
        {
            sb.Append(newline);
        }

        public void AppendLine(string text)
        {
            sb.Append(text);
            sb.Append(newline);
        }

        public void AppendLine(string text, object arg1)
        {
            sb.AppendFormat(text, arg1);
            sb.Append(newline);
        }

        public void AppendLine(string text, object arg1, object arg2)
        {
            sb.AppendFormat(text, arg1, arg2);
            sb.Append(newline);
        }

        public void AppendLine(string text, object arg1, object arg2, object arg3)
        {
            sb.AppendFormat(text, arg1, arg2, arg3);
            sb.Append(newline);
        }

        public void AppendLine(string text, params object[] args)
        {
            sb.AppendFormat(text, args);
            sb.Append(newline);
        }

        public void Append(string text)
        {
            sb.Append(text);
        }

        public void Append(string text, object arg1)
        {
            sb.AppendFormat(text, arg1);
        }

        public void Append(string text, object arg1, object arg2)
        {
            sb.AppendFormat(text, arg1, arg2);
        }

        public void Append(string text, object arg1, object arg2, object arg3)
        {
            sb.AppendFormat(text, arg1, arg2, arg3);
        }

        public void Append(string text, params object[] args)
        {
            sb.AppendFormat(text, args);
        }


        public void Clear()
        {
            sb.Clear();
        }

        public void Generate(Type type)
        {
            // Prefs.GetPrefs().outDir
            Clear();
            using (new PlatformCodeGen(this))
            {
                AppendLine("// test {0}", type.FullName);
            }
            Debug.Log(sb.ToString());
        }
    }
}