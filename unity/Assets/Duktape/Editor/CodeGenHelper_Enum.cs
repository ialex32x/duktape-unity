using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class EnumCodeGen : TypeCodeGen
    {
        public EnumCodeGen(CodeGenerator cg, TypeBindingInfo type)
        : base(cg, type)
        {
            this.cg.AppendJSDoc(type.type);
            var prefix = bindingInfo.Namespace != null ? "" : "declare ";
            this.cg.typescript.AppendLine("{0}enum {1} {{", prefix, bindingInfo.regName);
            this.cg.typescript.AddTabLevel();
        }

        public override void Dispose()
        {
            using (new PreservedCodeGen(cg))
            {
                using (new RegFuncCodeGen(cg))
                {
                    using (new RegFuncNamespaceCodeGen(cg, bindingInfo))
                    {
                        this.cg.csharp.AppendLine("duk_begin_enum(ctx, \"{0}\", typeof({1}));",
                            bindingInfo.regName,
                            this.cg.bindingManager.GetCSTypeFullName(bindingInfo.type));
                        var values = new Dictionary<string, int>();
                        foreach (var ev in Enum.GetValues(bindingInfo.type))
                        {
                            values[Enum.GetName(bindingInfo.type, ev)] = Convert.ToInt32(ev);
                        }
                        foreach (var kv in values)
                        {
                            var name = kv.Key;
                            var value = kv.Value;
                            this.cg.csharp.AppendLine("duk_add_const(ctx, \"{0}\", {1}, {2});", name, value, -2);
                            this.cg.typescript.AppendLine("{0} = {1},", name, value);
                        }
                        this.cg.csharp.AppendLine("duk_end_enum(ctx);");
                    }
                }
                base.Dispose();
            }
            this.cg.typescript.DecTabLevel();
            this.cg.typescript.AppendLine("}");
        }
    }
}
