using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duktape
{
    public class DuktapeContext
    {
        private DuktapeVM _vm;
        private IntPtr _ctx;

        public IntPtr rawValue
        {
            get
            {
                return this._ctx;
            }
        }

        public DuktapeVM vm { get { return _vm; } }

        public DuktapeContext(DuktapeVM vm, IntPtr ctx)
        {
            this._vm = vm;
            this._ctx = ctx;
            DuktapeVM.addContext(this);
        }

        public void OnDestroy()
        {
            _ctx = IntPtr.Zero;
        }

        public static DuktapeVM GetVM(IntPtr ctx)
        {
            return DuktapeVM.GetContext(ctx)._vm;
        }
    }
}
