using System;
using UnityEngine;

namespace Duktape
{
    // 包装了一个 js function/eventdispatcher
    // 对于 function 调用时传入的 this 为 function 自身
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
            // Debug.Log($"BeginInvoke: {_savedState}");
            if (_jsInvoker == null)
            {
                this.Push(ctx); // push this
                if (!DuktapeDLL.duk_is_function(ctx, -1)) 
                {
                    DuktapeDLL.duk_get_prop_string(ctx, -1, "dispatch");
                    DuktapeDLL.duk_remove(ctx, -2); // remove this
                }
                _jsInvoker = new DuktapeFunction(ctx, DuktapeDLL.duk_unity_ref(ctx));
            }
            _jsInvoker.Push(ctx); // push function
            this.Push(ctx); // push this
            _savedState = DuktapeDLL.duk_get_top(ctx);
        }

        // 根据当前栈参数数量调用函数
        public void EndInvoke(IntPtr ctx)
        {
            var nargs = DuktapeDLL.duk_get_top(ctx) - _savedState;
            // Debug.Log($"EndInvoke: {nargs}");
            var ret = DuktapeDLL.duk_pcall_method(ctx, nargs);
            if (ret != DuktapeDLL.DUK_EXEC_SUCCESS)
            {
                DuktapeAux.PrintError(ctx, -1);
                // throw new Exception(err); 
            }
        }
    }
}
