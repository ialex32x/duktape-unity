using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace Duktape
{
    using duk_size_t = System.Int32;
    using duk_int_t = System.Int32;
    using duk_idx_t = System.Int32;
    using duk_uint_t = System.UInt32;
    using duk_uarridx_t = System.UInt32;
    using duk_bool_t = System.Boolean;
    using duk_double_t = System.Double;
    using duk_errcode_t = System.Int32;
    using duk_codepoint_t = System.Int32;
    using duk_ret_t = System.Int32;
    using duk_int32_t = System.Int32;
    using duk_uint32_t = System.UInt32;
    using duk_int16_t = System.Int16;
    using duk_uint16_t = System.UInt16;

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

