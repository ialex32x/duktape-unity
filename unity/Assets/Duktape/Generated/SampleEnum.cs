#if UNITY_STANDALONE_OSX
// UserName: huliangjie @ 2/26/2019 9:23:05 AM
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Type: SampleEnum
using System;
using System.Collections.Generic;

namespace DuktapeJS {
    using Duktape;
    [JSBindingAttribute(65537)]
    [UnityEngine.Scripting.Preserve]
    public class DuktapeJS_SampleEnum : DuktapeBinding {
        [UnityEngine.Scripting.Preserve]
        public static int reg(IntPtr ctx)
        {
            duk_begin_namespace(ctx);
            duk_begin_enum(ctx, "SampleEnum", typeof(SampleEnum));
            duk_add_const(ctx, "a", 0);
            duk_add_const(ctx, "b", 1);
            duk_add_const(ctx, "c", 2);
            duk_end_enum(ctx);
            duk_end_namespace(ctx);
            return 0;
        }
    }
}
#endif
