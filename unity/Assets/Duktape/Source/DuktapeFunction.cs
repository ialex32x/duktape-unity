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
    }
}
