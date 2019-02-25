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
            var tab = Prefs.GetPrefs().tab;
            var newline = Prefs.GetPrefs().newline;
            csharp = new TextGenerator(newline, tab);
            typescript = new TextGenerator(newline, tab);
        }

        public void Clear()
        {
            csharp.Clear();
            typescript.Clear();
        }

        public void Generate(TypeBindingInfo typeBindingInfo)
        {
            Clear();
            using (new PlatformCodeGen(this))
            {
                using (new TopLevelCodeGen(this, typeBindingInfo))
                {
                    using (new NamespaceCodeGen(this, Prefs.GetPrefs().ns, typeBindingInfo))
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

        public void WriteTo(string outDir, string filename, string tx)
        {
            try
            {
                if (this.csharp.enabled)
                {
                    var csName = filename + ".cs" + tx;
                    var csPath = Path.Combine(outDir, csName);
                    this.bindingManager.AddOutputFile(csPath);
                    File.WriteAllText(csPath, this.csharp.ToString());
                }
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write csharp file failed [{0}]: {1}", filename, exception.Message);
            }

            try
            {
                if (this.typescript.enabled)
                {
                    var tsName = filename + ".d.ts" + tx;
                    var tsPath = Path.Combine(outDir, tsName);
                    this.bindingManager.AddOutputFile(tsPath);
                    File.WriteAllText(tsPath, this.typescript.ToString());
                }
            }
            catch (Exception exception)
            {
                this.bindingManager.Error("write typescript file failed [{0}]: {1}", filename, exception.Message);
            }
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
            return "duk_get_classvalue";
        }

        public void AppendPushValue(Type type, string value)
        {
            //TODO: push 分类需要继续完善
            if (type.IsEnum)
            {
                this.csharp.AppendLine("duk_push_any(ctx, ({0}){1});", type.GetEnumUnderlyingType().FullName, value);
                return;
            }
            this.csharp.AppendLine("duk_push_any(ctx, {0});", value);
        }

        public string AppendGetThisCS(FieldBindingInfo bindingInfo)
        {
            return AppendGetThisCS(bindingInfo.isStatic, bindingInfo.fieldInfo.DeclaringType);
        }

        public string AppendGetThisCS(MethodInfo method)
        {
            return AppendGetThisCS(method.IsStatic, method.DeclaringType);
        }

        public string AppendGetThisCS(bool isStatic, Type declaringType)
        {
            var caller = "";
            if (isStatic)
            {
                caller = declaringType.FullName;
            }
            else
            {
                caller = "self";
                this.csharp.AppendLine("{0} {1};", this.bindingManager.GetTypeFullNameCS(declaringType), caller);
                this.csharp.AppendLine("{0}(ctx, out {1});", this.GetDuktapeThisGetter(declaringType), caller);
            }
            return caller;
        }

        public string AppendGetArgc(bool isVararg)
        {
            if (isVararg)
            {
                var varName = "argc";
                csharp.AppendLine("var {0} = DuktapeDLL.duk_get_top(ctx);", varName);
                return varName;
            }
            return null;
        }

        public string AppendGetParameters(string argc, ParameterInfo[] parameters, List<ParameterInfo> parametersByRef)
        {
            var arglist = "";
            for (var i = 0; i < parameters.Length; i++)
            {
                var parameter = parameters[i];
                //TODO: 需要处理 ref/out 参数在 js 中的返回方式问题
                //      可能的处理方式是将这些参数合并函数返回值转化为一个 object 作为最终返回值
                if (parameter.IsOut)
                {
                    arglist += "out ";
                    if (parametersByRef != null)
                    {
                        parametersByRef.Add(parameter);
                    }
                }
                else if (parameter.ParameterType.IsByRef)
                {
                    arglist += "ref ";
                    if (parametersByRef != null)
                    {
                        parametersByRef.Add(parameter);
                    }
                }
                arglist += "arg" + i;
                if (i != parameters.Length - 1)
                {
                    arglist += ", ";
                }
                if (argc != null && i == parameters.Length - 1)
                {
                    this.csharp.AppendLine("{0} arg{1} = new {2}[{3}];",
                        this.bindingManager.GetTypeFullNameCS(parameter.ParameterType),
                        i,
                        this.bindingManager.GetTypeFullNameCS(parameter.ParameterType.GetElementType()),
                        i == 0 ? argc : argc + " - " + i);
                    this.csharp.AppendLine("for (var i = {0}; i < {1}; i++)", i, argc);
                    this.csharp.AppendLine("{");
                    this.csharp.AddTabLevel();
                    {
                        // this.csharp.AppendLine("{0} el;", this.bindingManager.GetTypeFullNameCS(parameter.ParameterType.GetElementType()));
                        this.csharp.AppendLine("{0}(ctx, i, out arg{1}[i{2}]);",
                            this.GetDuktapeGetter(parameter.ParameterType.GetElementType()),
                            i,
                            i == 0 ? "" : " - " + i);
                        // this.csharp.AppendLine("arg{0}[i] = el;", i);
                    }
                    this.csharp.DecTabLevel();
                    this.csharp.AppendLine("}");
                }
                else
                {
                    this.csharp.AppendLine("{0} arg{1};", this.bindingManager.GetTypeFullNameCS(parameter.ParameterType), i);
                    this.csharp.AppendLine("{0}(ctx, {1}, out arg{1});", this.GetDuktapeGetter(parameter.ParameterType), i);
                }
            }
            return arglist;
        }
    }
}