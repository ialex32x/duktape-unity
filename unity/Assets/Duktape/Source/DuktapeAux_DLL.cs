using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace Duktape
{
    using duk_idx_t = System.Int32;

    // 对原始导入函数的简单封装
    public static partial class DuktapeAux
    {
        public static string duk_require_string(IntPtr ctx, duk_idx_t idx)
        {
            var ptr = DuktapeDLL.duk_require_string(ctx, idx);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static byte[] duk_require_lstring(IntPtr ctx, duk_idx_t idx)
        {
            var size_t = 0U;
            var ptr = DuktapeDLL.duk_unity_require_lstring(ctx, idx, out size_t);
            var out_size = (int)size_t;
            if (ptr != IntPtr.Zero && out_size > 0)
            {
                var bytes = new byte[out_size];
                Marshal.Copy(ptr, bytes, 0, out_size);
                return bytes;
            }
            return null;
        }

        public static byte[] duk_require_buffer(IntPtr ctx, duk_idx_t idx)
        {
            var size_t = 0U;
            var ptr = DuktapeDLL.duk_unity_require_lstring(ctx, idx, out size_t);
            var out_size = (int)size_t;
            if (ptr != IntPtr.Zero && out_size > 0)
            {
                var bytes = new byte[out_size];
                Marshal.Copy(ptr, bytes, 0, out_size);
                return bytes;
            }
            return null;
        }

        public static byte[] duk_require_buffer_data(IntPtr ctx, duk_idx_t idx)
        {
            var size_t = 0U;
            var ptr = DuktapeDLL.duk_unity_require_buffer_data(ctx, idx, out size_t);
            var out_size = (int)size_t;
            if (ptr != IntPtr.Zero && out_size > 0)
            {
                var bytes = new byte[out_size];
                Marshal.Copy(ptr, bytes, 0, out_size);
                return bytes;
            }
            return null;
        }

        public static string duk_to_string(IntPtr ctx, duk_idx_t idx)
        {
            var ptr = DuktapeDLL.duk_to_string(ctx, idx);
            return Marshal.PtrToStringAnsi(ptr);
        }

        public static byte[] duk_to_lstring(IntPtr ctx, duk_idx_t idx)
        {
            var size_t = 0U;
            var ptr = DuktapeDLL.duk_unity_to_lstring(ctx, idx, out size_t);
            var out_size = (int)size_t;
            if (ptr != IntPtr.Zero && out_size > 0)
            {
                var bytes = new byte[out_size];
                Marshal.Copy(ptr, bytes, 0, out_size);
                return bytes;
            }
            return null;
        }

        public static byte[] duk_get_lstring(IntPtr ctx, duk_idx_t idx)
        {
            var size_t = 0U;
            var ptr = DuktapeDLL.duk_unity_get_lstring(ctx, idx, out size_t);
            var out_size = (int)size_t;
            if (ptr != IntPtr.Zero && out_size > 0)
            {
                var bytes = new byte[out_size];
                Marshal.Copy(ptr, bytes, 0, out_size);
                return bytes;
            }
            return null;
        }

        public static byte[] duk_to_buffer(IntPtr ctx, duk_idx_t idx)
        {
            var size_t = 0U;
            var ptr = DuktapeDLL.duk_to_buffer(ctx, idx, out size_t);
            var out_size = (int)size_t;
            if (ptr != IntPtr.Zero && out_size > 0)
            {
                var bytes = new byte[out_size];
                Marshal.Copy(ptr, bytes, 0, out_size);
                return bytes;
            }
            return null;
        }

        public static byte[] duk_get_buffer(IntPtr ctx, duk_idx_t idx)
        {
            var size_t = 0U;
            var ptr = DuktapeDLL.duk_unity_get_buffer(ctx, idx, out size_t);
            var out_size = (int)size_t;
            if (ptr != IntPtr.Zero && out_size > 0)
            {
                var bytes = new byte[out_size];
                Marshal.Copy(ptr, bytes, 0, out_size);
                return bytes;
            }
            return null;
        }

        public static byte[] duk_get_buffer_data(IntPtr ctx, duk_idx_t idx)
        {
            var size_t = 0U;
            var ptr = DuktapeDLL.duk_unity_get_buffer_data(ctx, idx, out size_t);
            if (ptr != IntPtr.Zero)
            {
                var size = (int)size_t;
                var bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
            return null;
        }
    }
}

