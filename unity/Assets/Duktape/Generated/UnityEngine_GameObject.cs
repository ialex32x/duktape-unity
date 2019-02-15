using System;
using System.Collections.Generic;

namespace Duktape
{
    public class UnityEngine_GameObject : DuktapeBinding  
    {
        public static void reg(IntPtr ctx)
        {
            duk_begin_type(ctx, typeof(UnityEngine.GameObject), typeof(UnityEngine.Object));
        }
    }
}
