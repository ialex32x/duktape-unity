using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;
    using System.Xml;

    public class DocResolver
    {
        public class DocBody
        {
            public string[] summary;
            public Dictionary<string, string> parameters = new Dictionary<string, string>();
            public string returns;
        }

        private StringBuilder _sb = new StringBuilder();
        private Dictionary<string, DocBody> _tdocs = new Dictionary<string, DocBody>();
        private Dictionary<string, DocBody> _pdocs = new Dictionary<string, DocBody>();
        private Dictionary<string, DocBody> _fdocs = new Dictionary<string, DocBody>();
        private Dictionary<string, DocBody> _mdocs = new Dictionary<string, DocBody>();

        private static Dictionary<Assembly, DocResolver> _resolvers = new Dictionary<Assembly, DocResolver>();

        public static DocResolver GetResolver(Assembly assembly)
        {
            DocResolver resolver;
            if (!_resolvers.TryGetValue(assembly, out resolver))
            {
                resolver = _resolvers[assembly] = new DocResolver();
                resolver.Load(assembly);
            }
            return resolver;
        }

        public static DocBody GetDocBody(Type type)
        {
            return GetResolver(type.Assembly)._GetDocBody(type);
        }

        public static DocBody GetDocBody<T>(T methodBase)
        where T : MethodBase
        {
            return GetResolver(methodBase.DeclaringType.Assembly)._GetDocBody(methodBase);
        }

        public static DocBody GetDocBody(FieldInfo fieldInfo)
        {
            return GetResolver(fieldInfo.DeclaringType.Assembly)._GetDocBody(fieldInfo);
        }

        public static DocBody GetDocBody(PropertyInfo propertyInfo)
        {
            return GetResolver(propertyInfo.DeclaringType.Assembly)._GetDocBody(propertyInfo);
        }

        public void Load(Assembly assembly)
        {
            var location = assembly.Location;
            var ext = Path.GetExtension(location);
            var xlocation = location.Substring(0, location.Length - ext.Length) + ".xml";
            ParseXml(xlocation);
        }

        public DocBody GetFieldDocBody(string path)
        {
            DocBody body;
            _fdocs.TryGetValue(path, out body);
            return body;
        }

        private DocBody _GetDocBody(Type type)
        {
            if (type.IsGenericType || type.IsGenericTypeDefinition || !type.IsPublic)
            {
                return null;
            }
            var xName = type.FullName;
            DocBody body;
            _tdocs.TryGetValue(xName, out body);
            return body;
        }

        private DocBody _GetDocBody<T>(T methodBase)
        where T : MethodBase
        {
            if (methodBase.IsGenericMethod || !methodBase.IsPublic || methodBase.ContainsGenericParameters)
            {
                return null;
            }
            var declType = methodBase.DeclaringType;
            _sb.Clear();
            _sb.Append(declType.FullName);
            _sb.Append('.');
            _sb.Append(methodBase.Name);
            _sb.Append('(');
            if (!ExtractMethodParamters(methodBase, _sb))
            {
                return null;
            }
            _sb.Append(')');
            var xName = _sb.ToString();
            DocBody body;
            _mdocs.TryGetValue(xName, out body);
            return body;
        }

        private DocBody _GetDocBody(FieldInfo fieldInfo)
        {
            if (!fieldInfo.IsPublic)
            {
                return null;
            }
            var declType = fieldInfo.DeclaringType;
            var xName = declType.FullName + "." + fieldInfo.Name;
            DocBody body;
            _fdocs.TryGetValue(xName, out body);
            return body;
        }

        private DocBody _GetDocBody(PropertyInfo propertyInfo)
        {
            if (propertyInfo.GetMethod == null || !propertyInfo.GetMethod.IsPublic)
            {
                return null;
            }
            var declType = propertyInfo.DeclaringType;
            var xName = declType.FullName + "." + propertyInfo.Name;
            DocBody body;
            _pdocs.TryGetValue(xName, out body);
            return body;
        }

        private bool ExtractMethodParamters<T>(T methodBase, StringBuilder sb)
        where T : MethodBase
        {
            var parameters = methodBase.GetParameters();
            for (int i = 0, size = parameters.Length; i < size; i++)
            {
                var type = parameters[i].ParameterType;
                if (type.IsGenericType)
                {
                    return false;
                }
                sb.Append(type.FullName);
                if (i != size - 1)
                {
                    sb.Append(',');
                }
            }
            return true;
        }

        private void ParseXmlMember(XmlReader reader, DocBody body, string elementName)
        {
            while (reader.Read())
            {
                var type = reader.NodeType;
                if (type == XmlNodeType.EndElement && reader.Name == elementName)
                {
                    break;
                }
                if (type == XmlNodeType.Element && reader.Name == "summary")
                {
                    body.summary = ReadTextBlock(reader, "summary");
                }
                if (type == XmlNodeType.Element && reader.Name == "param")
                {
                    var pname = reader.GetAttribute("name");
                    body.parameters[pname] = ReadSingleTextBlock(reader, "param");
                }
                if (type == XmlNodeType.Element && reader.Name == "returns")
                {
                    body.returns = ReadSingleTextBlock(reader, "returns");
                }
            }
        }

        private string[] ReadTextBlock(XmlReader reader, string elementName)
        {
            var lines = new List<string>();
            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    var type = reader.NodeType;
                    if (type == XmlNodeType.EndElement && reader.Name == elementName)
                    {
                        break;
                    }
                    if (type == XmlNodeType.Element && reader.Name == "para")
                    {
                        lines.Add(reader.ReadElementContentAsString());
                    }
                }
            }
            return lines.ToArray();
        }

        private string ReadSingleTextBlock(XmlReader reader, string elementName)
        {
            _sb.Clear();
            if (!reader.IsEmptyElement)
            {
                while (reader.Read())
                {
                    var type = reader.NodeType;
                    if (type == XmlNodeType.EndElement && reader.Name == elementName)
                    {
                        break;
                    }
                    if (type == XmlNodeType.Element && reader.Name == "para")
                    {
                        _sb.Append(reader.ReadElementContentAsString());
                        _sb.Append(' ');
                    }
                    if (type == XmlNodeType.Text)
                    {
                        _sb.Append(reader.Value);
                    }
                }
            }
            return _sb.ToString();
        }

        public void ParseXml(string filename)
        {
            if (!File.Exists(filename))
            {
                return;
            }
            // Debug.LogFormat("read doc: {0}", filename);
            using (var fs = File.OpenRead(filename))
            {
                using (var reader = XmlReader.Create(fs))
                {
                    while (reader.Read())
                    {
                        var type = reader.NodeType;
                        if (type == XmlNodeType.Element && reader.Name == "member")
                        {
                            var body = new DocBody();
                            var name = reader.GetAttribute("name");
                            switch (name[0])
                            {
                                case 'F': _fdocs[name.Substring(2)] = body; break;
                                case 'P': _pdocs[name.Substring(2)] = body; break;
                                case 'M': _mdocs[name.Substring(2)] = body; break;
                                case 'T': _tdocs[name.Substring(2)] = body; break;
                            }

                            ParseXmlMember(reader, body, "member");
                        }
                    }
                }
            }
        }
    }
}
