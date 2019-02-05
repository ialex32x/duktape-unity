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
    using duk_double_t = System.Double;
    using duk_errcode_t = System.Int32;
    using duk_codepoint_t = System.Int32;
    using duk_ret_t = System.Int32;

    public class DuktapeDLL
    {
#if UNITY_IPHONE && !UNITY_EDITOR
	    const string DUKTAPEDLL = "__Internal";
#else
        const string DUKTAPEDLL = "duktape";
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int duk_c_function(IntPtr ctx);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr duk_alloc_function(IntPtr udata, int size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr duk_realloc_function(IntPtr udata, IntPtr ptr, int size);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void duk_free_function(IntPtr udata, IntPtr ptr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void duk_fatal_function(IntPtr udata, string msg);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void duk_decode_char_function(IntPtr udata, duk_codepoint_t codepoint);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate duk_codepoint_t duk_map_char_function(IntPtr udata, duk_codepoint_t codepoint);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate duk_ret_t duk_safe_call_function(IntPtr ctx, IntPtr udata);
#else
	    public delegate int duk_c_function(IntPtr ctx);
        public delegate IntPtr duk_alloc_function(IntPtr udata, int size);
        public delegate IntPtr duk_realloc_function(IntPtr udata, IntPtr ptr, int size);
        public delegate void duk_free_function(IntPtr udata, IntPtr ptr);
        public delegate void duk_fatal_function(IntPtr udata, string msg);
        public delegate void duk_decode_char_function(IntPtr udata, duk_codepoint_t codepoint);
        public delegate duk_codepoint_t duk_map_char_function(IntPtr udata, duk_codepoint_t codepoint);
        public delegate duk_ret_t duk_safe_call_function(IntPtr ctx, IntPtr udata);
#endif
        [StructLayout(LayoutKind.Sequential)]
        public struct duk_function_list_entry
        {
            public string key;
            public IntPtr value;
            public duk_idx_t nargs;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct duk_number_list_entry
        {
            public string key;
            public duk_double_t value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct duk_time_components
        {
            public duk_double_t year;          /* year, e.g. 2016, ECMAScript year range */
            public duk_double_t month;         /* month: 1-12 */
            public duk_double_t day;           /* day: 1-31 */
            public duk_double_t hours;         /* hour: 0-59 */
            public duk_double_t minutes;       /* minute: 0-59 */
            public duk_double_t seconds;       /* second: 0-59 (in POSIX time no leap second) */
            public duk_double_t milliseconds;  /* may contain sub-millisecond fractions */
            public duk_double_t weekday;       /* weekday: 0-6, 0=Sunday, 1=Monday, ..., 6=Saturday */
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct duk_memory_functions
        {
            public IntPtr alloc_func; /* duk_alloc_function */
            public IntPtr realloc_func; /* duk_realloc_function */
            public IntPtr free_func; /* duk_free_function */
            public IntPtr udata; /* void */
        }

        #region Constants
        public static duk_int_t DUK_VARARGS = -1;

        public static duk_uint_t DUK_COMPILE_EVAL = 1U << 3;
        public static duk_uint_t DUK_COMPILE_SAFE = 1U << 7;
        public static duk_uint_t DUK_COMPILE_NORESULT = 1U << 8;
        public static duk_uint_t DUK_COMPILE_NOSOURCE = 1U << 9;
        public static duk_uint_t DUK_COMPILE_STRLEN = 1U << 10;
        public static duk_uint_t DUK_COMPILE_NOFILENAME = 1U << 11;

        /* Value types, used by e.g. duk_get_type() */
        public static duk_uint_t DUK_TYPE_MIN = 0U;
        public static duk_uint_t DUK_TYPE_NONE = 0U;    /* no value, e.g. invalid index */
        public static duk_uint_t DUK_TYPE_UNDEFINED = 1U;    /* ECMAScript undefined */
        public static duk_uint_t DUK_TYPE_NULL = 2U;    /* ECMAScript null */
        public static duk_uint_t DUK_TYPE_BOOLEAN = 3U;    /* ECMAScript boolean: 0 or 1 */
        public static duk_uint_t DUK_TYPE_NUMBER = 4U;    /* ECMAScript number: double */
        public static duk_uint_t DUK_TYPE_STRING = 5U;    /* ECMAScript string: CESU-8 / extended UTF-8 encoded */
        public static duk_uint_t DUK_TYPE_OBJECT = 6U;    /* ECMAScript object: includes objects, arrays, functions, threads */
        public static duk_uint_t DUK_TYPE_BUFFER = 7U;    /* fixed or dynamic, garbage collected byte buffer */
        public static duk_uint_t DUK_TYPE_POINTER = 8U;    /* raw void pointer */
        public static duk_uint_t DUK_TYPE_LIGHTFUNC = 9U;    /* lightweight function pointer */
        public static duk_uint_t DUK_TYPE_MAX = 9U;

        /* Value mask types, used by e.g. duk_get_type_mask() */
        public static duk_uint_t DUK_TYPE_MASK_NONE = (1U << (int)DUK_TYPE_NONE);
        public static duk_uint_t DUK_TYPE_MASK_UNDEFINED = (1U << (int)DUK_TYPE_UNDEFINED);
        public static duk_uint_t DUK_TYPE_MASK_NULL = (1U << (int)DUK_TYPE_NULL);
        public static duk_uint_t DUK_TYPE_MASK_BOOLEAN = (1U << (int)DUK_TYPE_BOOLEAN);
        public static duk_uint_t DUK_TYPE_MASK_NUMBER = (1U << (int)DUK_TYPE_NUMBER);
        public static duk_uint_t DUK_TYPE_MASK_STRING = (1U << (int)DUK_TYPE_STRING);
        public static duk_uint_t DUK_TYPE_MASK_OBJECT = (1U << (int)DUK_TYPE_OBJECT);
        public static duk_uint_t DUK_TYPE_MASK_BUFFER = (1U << (int)DUK_TYPE_BUFFER);
        public static duk_uint_t DUK_TYPE_MASK_POINTER = (1U << (int)DUK_TYPE_POINTER);
        public static duk_uint_t DUK_TYPE_MASK_LIGHTFUNC = (1U << (int)DUK_TYPE_LIGHTFUNC);
        public static duk_uint_t DUK_TYPE_MASK_THROW = (1U << 10);  /* internal flag value: throw if mask doesn't match */
        public static duk_uint_t DUK_TYPE_MASK_PROMOTE = (1U << 11);  /* internal flag value: promote to object if mask matches */

        /* Flags for duk_push_thread_raw() */
        public static duk_uint_t DUK_THREAD_NEW_GLOBAL_ENV = (1U << 0);    /* create a new global environment */

        /* Flags for duk_gc() */
        public static duk_uint_t DUK_GC_COMPACT = (1U << 0);    /* compact heap objects */

        /* Error codes (must be 8 bits at most, see duk_error.h) */
        public static duk_int_t DUK_ERR_NONE = 0;    /* no error (e.g. from duk_get_error_code()) */
        public static duk_int_t DUK_ERR_ERROR = 1;    /* Error */
        public static duk_int_t DUK_ERR_EVAL_ERROR = 2;    /* EvalError */
        public static duk_int_t DUK_ERR_RANGE_ERROR = 3;    /* RangeError */
        public static duk_int_t DUK_ERR_REFERENCE_ERROR = 4;    /* ReferenceError */
        public static duk_int_t DUK_ERR_SYNTAX_ERROR = 5;    /* SyntaxError */
        public static duk_int_t DUK_ERR_TYPE_ERROR = 6;    /* TypeError */
        public static duk_int_t DUK_ERR_URI_ERROR = 7;    /* URIError */

        #endregion

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duk_create_heap(IntPtr allocFunc, IntPtr reallocFunc, IntPtr freeFunc, IntPtr heapUdata, IntPtr fatalFunc);

        public static IntPtr duk_create_heap_default()
        {
            return duk_create_heap(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        }

        public static IntPtr duk_create_heap(duk_alloc_function allocFunc, duk_realloc_function reallocFunc, duk_free_function freeFunc, IntPtr heapUdata, duk_fatal_function fatalFunc)
        {
            var alloc_ptr = Marshal.GetFunctionPointerForDelegate(allocFunc);
            var realloc_ptr = Marshal.GetFunctionPointerForDelegate(reallocFunc);
            var free_ptr = Marshal.GetFunctionPointerForDelegate(freeFunc);
            var fatal_ptr = Marshal.GetFunctionPointerForDelegate(fatalFunc);
            return duk_create_heap(alloc_ptr, realloc_ptr, free_ptr, heapUdata, fatal_ptr);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_suspend(IntPtr ctx, IntPtr state);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_resume(IntPtr ctx, IntPtr state);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_destroy_heap(IntPtr ctx);

        /*
        *  Memory management
        *
        *  Raw functions have no side effects (cannot trigger GC).
        */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duk_alloc_raw(IntPtr ctx, duk_size_t size);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_free_raw(IntPtr ctx, IntPtr ptr);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duk_realloc_raw(IntPtr ctx, IntPtr ptr, duk_size_t size);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duk_alloc(IntPtr ctx, duk_size_t size);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_free(IntPtr ctx, IntPtr ptr);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duk_realloc(IntPtr ctx, IntPtr ptr, duk_size_t size);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_get_memory_functions(IntPtr ctx, ref duk_memory_functions out_funcs);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_gc(IntPtr ctx, duk_uint_t flags);

        /*
        *  Error handling
        */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_throw_raw(IntPtr ctx);

        public static void duk_throw(IntPtr ctx)
        {
            duk_throw_raw(ctx);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_fatal_raw(IntPtr ctx, string err_msg);

        public static void duk_fatal(IntPtr ctx, string err_msg)
        {
            duk_fatal_raw(ctx, err_msg);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_error_raw(IntPtr ctx, duk_errcode_t err_code, string filename, duk_int_t line, string fmt, params object[] args);

        /*
         *  Pop operations
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_pop(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_pop_n(IntPtr ctx, duk_idx_t count);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_pop_2(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_pop_3(IntPtr ctx);

        /*
        *  Type checks
        *
        *  duk_is_none(), which would indicate whether index it outside of stack,
        *  is not needed; duk_is_valid_index() gives the same information.
        */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_get_type(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_check_type(IntPtr ctx, duk_idx_t idx, duk_int_t type);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_uint_t duk_get_type_mask(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_check_type_mask(IntPtr ctx, duk_idx_t idx, duk_uint_t mask);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_undefined(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_null(IntPtr ctx, duk_idx_t idx);

        public static duk_bool_t duk_is_null_or_undefined(IntPtr ctx, duk_idx_t idx)
        {
            return (duk_get_type_mask(ctx, idx) & (DUK_TYPE_MASK_NULL | DUK_TYPE_MASK_UNDEFINED)) != 0;
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_boolean(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_number(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_nan(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_string(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_object(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_buffer(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_buffer_data(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_pointer(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_lightfunc(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_symbol(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_array(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_function(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_c_function(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_ecmascript_function(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_bound_function(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_thread(IntPtr ctx, duk_idx_t idx);

        public static duk_bool_t duk_is_callable(IntPtr ctx, duk_idx_t idx)
        {
            return duk_is_function(ctx, idx);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_constructable(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_dynamic_buffer(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_fixed_buffer(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_external_buffer(IntPtr ctx, duk_idx_t idx);

        /* Buffers and lightfuncs are not considered primitive because they mimic
         * objects and e.g. duk_to_primitive() will coerce them instead of returning
         * them as is.  Symbols are represented as strings internally.
         */
        public static duk_bool_t duk_is_primitive(IntPtr ctx, duk_idx_t idx)
        {
            return duk_check_type_mask(ctx, idx, DUK_TYPE_MASK_UNDEFINED |
                                          DUK_TYPE_MASK_NULL |
                                          DUK_TYPE_MASK_BOOLEAN |
                                          DUK_TYPE_MASK_NUMBER |
                                          DUK_TYPE_MASK_STRING |
                                          DUK_TYPE_MASK_POINTER);
        }

        /* Symbols are object coercible, covered by DUK_TYPE_MASK_STRING. */
        public static duk_bool_t duk_is_object_coercible(IntPtr ctx, duk_idx_t idx)
        {
            return duk_check_type_mask(ctx, idx, DUK_TYPE_MASK_BOOLEAN |
                                          DUK_TYPE_MASK_NUMBER |
                                          DUK_TYPE_MASK_STRING |
                                          DUK_TYPE_MASK_OBJECT |
                                          DUK_TYPE_MASK_BUFFER |
                                          DUK_TYPE_MASK_POINTER |
                                          DUK_TYPE_MASK_LIGHTFUNC);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_errcode_t duk_get_error_code(IntPtr ctx, duk_idx_t idx);

        public static duk_bool_t duk_is_error(IntPtr ctx, duk_idx_t idx)
        {
            return duk_get_error_code(ctx, idx) != 0;
        }

        public static duk_bool_t duk_is_eval_error(IntPtr ctx, duk_idx_t idx)
        {
            return duk_get_error_code(ctx, idx) == DUK_ERR_EVAL_ERROR;
        }

        public static duk_bool_t duk_is_range_error(IntPtr ctx, duk_idx_t idx)
        {
            return duk_get_error_code(ctx, idx) == DUK_ERR_RANGE_ERROR;
        }

        public static duk_bool_t duk_is_reference_error(IntPtr ctx, duk_idx_t idx)
        {
            return duk_get_error_code(ctx, idx) == DUK_ERR_REFERENCE_ERROR;
        }

        public static duk_bool_t duk_is_syntax_error(IntPtr ctx, duk_idx_t idx)
        {
            return duk_get_error_code(ctx, idx) == DUK_ERR_SYNTAX_ERROR;
        }

        public static duk_bool_t duk_is_type_error(IntPtr ctx, duk_idx_t idx)
        {
            return duk_get_error_code(ctx, idx) == DUK_ERR_TYPE_ERROR;
        }

        public static duk_bool_t duk_is_uri_error(IntPtr ctx, duk_idx_t idx)
        {
            return duk_get_error_code(ctx, idx) == DUK_ERR_URI_ERROR;
        }

        /*
        *  Get operations: no coercion, returns default value for invalid
        *  indices and invalid value types.
        *
        *  duk_get_undefined() and duk_get_null() would be pointless and
        *  are not included.
        */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_get_boolean(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_double_t duk_get_number(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_get_int(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_uint_t duk_get_uint(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern string duk_get_string(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte[] duk_get_lstring(IntPtr ctx, duk_idx_t idx, ref duk_size_t out_len);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte[] duk_get_buffer(IntPtr ctx, duk_idx_t idx, ref duk_size_t out_size);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte[] duk_get_buffer_data(IntPtr ctx, duk_idx_t idx, ref duk_size_t out_size);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duk_get_pointer(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duk_get_c_function(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duk_get_context(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern byte[] duk_get_heapptr(IntPtr ctx, duk_idx_t idx);

        /*
        *  Other state related functions
        */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_strict_call(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_constructor_call(IntPtr ctx);

        /*
        *  Stack management
        */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_normalize_index(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_require_normalize_index(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_is_valid_index(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_require_valid_index(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_get_top(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_set_top(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_get_top_index(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_require_top_index(IntPtr ctx);

        /* Although extra/top could be an unsigned type here, using a signed type
        * makes the API more robust to calling code calculation errors or corner
        * cases (where caller might occasionally come up with negative values).
        * Negative values are treated as zero, which is better than casting them
        * to a large unsigned number.  (This principle is used elsewhere in the
        * API too.)
        */
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_check_stack(IntPtr ctx, duk_idx_t extra);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_require_stack(IntPtr ctx, duk_idx_t extra);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_check_stack_top(IntPtr ctx, duk_idx_t top);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_require_stack_top(IntPtr ctx, duk_idx_t top);

        /*
        *  Stack manipulation (other than push/pop)
        */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_swap(IntPtr ctx, duk_idx_t idx1, duk_idx_t idx2);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_swap_top(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_dup(IntPtr ctx, duk_idx_t from_idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_dup_top(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_insert(IntPtr ctx, duk_idx_t to_idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_replace(IntPtr ctx, duk_idx_t to_idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_copy(IntPtr ctx, duk_idx_t from_idx, duk_idx_t to_idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_remove(IntPtr ctx, duk_idx_t idx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_xcopymove_raw(IntPtr to_ctx, IntPtr from_ctx, duk_idx_t count, duk_bool_t is_copy);

        public static void duk_xmove_top(IntPtr to_ctx, IntPtr from_ctx, duk_idx_t count)
        {
            duk_xcopymove_raw((to_ctx), (from_ctx), (count), false /*is_copy*/);
        }

        public static void duk_xcopy_top(IntPtr to_ctx, IntPtr from_ctx, duk_idx_t count)
        {
            duk_xcopymove_raw((to_ctx), (from_ctx), (count), true /*is_copy*/);
        }

        /*
         *  Push operations
         *
         *  Push functions return the absolute (relative to bottom of frame)
         *  position of the pushed value for convenience.
         *
         *  Note: duk_dup() is technically a push.
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_undefined(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_null(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_boolean(IntPtr ctx, duk_bool_t val);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_true(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_false(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_number(IntPtr ctx, duk_double_t val);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_nan(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_int(IntPtr ctx, duk_int_t val);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_uint(IntPtr ctx, duk_uint_t val);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_put_prop_string(IntPtr ctx, duk_idx_t obj_idx, string key);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_put_prop_lstring(IntPtr ctx, duk_idx_t obj_idx, byte[] key, duk_size_t key_len);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_pointer(IntPtr ctx, IntPtr p);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_this(IntPtr ctx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_new_target(IntPtr ctx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_current_function(IntPtr ctx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_current_thread(IntPtr ctx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_global_object(IntPtr ctx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_heap_stash(IntPtr ctx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_global_stash(IntPtr ctx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_thread_stash(IntPtr ctx, IntPtr target_ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_push_object(IntPtr ctx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_push_bare_object(IntPtr ctx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_push_array(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_push_c_function(IntPtr ctx, IntPtr func, duk_idx_t nargs);

        public static duk_idx_t duk_push_c_function(IntPtr ctx, duk_c_function func, duk_idx_t nargs)
        {
            var fn = Marshal.GetFunctionPointerForDelegate(func);
            return duk_push_c_function(ctx, fn, nargs);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_push_c_lightfunc(IntPtr ctx, IntPtr func, duk_idx_t nargs, duk_idx_t length, duk_int_t magic);

        public static duk_idx_t duk_push_c_lightfunc(IntPtr ctx, duk_c_function func, duk_idx_t nargs, duk_idx_t length, duk_int_t magic)
        {
            var fn = Marshal.GetFunctionPointerForDelegate(func);
            return duk_push_c_lightfunc(ctx, fn, nargs, length, magic);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_push_thread_raw(IntPtr ctx, duk_uint_t flags);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_idx_t duk_push_proxy(IntPtr ctx, duk_uint_t proxy_flags);


        /*
         *  Inspection
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_inspect_value(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_inspect_callstack_entry(IntPtr ctx, duk_int_t level);

        /*
         *  Object prototype
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_get_prototype(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_set_prototype(IntPtr ctx, duk_idx_t idx);

        /*
         *  Object finalizer
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_get_finalizer(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_set_finalizer(IntPtr ctx, duk_idx_t idx);

        /*
         *  Global object
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_set_global_object(IntPtr ctx);

        /*
         *  Duktape/C function magic value
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_get_magic(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_set_magic(IntPtr ctx, duk_idx_t idx, duk_int_t magic);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_get_current_magic(IntPtr ctx);

        /*
         *  Module helpers: put multiple function or constant properties
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_put_function_list(IntPtr ctx, duk_idx_t obj_idx, ref duk_function_list_entry funcs);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_put_number_list(IntPtr ctx, duk_idx_t obj_idx, ref duk_number_list_entry numbers);

        /*
         *  Object operations
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_compact(IntPtr ctx, duk_idx_t obj_idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_enum(IntPtr ctx, duk_idx_t obj_idx, duk_uint_t enum_flags);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_next(IntPtr ctx, duk_idx_t enum_idx, duk_bool_t get_value);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_seal(IntPtr ctx, duk_idx_t obj_idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_freeze(IntPtr ctx, duk_idx_t obj_idx);

        /*
         *  String manipulation
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_concat(IntPtr ctx, duk_idx_t count);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_join(IntPtr ctx, duk_idx_t count);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_decode_string(IntPtr ctx, duk_idx_t idx, IntPtr callback, IntPtr udata);
        public static void duk_decode_string(IntPtr ctx, duk_idx_t idx, duk_decode_char_function callback, IntPtr udata)
        {
            var callback_fn = Marshal.GetFunctionPointerForDelegate(callback);
            duk_decode_string(ctx, idx, callback_fn, udata);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_map_string(IntPtr ctx, duk_idx_t idx, IntPtr callback, IntPtr udata);
        public static void duk_map_string(IntPtr ctx, duk_idx_t idx, duk_map_char_function callback, IntPtr udata)
        {
            var callback_fn = Marshal.GetFunctionPointerForDelegate(callback);
            duk_map_string(ctx, idx, callback_fn, udata);
        }

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_substring(IntPtr ctx, duk_idx_t idx, duk_size_t start_char_offset, duk_size_t end_char_offset);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_trim(IntPtr ctx, duk_idx_t idx);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_codepoint_t duk_char_code_at(IntPtr ctx, duk_idx_t idx, duk_size_t char_offset);

        /*
         *  ECMAScript operators
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_equals(IntPtr ctx, duk_idx_t idx1, duk_idx_t idx2);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_strict_equals(IntPtr ctx, duk_idx_t idx1, duk_idx_t idx2);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_samevalue(IntPtr ctx, duk_idx_t idx1, duk_idx_t idx2);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_instanceof(IntPtr ctx, duk_idx_t idx1, duk_idx_t idx2);

        /*
         *  Random
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_double_t duk_random(IntPtr ctx);

        /*
         *  Function (method) calls
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_call(IntPtr ctx, duk_idx_t nargs);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_call_method(IntPtr ctx, duk_idx_t nargs);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_call_prop(IntPtr ctx, duk_idx_t obj_idx, duk_idx_t nargs);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_pcall(IntPtr ctx, duk_idx_t nargs);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_pcall_method(IntPtr ctx, duk_idx_t nargs);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_pcall_prop(IntPtr ctx, duk_idx_t obj_idx, duk_idx_t nargs);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_new(IntPtr ctx, duk_idx_t nargs);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_pnew(IntPtr ctx, duk_idx_t nargs);
        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_safe_call(IntPtr ctx, IntPtr func, IntPtr udata, duk_idx_t nargs, duk_idx_t nrets);
        public static duk_int_t duk_safe_call(IntPtr ctx, duk_safe_call_function func, IntPtr udata, duk_idx_t nargs, duk_idx_t nrets)
        {
            var func_fn = Marshal.GetFunctionPointerForDelegate(func);
            return duk_safe_call(ctx, func_fn, udata, nargs, nrets);
        }

        /*
         *  Compilation and evaluation
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_eval_raw(IntPtr ctx, byte[] src_buffer, duk_size_t src_length, duk_uint_t flags);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_int_t duk_compile_raw(IntPtr ctx, byte[] src_buffer, duk_size_t src_length, duk_uint_t flags);

        public static duk_int_t duk_pcompile(IntPtr ctx, uint flags)
        {
            return duk_compile_raw(ctx, null, 0, 2 | flags | DUK_COMPILE_SAFE);
        }

        public static duk_int_t duk_peval_string_noresult(IntPtr ctx, string src)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(src);
            return duk_eval_raw(ctx, bytes, bytes.Length, 0 /*args*/ | DUK_COMPILE_EVAL | DUK_COMPILE_SAFE | DUK_COMPILE_NOSOURCE | DUK_COMPILE_STRLEN | DUK_COMPILE_NORESULT | DUK_COMPILE_NOFILENAME);
        }


        /*
         *  Bytecode load/dump
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_dump_function(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_load_function(IntPtr ctx);


        /*
         *  Debugging
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_push_context_dump(IntPtr ctx);


        /*
         *  Debugger (debug protocol)
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_debugger_attach(IntPtr ctx,
                                                   IntPtr read_cb,
                                                   IntPtr write_cb,
                                                   IntPtr peek_cb,
                                                   IntPtr read_flush_cb,
                                                   IntPtr write_flush_cb,
                                                   IntPtr request_cb,
                                                   IntPtr detached_cb,
                                                   IntPtr udata);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_debugger_detach(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_debugger_cooperate(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_bool_t duk_debugger_notify(IntPtr ctx, duk_idx_t nvalues);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_debugger_pause(IntPtr ctx);


        /*
         *  Time handling
         */

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_double_t duk_get_now(IntPtr ctx);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_time_to_components(IntPtr ctx, duk_double_t timeval, ref duk_time_components comp);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern duk_double_t duk_components_to_time(IntPtr ctx, ref duk_time_components comp);

    }
}