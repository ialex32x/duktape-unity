using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duktape
{
    public class DuktapeContext
    {
        private DuktapeVM _vm;
        private IntPtr _ctx;

        // private static int _thread = 0;

        private static Dictionary<IntPtr, DuktapeContext> _contexts = new Dictionary<IntPtr, DuktapeContext>();
        private static IntPtr _lastContextPtr;
        private static DuktapeContext _lastContext;

        public IntPtr rawValue
        {
            get { return this._ctx; }
        }

        public DuktapeVM vm { get { return _vm; } }

        public DuktapeContext(DuktapeVM vm, IntPtr ctx)
        {
            this._vm = vm;
            this._ctx = ctx;
            _contexts[ctx] = this;
            _lastContext = this;
            _lastContextPtr = ctx;
        }

        public static DuktapeVM GetVM(IntPtr ctx)
        {
            return GetContext(ctx)._vm;
        }

        public static DuktapeContext GetContext(IntPtr ctx)
        {
            if (_lastContextPtr == ctx)
            {
                return _lastContext;
            }
            DuktapeContext context;
            if (_contexts.TryGetValue(ctx, out context))
            {
                _lastContext = context;
                _lastContextPtr = ctx;
                return context;
            }
            // fixme 如果是 thread 则获取对应 main context
            return null;
        }

        public void GC(int refid)
        {
            // fixme 加入回收队列
        }
    }
}
