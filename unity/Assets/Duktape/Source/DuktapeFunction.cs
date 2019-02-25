using System;
using UnityEngine;

namespace Duktape
{
    public class DuktapeFunction : DuktapeValue
    {
        public DuktapeFunction(IntPtr ctx, uint refid)
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

        public void Call()
        {
            var ctx = _ctx.rawValue;
            this.Push(ctx);
            var ret = DuktapeDLL.duk_pcall(ctx, 0);
            if (ret != DuktapeDLL.DUK_EXEC_SUCCESS)
            {
                var err = DuktapeAux.duk_to_string(ctx, -1);
                DuktapeDLL.duk_pop(ctx);
                throw new Exception(err);
            }
            DuktapeDLL.duk_pop(ctx);
        }

        // public void Call(object args)
    }
}
