using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    /// utility helps to compile js source into bytecode
    public class DuktapeCompiler : IDisposable
    {
        private IntPtr _ctx = IntPtr.Zero;
        // private DuktapeVM _vm;

        public DuktapeCompiler()
        {
            _ctx = DuktapeDLL.duk_create_heap_default();
            // _vm = new DuktapeVM();
            // _vm.Initialize(null);
        }

        ~DuktapeCompiler()
        {
            Dispose(false);
        }

        public byte[] Compile(string filename)
        {
            return Compile(filename, File.ReadAllBytes(filename));
        }

        public byte[] Compile(string filename, byte[] bytes)
        {
            try
            {
                // return _vm.DumpBytecode(filename, bytes);
                return DuktapeAux.DumpBytecode(_ctx, filename, bytes);
            }
            catch (Exception exception)
            {
                Debug.LogError(exception);
                return null;
            }
        }

        public virtual void Dispose(bool bManaged)
        {
            // if (_vm != null)
            // {
            //     _vm.Destroy();
            //     _vm = null;
            // }
            if (_ctx != IntPtr.Zero)
            {
                DuktapeDLL.duk_destroy_heap(_ctx);
                _ctx = IntPtr.Zero;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}