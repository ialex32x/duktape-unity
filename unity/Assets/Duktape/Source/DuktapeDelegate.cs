using System;
using UnityEngine;

namespace Duktape
{
    public class DuktapeDelegate : DuktapeFunction
    {
        public Delegate target;

        public DuktapeDelegate(IntPtr ctx, uint refid)
        : base(ctx, refid)
        {
        }
    }
}
