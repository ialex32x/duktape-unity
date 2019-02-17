using System;
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

        public void Generate()
        {
            var cg = new CodeGenerator(this);
            foreach (var type in types)
            {
                cg.Generate(type);
            }
        }
    }
}