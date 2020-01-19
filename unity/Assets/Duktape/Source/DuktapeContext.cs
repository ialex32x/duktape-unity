using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duktape
{
    public class DuktapeContext
    {
        private DuktapeVM _vm;
        private IntPtr _ctx;
        // private byte[] _tStringBuffer;

        public IntPtr rawValue { get { return this._ctx; } }

        public DuktapeVM vm { get { return _vm; } }

        public DuktapeContext(DuktapeVM vm, IntPtr ctx)
        {
            this._vm = vm;
            this._ctx = ctx;
            DuktapeVM.addContext(this);
        }

        // public byte[] GetBytes(string v)
        // {
        //     System.Text.Encoding.UTF8.GetBytes(v, 0, _tStringBuffer, 0);
        // }

        public void onDestroy()
        {
            _ctx = IntPtr.Zero;
        }

        // 获取全局函数并调用 (do not cache it)
        public void Invoke(string funcName)
        {
            DuktapeDLL.duk_push_global_object(_ctx);
            DuktapeDLL.duk_get_prop_string(_ctx, -1, funcName);
            if (DuktapeDLL.duk_is_function(_ctx, -1))
            {
                if (DuktapeDLL.duk_pcall(_ctx, 0) != DuktapeDLL.DUK_EXEC_SUCCESS)
                {
                    DuktapeAux.PrintError(_ctx, -1);
                }
            }
            DuktapeDLL.duk_pop_2(_ctx);
        }
    }
}
