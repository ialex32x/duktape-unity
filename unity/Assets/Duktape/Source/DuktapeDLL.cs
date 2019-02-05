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
    using duk_bool_t = System.Boolean;

    public class DuktapeDLL
    {
#if UNITY_IPHONE && !UNITY_EDITOR
	    const string DUKTAPEDLL = "__Internal";
#else
        const string DUKTAPEDLL = "duktape";
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int DuktapeCSFunction(IntPtr ctx);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr DuktapeAllocFunction(IntPtr udata, int size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr DuktapeReallocFunction(IntPtr udata, IntPtr ptr, int size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DuktapeFreeFunction(IntPtr udata, IntPtr ptr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DuktapeFatalFunction(IntPtr udata, string msg);
#else
	    public delegate int DuktapeCSFunction(IntPtr ctx);
        public delegate IntPtr DuktapeAllocFunction(IntPtr udata, int size);
        public delegate IntPtr DuktapeReallocFunction(IntPtr udata, IntPtr ptr, int size);
        public delegate void DuktapeFreeFunction(IntPtr udata, IntPtr ptr);
        public delegate void DuktapeFatalFunction(IntPtr udata, string msg);
#endif

        #region Constants
        public static duk_int_t DUK_VARARGS = -1;

        public static duk_uint_t DUK_COMPILE_EVAL = 1U << 3;
        public static duk_uint_t DUK_COMPILE_SAFE = 1U << 7;
        public static duk_uint_t DUK_COMPILE_NORESULT = 1U << 8;
        public static duk_uint_t DUK_COMPILE_NOSOURCE = 1U << 9;
        public static duk_uint_t DUK_COMPILE_STRLEN = 1U << 10;
        public static duk_uint_t DUK_COMPILE_NOFILENAME = 1U << 11;
        #endregion

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duk_create_heap(IntPtr allocFunc, IntPtr reallocFunc, IntPtr freeFunc, IntPtr heapUdata, IntPtr fatalFunc);

        public static IntPtr duk_create_heap_default()
        {
            return duk_create_heap(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        public static IntPtr duk_create_heap(DuktapeAllocFunction allocFunc, DuktapeReallocFunction reallocFunc, DuktapeFreeFunction freeFunc, IntPtr heapUdata, DuktapeFatalFunction fatalFunc)
        {
            var alloc_ptr = Marshal.GetFunctionPointerForDelegate(allocFunc);
            var realloc_ptr = Marshal.GetFunctionPointerForDelegate(reallocFunc);
            var free_ptr = Marshal.GetFunctionPointerForDelegate(freeFunc);
            var fatal_ptr = Marshal.GetFunctionPointerForDelegate(fatalFunc);
            return duk_create_heap(alloc_ptr, realloc_ptr, free_ptr, heapUdata, fatal_ptr);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_destroy_heap(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_compile_raw(IntPtr ctx, byte[] src_buffer, duk_size_t src_length, duk_uint_t flags);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_eval_raw(IntPtr ctx, byte[] src_buffer, duk_size_t src_length, duk_uint_t flags);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_push_c_function(IntPtr ctx, IntPtr func, duk_idx_t nargs);

        public static duk_idx_t duk_push_c_function(IntPtr ctx, DuktapeCSFunction func, duk_idx_t nargs)
        {
            var fn = Marshal.GetFunctionPointerForDelegate(func);
            return duk_push_c_function(ctx, fn, nargs);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_global_object(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_pop(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_pop_n(IntPtr ctx, duk_idx_t count);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_pop_2(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_pop_3(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_put_prop_string(IntPtr ctx, duk_idx_t obj_idx, string key);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_put_prop_lstring(IntPtr ctx, duk_idx_t obj_idx, byte[] key, duk_size_t key_len);

        public static duk_int_t duk_pcompile(IntPtr ctx, uint flags)
        {
            return duk_compile_raw(ctx, null, 0, 2 | flags | DUK_COMPILE_SAFE);
        }

        public static duk_int_t duk_peval_string_noresult(IntPtr ctx, string src)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(src);
            return duk_eval_raw(ctx, bytes, bytes.Length, 0 /*args*/ | DUK_COMPILE_EVAL | DUK_COMPILE_SAFE | DUK_COMPILE_NOSOURCE | DUK_COMPILE_STRLEN | DUK_COMPILE_NORESULT | DUK_COMPILE_NOFILENAME);
        }
    }
}