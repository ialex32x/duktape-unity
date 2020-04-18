using System;

namespace Duktape
{
    using UnityEngine;

    public class DuktapeThread
    {
        private DuktapeValue _thread;
        private DuktapeContext _threadContext;
        
        public DuktapeThread(DuktapeFunction fn)
        {
            var ctx = fn.ctx;
            var vm = DuktapeVM.GetContext(ctx).vm;
            var idx = DuktapeDLL.duk_push_thread(ctx);

            DuktapeDLL.duk_dup(ctx, -1);
            var ptr = DuktapeDLL.duk_get_heapptr(ctx, -1);
            _thread = new DuktapeValue(ctx, DuktapeDLL.duk_unity_ref(ctx), ptr);
            _threadContext = new DuktapeContext(vm, DuktapeDLL.duk_get_context(ctx, idx));
            if (fn.Push(_threadContext.rawValue))
            {
            }

            DuktapeDLL.duk_pop(ctx);
        }
    
        public bool Resume(out object value)
        {
            var ctx = _thread.ctx;
            DuktapeDLL.duk_push_c_function(ctx, DuktapeDLL.duk_unity_thread_resume, DuktapeDLL.DUK_VARARGS);
            if (_thread.Push(ctx))
            {
                DuktapeDLL.duk_push_null(ctx); // 预留
                DuktapeDLL.duk_call(ctx, 2);
                DuktapeBinding.duk_get_var(ctx, -1, out value);
                DuktapeDLL.duk_pop(ctx);
                if (_thread.Push(ctx))
                {
                    var state = DuktapeDLL.duk_unity_thread_state(ctx);
                    DuktapeDLL.duk_pop(ctx);
                    return state != 5;
                }
            }
            else
            {
                DuktapeDLL.duk_pop(ctx);
            }

            value = null;
            return false;
        }
    }
}
