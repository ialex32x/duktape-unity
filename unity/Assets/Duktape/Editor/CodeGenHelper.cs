using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;
    
    public class NamespaceCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public NamespaceCodeGen(CodeGenerator cg, string ns)
        {
            this.cg = cg;
            cg.AppendLine("namespace {0} {{", ns);
            this.cg.AddTabLevel();
        }

        public void Dispose()
        {
            this.cg.DecTabLevel();
            cg.AppendLine("}");
        }
    }


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
                    cg.AppendLineL("#if UNITY_ANDROID");
                    break;
                case BuildTarget.iOS:
                    cg.AppendLineL("#if UNITY_IOS");
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    cg.AppendLineL("#if UNITY_STANDALONE_WIN");
                    break;
                case BuildTarget.StandaloneOSX:
                    cg.AppendLineL("#if UNITY_STANDALONE_OSX");
                    break;
                default:
                    cg.AppendLineL("#if false");
                    break;
            }
        }


        public void Dispose()
        {
            cg.AppendLineL("#endif");
        }
    }
}