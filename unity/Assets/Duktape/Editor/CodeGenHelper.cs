using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class TopLevelCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public TopLevelCodeGen(CodeGenerator cg, TypeBindingInfo type)
        {
            this.cg = cg;
            this.cg.csharp.AppendLine("// UserName: {0} @ {1}", Environment.UserName, this.cg.bindingManager.dateTime);
            this.cg.csharp.AppendLine("// Assembly: {0}", type.Assembly.GetName());
            this.cg.csharp.AppendLine("// Type: {0}", type.FullName);
            this.cg.csharp.AppendLine("using System;");
            this.cg.csharp.AppendLine("using System.Collections.Generic;");
            this.cg.csharp.AppendLine();

            this.cg.typescript.AppendLine("// {0} {1}", Environment.UserName, this.cg.bindingManager.dateTime);
        }

        public void Dispose()
        {
        }
    }

    public class NamespaceCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected string ns;
        protected TypeBindingInfo type;

        public NamespaceCodeGen(CodeGenerator cg, string ns, TypeBindingInfo type)
        {
            this.cg = cg;
            this.ns = ns;
            this.type = type;
            if (!string.IsNullOrEmpty(ns))
            {
                this.cg.csharp.AppendLine("namespace {0} {{", ns);
                this.cg.csharp.AddTabLevel();
            }
            this.cg.csharp.AppendLine("using Duktape;");
            // cg.csharp.AppendLine("using UnityEngine;");
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                this.cg.typescript.AppendLine("declare namespace {0} {{", type.Namespace);
                this.cg.typescript.AddTabLevel();
            }
        }

        public void Dispose()
        {
            if (!string.IsNullOrEmpty(ns))
            {
                this.cg.csharp.DecTabLevel();
                this.cg.csharp.AppendLine("}");
            }
            if (!string.IsNullOrEmpty(type.Namespace))
            {
                this.cg.typescript.DecTabLevel();
                this.cg.typescript.AppendLine("}");
            }
        }
    }

    public class RegFuncCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public RegFuncCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            this.cg.csharp.AppendLine("public static int reg(IntPtr ctx)");
            this.cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
        }

        public virtual void Dispose()
        {
            this.cg.csharp.AppendLine("return 0;");
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }
    }

    public class RegFuncNamespaceCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public RegFuncNamespaceCodeGen(CodeGenerator cg, TypeBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.cg.csharp.Append("duk_begin_namespace(ctx");
            // Debug.LogErrorFormat("{0}: {1}", bindingInfo.type, bindingInfo.Namespace);
            if (bindingInfo.Namespace != null)
            {
                var split_ns = bindingInfo.Namespace.Split('.');
                for (var i = 0; i < split_ns.Length; i++)
                {
                    var el_ns = split_ns[i];
                    this.cg.csharp.AppendL(", \"{0}\"", el_ns);
                }
            }
            this.cg.csharp.AppendLineL(");");
        }

        public virtual void Dispose()
        {
            this.cg.csharp.AppendLine("duk_end_namespace(ctx);");
        }
    }

    public class TypeCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        protected TypeBindingInfo bindingInfo;

        public TypeCodeGen(CodeGenerator cg, TypeBindingInfo bindingInfo)
        {
            this.cg = cg;
            this.bindingInfo = bindingInfo;
            this.cg.csharp.AppendLine("[{0}({1})]", typeof(JSBindingAttribute).Name, DuktapeVM.VERSION);
            this.cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
            this.cg.csharp.AppendLine("public class {0} : {1} {{", bindingInfo.name, typeof(DuktapeBinding).Name);
            this.cg.csharp.AddTabLevel();
        }

        public string GetTypeName(Type type)
        {
            return type.FullName.Replace(".", "_");
        }

        public virtual void Dispose()
        {
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }
    }

    public class PreservedCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public PreservedCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            this.cg.csharp.AppendLine("[UnityEngine.Scripting.Preserve]");
        }

        public virtual void Dispose()
        {
        }
    }

    public class PInvokeGuardCodeGen : PreservedCodeGen
    {
        public PInvokeGuardCodeGen(CodeGenerator cg)
        : base(cg)
        {
            this.cg.csharp.AppendLine("[AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]");
        }
    }

    public class BindingFuncDeclareCodeGen : IDisposable
    {
        protected CodeGenerator cg;

        public BindingFuncDeclareCodeGen(CodeGenerator cg, string name)
        {
            this.cg = cg;
            this.cg.csharp.AppendLine("public static int {0}(IntPtr ctx)", name);
            this.cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
        }

        public virtual void Dispose()
        {
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }
    }

    public class TryCatchGuradCodeGen : IDisposable
    {
        protected CodeGenerator cg;
        public TryCatchGuradCodeGen(CodeGenerator cg)
        {
            this.cg = cg;
            this.cg.csharp.AppendLine("try");
            this.cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
        }

        private string GetVarName(Type type)
        {
            var name = type.Name;
            return name[0].ToString().ToLower() + name.Substring(1);
        }

        private void AddCatchClause(Type exceptionType, string handler)
        {
            var varName = GetVarName(exceptionType);
            this.cg.csharp.AppendLine("catch ({0} {1})", exceptionType.Name, varName);
            this.cg.csharp.AppendLine("{");
            this.cg.csharp.AddTabLevel();
            {
                this.cg.csharp.AppendLine("return DuktapeDLL.{0}(ctx, {1}.ToString());", handler, varName);
            }
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
        }

        public virtual void Dispose()
        {
            this.cg.csharp.DecTabLevel();
            this.cg.csharp.AppendLine("}");
            // this.AddCatchClause(typeof(NullReferenceException), "duk_reference_error");
            // this.AddCatchClause(typeof(IndexOutOfRangeException), "duk_range_error");
            this.AddCatchClause(typeof(Exception), "duk_generic_error");
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
                    cg.csharp.AppendLineL("#if UNITY_ANDROID");
                    break;
                case BuildTarget.iOS:
                    cg.csharp.AppendLineL("#if UNITY_IOS");
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    cg.csharp.AppendLineL("#if UNITY_STANDALONE_WIN");
                    break;
                case BuildTarget.StandaloneOSX:
                    cg.csharp.AppendLineL("#if UNITY_STANDALONE_OSX");
                    break;
                default:
                    cg.csharp.AppendLineL("#if false");
                    break;
            }
        }


        public void Dispose()
        {
            cg.csharp.AppendLineL("#endif");
        }
    }
}