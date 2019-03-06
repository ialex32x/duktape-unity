using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public partial class CodeGenerator
    {
        public BindingManager bindingManager;
        public TextGenerator csharp;
        public TextGenerator typescript;

        public CodeGenerator(BindingManager bindingManager)
        {
            this.bindingManager = bindingManager;
            var tab = this.bindingManager.prefs.tab;
            var newline = this.bindingManager.prefs.newline;
            csharp = new TextGenerator(newline, tab);
            typescript = new TextGenerator(newline, tab);
        }

        public void Clear()
        {
            csharp.Clear();
            typescript.Clear();
        }

        // 生成委托绑定
        public void Generate(DelegateBindingInfo[] delegateBindingInfos)
        {
            using (new PlatformCodeGen(this))
            {
                using (new TopLevelCodeGen(this, DuktapeVM._DuktapeDelegates))
                {
                    using (new NamespaceCodeGen(this, this.bindingManager.prefs.ns))
                    {
                        using (new DelegateWrapperCodeGen(this, delegateBindingInfos))
                        {
                            for (var i = 0; i < delegateBindingInfos.Length; i++)
                            {
                                var bindingInfo = delegateBindingInfos[i];
                                this.bindingManager.OnPreGenerateDelegate(bindingInfo);
                                using (new PreservedCodeGen(this))
                                {
                                    using (new DelegateCodeGen(this, bindingInfo, i))
                                    {
                                    }
                                }
                                this.bindingManager.OnPostGenerateDelegate(bindingInfo);
                            }
                        }
                    }
                }
            }
        }

        // 生成类型绑定
        public void Generate(TypeBindingInfo typeBindingInfo)
        {
            using (new PlatformCodeGen(this))
            {
                using (new TopLevelCodeGen(this, typeBindingInfo))
                {
                    using (new NamespaceCodeGen(this, this.bindingManager.prefs.ns, typeBindingInfo.Namespace))
                    {
                        if (typeBindingInfo.IsEnum)
                        {
                            using (new EnumCodeGen(this, typeBindingInfo))
                            {
                            }
                        }
                        else
                        {
                            using (new ClassCodeGen(this, typeBindingInfo))
                            {
                            }
                        }
                    }
                }
            }
        }

        private void WriteAllText(string path, string contents)
        {
            // if (File.Exists(path))
            // {
            //     var old = File.ReadAllText(path);
            //     if (old == contents)
            //     {
            //         return;
            //     }
            // }
            File.WriteAllText(path, contents);
        }

        public void WriteTo(string outDir, string filename, string tx)
        {
            try
            {
                if (this.csharp.enabled && this.csharp.size > 0)
                {
                    var csName = filename + ".cs" + tx;
                    var csPath = Path.Combine(outDir, csName);
                    this.bindingManager.AddOutputFile(csPath);
                    WriteAllText(csPath, this.csharp.ToString());
                }
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write csharp file failed [{0}]: {1}", filename, exception.Message);
            }

            try
            {
                if (this.typescript.enabled && this.typescript.size > 0)
                {
                    var tsName = filename + ".d.ts" + tx;
                    var tsPath = Path.Combine(outDir, tsName);
                    this.bindingManager.AddOutputFile(tsPath);
                    WriteAllText(tsPath, this.typescript.ToString());
                }
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write typescript file failed [{0}]: {1}", filename, exception.Message);
            }
        }

        public void AppendPushValue(Type type, string value)
        {
            //TODO: push 分类需要继续完善
            if (type.IsEnum)
            {
                var eType = type.GetEnumUnderlyingType();
                var eTypeName = this.bindingManager.GetCSTypeFullName(eType);
                this.csharp.AppendLine($"{this.bindingManager.GetDuktapePusher(eType)}(ctx, ({eTypeName}){value});");
                return;
            }
            this.csharp.AppendLine($"{this.bindingManager.GetDuktapePusher(type)}(ctx, {value});");
        }

        public string AppendGetThisCS(FieldBindingInfo bindingInfo)
        {
            return AppendGetThisCS(bindingInfo.isStatic, bindingInfo.fieldInfo.DeclaringType);
        }

        public string AppendGetThisCS(MethodBase method)
        {
            if (method.IsConstructor)
            {
                return null;
            }
            return AppendGetThisCS(method.IsStatic, method.DeclaringType);
        }

        public string AppendGetThisCS(bool isStatic, Type declaringType)
        {
            var caller = "";
            if (isStatic)
            {
                caller = this.bindingManager.GetCSTypeFullName(declaringType);
            }
            else
            {
                caller = "self";
                this.csharp.AppendLine("{0} {1};", this.bindingManager.GetCSTypeFullName(declaringType), caller);
                this.csharp.AppendLine("{0}(ctx, out {1});", this.bindingManager.GetDuktapeThisGetter(declaringType), caller);
            }
            return caller;
        }

        public string AppendGetArgCount(bool isVararg)
        {
            if (isVararg)
            {
                var varName = "argc";
                csharp.AppendLine("var {0} = DuktapeDLL.duk_get_top(ctx);", varName);
                return varName;
            }
            return null;
        }

        public void AppendJSDoc(MemberInfo info)
        {
            var jsdoc = info.GetCustomAttribute(typeof(JSDocAttribute), false) as JSDocAttribute;
            if (jsdoc != null)
            {
                var lines = jsdoc.lines;
                if (lines.Length > 1)
                {
                    this.typescript.AppendLine("/**");
                    foreach (var line in lines)
                    {
                        this.typescript.AppendLine(" * {0}", line.Replace('\r', ' '));
                    }
                }
                else
                {
                    this.typescript.AppendLine("/** {0}", lines[0]);
                }
                this.typescript.AppendLine(" */");
            }
        }
    }
}