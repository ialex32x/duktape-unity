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

        public DuktapeDelegate(IntPtr ctx, uint refid, IntPtr heapPtr)
        : base(ctx, refid, heapPtr)
        {
            // Debug.LogErrorFormat("create delegate ptr {0} {1}", heapPtr, refid);
        }

        // private static void duk_unity_unref_delegate(IntPtr ctx, uint refid, object target)
        // {
        //     var cache = DuktapeVM.GetObjectCache(ctx);
        //     cache.RemoveJSValue(target);
        // }

        private static void duk_unity_unref(IntPtr ctx, uint refid, object target)
        {
            var cache = DuktapeVM.GetObjectCache(ctx);
            var t = cache.RemoveDelegate((IntPtr)target);
            // Debug.LogErrorFormat("release delegate ptr {0} {1}", target, t);
            DuktapeDLL.duk_unity_unref(ctx, refid);
        }

        protected override void Dispose(bool bManaged)
        {
            _jsInvoker = null;
            // Debug.LogErrorFormat("Dispose delegate ptr {0} {1}", _refPtr, _refid);
            if (this._refid != 0 && this._context != null)
            {
                var vm = this._context.vm;
                // vm.GC(0, this.target, duk_unity_unref_delegate);
                vm.GC(this._refid, this._refPtr, duk_unity_unref);
                this._refid = 0;
                this._refPtr = IntPtr.Zero;
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
                var ptr = DuktapeDLL.duk_get_heapptr(ctx, -1);
                _jsInvoker = new DuktapeFunction(ctx, DuktapeDLL.duk_unity_ref(ctx), ptr);
            }
            _jsInvoker.Push(ctx); // push function
            this.Push(ctx); // push this
            _savedState = DuktapeDLL.duk_get_top(ctx);
        }

        // 根据当前栈参数数量调用函数
        // 调用失败时抛异常， 成功时栈上保留返回值
        public void EndInvokeWithReturnValue(IntPtr ctx)
        {
            var nargs = DuktapeDLL.duk_get_top(ctx) - _savedState;
            var ret = DuktapeDLL.duk_pcall_method(ctx, nargs);
            if (ret != DuktapeDLL.DUK_EXEC_SUCCESS)
            {
                DuktapeAux.PrintError(ctx, -1);
                DuktapeDLL.duk_pop(ctx);
                throw new Exception("DuktapeDelegate error catch and rethrow"); 
            }
        }
    }
}
