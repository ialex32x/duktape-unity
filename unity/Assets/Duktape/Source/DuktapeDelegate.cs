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

        private static void duk_unity_unref(IntPtr ctx, uint refid, object target)
        {
            DuktapeVM.GetObjectCache(ctx).RemoveJSValue(target);
            DuktapeDLL.duk_unity_unref(ctx, refid);
        }

        protected override void Dispose(bool bManaged)
        {
            _jsInvoker = null;
            if (this._refid != 0 && this._context != null)
            {
                var vm = this._context.vm;
                vm.GC(this._refid, this.target, duk_unity_unref);
                this._refid = 0;
                this.target = null;
            }
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
                    // Debug.Log("DuktapeDelegate based on Dispatcher");
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
        // 调用失败时抛异常， 成功时栈上保留返回值
        public void EndInvoke(IntPtr ctx)
        {
            var nargs = DuktapeDLL.duk_get_top(ctx) - _savedState;
            var ret = DuktapeDLL.duk_pcall_method(ctx, nargs);
            if (ret != DuktapeDLL.DUK_EXEC_SUCCESS)
            {
                DuktapeAux.PrintError(ctx, -1);
                DuktapeDLL.duk_pop(ctx);
                throw new Exception("duktape delegate exception"); 
            }
        }
    }
}
