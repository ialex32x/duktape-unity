using System;
using UnityEngine;

namespace Duktape
{
    public class DuktapeFunction : DuktapeValue
    {
        public DuktapeFunction(IntPtr ctx, int refid)
        : base(ctx, refid)
        {
        }

        // push 当前函数的 prototype 
        public void PushPrototype(IntPtr ctx)
        {
            this.Push(ctx);
            DuktapeDLL.duk_get_prop_string(ctx, -1, "prototype");
            DuktapeDLL.duk_remove(ctx, -2);
        }
    }
}
