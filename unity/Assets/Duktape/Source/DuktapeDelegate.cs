using System;
using UnityEngine;

namespace Duktape
{
    public class DuktapeDelegate : DuktapeValue
    {
        // cache the delegate object
        public Delegate target;
        private int _savedState;

        public DuktapeDelegate(IntPtr ctx, uint refid)
        : base(ctx, refid)
        {
        }

        // 记录栈状态
        public void BeginInvoke(IntPtr ctx)
        {
            _savedState = DuktapeDLL.duk_get_top(ctx);
            //TODO: 改成 push invoke 属性
            this.Push(ctx);
        }

        // 根据当前栈参数数量调用函数
        public void EndInvoke(IntPtr ctx)
        {
            var nargs = DuktapeDLL.duk_get_top(ctx) - _savedState;
            var ret = DuktapeDLL.duk_pcall(ctx, nargs);
            if (ret != DuktapeDLL.DUK_EXEC_SUCCESS)
            {
                var err = DuktapeAux.duk_to_string(ctx, -1);
                // throw new Exception(err); 
                Debug.LogError(err);
            }
        }
    }
}
