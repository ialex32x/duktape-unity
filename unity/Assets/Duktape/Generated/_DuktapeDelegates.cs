#if UNITY_STANDALONE_WIN
// Special: _DuktapeDelegates
using System;
using System.Collections.Generic;

namespace DuktapeJS {
    using Duktape;
    [JSBindingAttribute(65537)]
    [UnityEngine.Scripting.Preserve]
    public class _DuktapeDelegates : DuktapeBinding {
        [UnityEngine.Scripting.Preserve]
        [Duktape.JSDelegateAttribute(typeof(SampleClass.DelegateFoo))]
        [Duktape.JSDelegateAttribute(typeof(SampleClass.DelegateFoo2))]
        public static void _DuktapeDelegates0(Duktape.DuktapeDelegate fn, string a, string b) {
            var ctx = fn.GetContext().rawValue;
            fn.BeginInvoke(ctx);
            duk_push_any(ctx, a);
            duk_push_any(ctx, b);
            fn.EndInvoke(ctx);
        }
        [UnityEngine.Scripting.Preserve]
        [Duktape.JSDelegateAttribute(typeof(SampleClass.DelegateFoo4))]
        public static double _DuktapeDelegates1(Duktape.DuktapeDelegate fn, int a, float b) {
            var ctx = fn.GetContext().rawValue;
            fn.BeginInvoke(ctx);
            duk_push_any(ctx, a);
            duk_push_any(ctx, b);
            fn.EndInvoke(ctx);
            double ret0;
            duk_get_primitive(ctx, -1, out ret0);
            return ret0;
        }
        [UnityEngine.Scripting.Preserve]
        [Duktape.JSDelegateAttribute(typeof(System.Action))]
        public static void _DuktapeDelegates2(Duktape.DuktapeDelegate fn) {
            var ctx = fn.GetContext().rawValue;
            fn.Invoke(ctx);
        }
        [UnityEngine.Scripting.Preserve]
        [Duktape.JSDelegateAttribute(typeof(System.Action<string>))]
        public static void _DuktapeDelegates3(Duktape.DuktapeDelegate fn, string obj) {
            var ctx = fn.GetContext().rawValue;
            fn.BeginInvoke(ctx);
            duk_push_any(ctx, obj);
            fn.EndInvoke(ctx);
        }
        [UnityEngine.Scripting.Preserve]
        public static int reg(IntPtr ctx)
        {
            var type = typeof(_DuktapeDelegates);
            var vm = DuktapeVM.GetVM(ctx);
            var methods = type.GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            for (int i = 0, size = methods.Length; i < size; i++)
            {
                var method = methods[i];
                var attributes = method.GetCustomAttributes(typeof(JSDelegateAttribute), false);
                var attributesLength = attributes.Length;
                if (attributesLength > 0)
                {
                    for (var a = 0; a < attributesLength; a++)
                    {
                        var attribute = attributes[a] as JSDelegateAttribute;
                        vm.AddDelegate(attribute.target, method);
                    }
                }
            }
            return 0;
        }
    }
}
#endif
