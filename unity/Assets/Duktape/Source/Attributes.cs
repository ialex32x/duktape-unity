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
    }

    // 强制转换为 JS Array
    [AttributeUsage(AttributeTargets.Parameter
                  | AttributeTargets.ReturnValue, 
                    AllowMultiple = false)]
    public class JSArrayAttribute : Attribute
    {
    }
}
