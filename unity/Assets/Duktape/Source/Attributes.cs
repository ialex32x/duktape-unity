using System;

namespace Duktape
{
    // 指定类型生成JS类型绑定代码
    [AttributeUsage(AttributeTargets.Class
                  | AttributeTargets.Struct
                  | AttributeTargets.Enum,
                    AllowMultiple = false,
                    Inherited = false)]
    public class JSTypeAttribute : Attribute
    {
    }

    // JS绑定代码
    [AttributeUsage(AttributeTargets.Class,
                    AllowMultiple = false,
                    Inherited = false)]
    public class JSBindingAttribute : Attribute
    {
        // 生成此 binding 代码对应的 duktape-unity 版本 (用于导入时给出版本不一致警告)
        public int Version { get; set; }

        public JSBindingAttribute()
        {
        } 
        
        public JSBindingAttribute(int version)
        {
            this.Version = version;
        }
    }

    // 强制转换为 JS Array
    [AttributeUsage(AttributeTargets.Parameter
                  | AttributeTargets.ReturnValue,
                    AllowMultiple = false)]
    public class JSArrayAttribute : Attribute
    {
    }

    // 在JS中指定名称
    [AttributeUsage(AttributeTargets.Class
                  | AttributeTargets.Struct
                  | AttributeTargets.Enum
                  | AttributeTargets.Field
                  | AttributeTargets.Method
                  | AttributeTargets.Property,
                    AllowMultiple = false)]
    public class JSNamingAttribute : Attribute
    {
        public string name { get; set; }

        public JSNamingAttribute(string name)
        {
            this.name = name;
        }
    }
}
