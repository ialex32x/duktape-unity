using System;
using UnityEngine;

namespace Duktape
{
    // 基础环境
    public static partial class DuktapeAux
    {
        public static bool printStacktrace = false;

        public static Type GetType(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            var type = System.Reflection.Assembly.GetExecutingAssembly().GetType(name);
            return type;
        }

        public static void PrintError(IntPtr ctx, int idx)
        {
            PrintError(ctx, idx, null);
        }

        public static void PrintError(IntPtr ctx, int idx, string filename)
        {
            string err;
            if (DuktapeDLL.duk_is_error(ctx, idx))
            {
                DuktapeDLL.duk_get_prop_string(ctx, idx, "stack");
                err = DuktapeDLL.duk_safe_to_string(ctx, -1);
                DuktapeDLL.duk_pop(ctx);
            }
            else
            {
                err = DuktapeDLL.duk_safe_to_string(ctx, idx);
            }

            if (filename != null)
            {
                Debug.LogError($"[JSError][{filename}] {err}");
            }
            else
            {
                Debug.LogError($"[JSError] {err}");
            }
        }

        public static void duk_open(IntPtr ctx)
        {
            DuktapeDLL.duk_push_global_object(ctx);
            DuktapeDLL.duk_push_c_function(ctx, duk_print, DuktapeDLL.DUK_VARARGS);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "print");

            DuktapeDLL.duk_push_c_function(ctx, duk_dofile, 1);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "dofile");

            DuktapeDLL.duk_push_c_function(ctx, duk_dostring, 1);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "dostring");

            DuktapeDLL.duk_push_c_function(ctx, duk_addSearchPath, 1);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "addSearchPath");

            DuktapeDLL.duk_push_c_function(ctx, duk_enableStacktrace, 1);
            DuktapeDLL.duk_put_prop_string(ctx, -2, "enableStacktrace");

            // DuktapeDLL.duk_push_c_function(ctx, duk_stacktrace, 0);
            // DuktapeDLL.duk_put_prop_string(ctx, -2, "stacktrace");

            DuktapeDLL.duk_pop(ctx);
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int duk_addSearchPath(IntPtr ctx)
        {
            var path = DuktapeAux.duk_require_string(ctx, 0);
            DuktapeVM.GetVM(ctx).AddSearchPath(path);
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int duk_print(IntPtr ctx)
        {
            var narg = DuktapeDLL.duk_get_top(ctx);
            var str = string.Empty;
            for (int i = 0; i < narg; i++)
            {
                object o;
                if (DuktapeBinding.duk_get_object(ctx, i, out o))
                {
                    str += (o == null ? "(null)" : o.ToString()) + " ";
                }
                else
                {
                    str += DuktapeDLL.duk_safe_to_string(ctx, i) + " ";
                }
            }
            if (printStacktrace)
            {
                var stacktrace = "stacktrace:\n";
                for (int i = -2; ; i--)
                {
                    DuktapeDLL.duk_inspect_callstack_entry(ctx, i);
                    if (!DuktapeDLL.duk_is_undefined(ctx, -1))
                    {
                        DuktapeDLL.duk_get_prop_string(ctx, -1, "lineNumber");
                        var lineNumber = DuktapeDLL.duk_to_int(ctx, -1);
                        DuktapeDLL.duk_get_prop_string(ctx, -2, "function");
                        DuktapeDLL.duk_get_prop_string(ctx, -1, "name");
                        var funcName = DuktapeDLL.duk_safe_to_string(ctx, -1);
                        DuktapeDLL.duk_get_prop_string(ctx, -2, "fileName");
                        var fileName = DuktapeDLL.duk_safe_to_string(ctx, -1);
                        DuktapeDLL.duk_pop_n(ctx, 4);
                        if (lineNumber != 0)
                        {
                            if (string.IsNullOrEmpty(funcName))
                            {
                                funcName = "[anonymous]";
                            }
                            stacktrace += $"{funcName} ({fileName}:{lineNumber})\n";
                        }
                        else
                        {
                            stacktrace += $"{funcName} (<native code>)\n";
                        }
                    }
                    else
                    {
                        DuktapeDLL.duk_pop(ctx);
                        break;
                    }
                }
                str += $"\n{stacktrace}";
            }
            Debug.Log(str);
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int duk_dofile(IntPtr ctx)
        {
            var filename = DuktapeAux.duk_require_string(ctx, 0);
            DuktapeVM.GetVM(ctx).EvalFile(filename);
            return 0;
        }

        [AOT.MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int duk_dostring(IntPtr ctx)
        {
            var source = DuktapeDLL.duk_get_string(ctx, 0);
            var filename = DuktapeDLL.duk_get_string(ctx, 1);
            DuktapeVM.GetVM(ctx).EvalSource(source, filename);
            return 0;
        }

        public static int duk_enableStacktrace(IntPtr ctx)
        {
            printStacktrace = DuktapeDLL.duk_require_boolean(ctx, 0);
            return 0;
        }

        // public static int duk_stacktrace(IntPtr ctx)
        // {
        //     DuktapeDLL.duk_inspect_callstack_entry(ctx, -2);
        //     DuktapeDLL.duk_get_prop_string(ctx, -1, "lineNumber");
        //     var lineNumber = DuktapeDLL.duk_to_int(ctx, -1);
        //     DuktapeDLL.duk_pop(ctx);
        //     DuktapeDLL.duk_get_prop_string(ctx, -1, "function");
        //     DuktapeDLL.duk_get_prop_string(ctx, -1, "fileName");
        //     var fileName = DuktapeDLL.duk_safe_to_string(ctx, -1);
        //     DuktapeDLL.duk_pop_2(ctx);
        //     DuktapeDLL.duk_push_string(ctx, $"{fileName}:{lineNumber}");
        //     return 1;
        // }
    }
}
