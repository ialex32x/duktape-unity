using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    public class UnityEngine_Debug : DuktapeBinding  
    {
        public static void reg(IntPtr ctx)
        {
            // duk_begin_namespace(ctx, "UnityEngine");
            // duk_begin_class(ctx, typeof(UnityEngine.Debug), typeof(object));
            // // duk_put_method(ctx, "")
            // duk_end_class(ctx);
            // duk_end_namespace(ctx);
        }
    }
}
