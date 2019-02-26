using System;
using UnityEngine;

namespace Duktape
{
    public class DuktapeTypeValue : DuktapeValue
    {
        private Type _type;

        public Type type
        {
            get { return _type; }
        }

        public DuktapeTypeValue(IntPtr ctx, uint refid, Type type)
        : base(ctx, refid)
        {
            _type = type;
        }
    }
}
