using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace Duktape
{
    using duk_idx_t = System.Int32;
    
    public static partial class DuktapeAux
    {
        public static string duk_require_string(IntPtr ctx, duk_idx_t idx)
        {
            var ptr = DuktapeDLL.duk_require_string(ctx, idx);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static string duk_get_string(IntPtr ctx, duk_idx_t idx)
        {
            var ptr = DuktapeDLL.duk_get_string(ctx, idx);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static string duk_to_string(IntPtr ctx, duk_idx_t idx)
        {
            var ptr = DuktapeDLL.duk_to_string(ctx, idx);
            return Marshal.PtrToStringAnsi(ptr);
        }
    }
}

