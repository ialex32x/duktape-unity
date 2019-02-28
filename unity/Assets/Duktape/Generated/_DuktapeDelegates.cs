#if UNITY_STANDALONE_WIN
// UserName: julio @ 2019/2/28 23:37:25
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
        public static void _DuktapeDelegates0(DuktapeFunction fn, string a, string b) {
            // generate binding code here
            // var ctx = fn.GetContext().rawValue;
            // fn.Push(ctx);
            // push arguments here...
            // fn._InternalCall(ctx, 2);
        }
        [UnityEngine.Scripting.Preserve]
        [Duktape.JSDelegateAttribute(typeof(SampleClass.DelegateFoo4))]
        public static void _DuktapeDelegates1(DuktapeFunction fn, int a, float b) {
            // generate binding code here
            // var ctx = fn.GetContext().rawValue;
            // fn.Push(ctx);
            // push arguments here...
            // fn._InternalCall(ctx, 2);
        }
    }
}
#endif
