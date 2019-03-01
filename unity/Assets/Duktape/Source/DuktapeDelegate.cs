using System;
using UnityEngine;

namespace Duktape
{
    public class DuktapeDelegate : DuktapeValue
    {
        // cache the delegate object
        public Delegate target;
        private int _savedState;

        private DuktapeFunction _jsInvoker;

        public DuktapeDelegate(IntPtr ctx, uint refid)
        : base(ctx, refid)
        {
        }

        public void Invoke(IntPtr ctx)
        {
            this.BeginInvoke(ctx);
            this.EndInvoke(ctx);
        }

        // 记录栈状态
        public void BeginInvoke(IntPtr ctx)
        {
            _savedState = DuktapeDLL.duk_get_top(ctx);
            //TODO: 改成 push invoke 属性
            if (_jsInvoker == null)
            {
                this.PushProperty(ctx, "dispatch");
                _jsInvoker = new DuktapeFunction(ctx, DuktapeDLL.duk_unity_ref(ctx));
            }
            _jsInvoker.Push(ctx); // push function
            this.Push(ctx); // push this
        }

        // 根据当前栈参数数量调用函数
        public void EndInvoke(IntPtr ctx)
        {
            var nargs = DuktapeDLL.duk_get_top(ctx) - _savedState;
            var ret = DuktapeDLL.duk_pcall_method(ctx, nargs);
            if (ret != DuktapeDLL.DUK_EXEC_SUCCESS)
            {
                var err = DuktapeAux.duk_to_string(ctx, -1);
                // throw new Exception(err); 
                Debug.LogError(err);
            }
        }
    }
}
