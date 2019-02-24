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
            var prefix = bindingInfo.Namespace != null ? "" : "export ";
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
                        this.cg.csharp.AppendLine("duk_begin_enum(ctx, \"{0}\", typeof({1}));", bindingInfo.regName, bindingInfo.FullName);
                        foreach (var ev in Enum.GetValues(bindingInfo.type))
                        {
                            var value = Convert.ToInt32(ev);
                            var name = ev.ToString();
                            this.cg.csharp.AppendLine("duk_add_const(ctx, \"{0}\", {1});", name, value);
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
