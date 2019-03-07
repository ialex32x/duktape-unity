using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class TypeTransform
    {
        private Dictionary<string, string> _redirectedMethods = new Dictionary<string, string>();

        public TypeTransform AddRedirectMethod(string from, string to)
        {
            _redirectedMethods[from] = to;
            return this;
        }

        public bool TryRedirectMethod(string name, out string to)
        {
            return _redirectedMethods.TryGetValue(name, out to);
        }

        public bool IsRedirectedMethod(string name)
        {
            return _redirectedMethods.ContainsKey(name);
        }
    }
}
