using System;

namespace Duktape
{
    using UnityEngine;

    public class DuktapeThread
    {
        private DuktapeContext _threadContext;
        
        public DuktapeThread(DuktapeFunction fn)
        {
            var ctx = fn.ctx;
            var vm = DuktapeVM.GetContext(ctx).vm;
            var idx = DuktapeDLL.duk_push_thread(ctx);
            
            _threadContext = new DuktapeContext(vm, DuktapeDLL.duk_get_context(ctx, idx));
            if (fn.Push(_threadContext.rawValue))
            {
            }

            DuktapeDLL.duk_pop(ctx);
        }
    
        public bool Resume(out object value)
        {
            //TODO: 未完成, 需要暴露接口以更方便操作 thread
            // value = Duktape.Thread.resume(_threadContext.rawValue);
            // done = Duktape.info(_threadContext.rawValue).tstate == 5;
            value = null;
            return false;
        }
    }
}
