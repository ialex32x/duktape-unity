using System;
using System.Runtime.InteropServices;
using System.Text;
using AOT;

namespace Duktape
{
    public class DuktapeDLL
    {
#if UNITY_IPHONE && !UNITY_EDITOR
	    const string DUKTAPEDLL = "__Internal";
#else
        const string DUKTAPEDLL = "duktape";
#endif

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int DuktapeCSFunction(IntPtr dukContext);
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr DuktapeAllocFunction(IntPtr udata, int size);
#else
	    public delegate int DuktapeCSFunction(IntPtr dukContext);
        public delegate IntPtr DuktapeAllocFunction(IntPtr udata, int size);
#endif

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr duk_create_heap(IntPtr allocFunc, IntPtr reallocFunc, IntPtr freeFunc, IntPtr heapUdata, IntPtr fatalHandler);

        [DllImport(DUKTAPEDLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern void duk_destroy_heap(IntPtr ctx);

    }
}