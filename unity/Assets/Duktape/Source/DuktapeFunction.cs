using System;
using UnityEngine;

namespace Duktape
{
    public class DuktapeFunction : DuktapeValue, Invokable
    {
        private DuktapeValue[] _argv;

        public DuktapeFunction(IntPtr ctx, uint refid)
        : base(ctx, refid)
        {
        }

        public DuktapeFunction(IntPtr ctx, uint refid, DuktapeValue[] argv)
        : base(ctx, refid)
        {
            _argv = argv;
        }

        // push 当前函数的 prototype 
        public void PushPrototype(IntPtr ctx)
        {
            this.Push(ctx);
            DuktapeDLL.duk_get_prop_string(ctx, -1, "prototype");
            DuktapeDLL.duk_remove(ctx, -2);
        }

        // 函数对象以及nargs数的参数必须由调用者保证已经入栈, 然后才能调用 _InternalCall
        // 调用完成后栈顶为函数返回值, 或异常对象
        public void _InternalPCall(IntPtr ctx, int nargs)
        {
            if (_argv != null)
            {
                var length = _argv.Length;
                nargs += length;
                for (var i = 0; i < length; i++)
                {
                    _argv[i].Push(ctx);
                }
            }
            var ret = DuktapeDLL.duk_pcall(ctx, nargs);
            if (ret != DuktapeDLL.DUK_EXEC_SUCCESS)
            {
                DuktapeAux.PrintError(ctx, -1);
                // throw new Exception(err); 
            }
        }

        public void Invoke()
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            _InternalPCall(ctx, 0);
            DuktapeDLL.duk_pop(ctx);
        }

        // 传参调用, 如果此函数已携带js参数, js参数排在invoke参数后
        public void Invoke(object arg0)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeBinding.duk_push_var(ctx, arg0);
            _InternalPCall(ctx, 1);
            DuktapeDLL.duk_pop(ctx);
        }

        public void Invoke(object arg0, object arg1)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeBinding.duk_push_var(ctx, arg0);
            DuktapeBinding.duk_push_var(ctx, arg1);
            _InternalPCall(ctx, 2);
            DuktapeDLL.duk_pop(ctx);
        }

        public void Invoke(object arg0, object arg1, object arg2)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeBinding.duk_push_var(ctx, arg0);
            DuktapeBinding.duk_push_var(ctx, arg1);
            DuktapeBinding.duk_push_var(ctx, arg2);
            _InternalPCall(ctx, 3);
            DuktapeDLL.duk_pop(ctx);
        }

        public void Invoke(object arg0, object arg1, object arg2, params object[] args)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeBinding.duk_push_var(ctx, arg0);
            DuktapeBinding.duk_push_var(ctx, arg1);
            DuktapeBinding.duk_push_var(ctx, arg2);
            var size = args.Length;
            for (var i = 0; i < size; i++)
            {
                DuktapeBinding.duk_push_var(ctx, args[i]);
            }
            _InternalPCall(ctx, size + 3);
            DuktapeDLL.duk_pop(ctx);
        }
    }
}
