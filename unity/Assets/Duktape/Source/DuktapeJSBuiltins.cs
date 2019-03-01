using System;
using System.Collections.Generic;
using System.Reflection;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public class DuktapeJSBuiltins : DuktapeBinding
    {
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int Enum_GetName(IntPtr ctx)
        {
            Debug.LogWarning("not implemented");
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int ClearTimer(IntPtr ctx)
        {
            if (DuktapeDLL.duk_is_number(ctx, 0))
            {
                var id = DuktapeDLL.duk_get_int(ctx, 0);
                DuktapeDLL.duk_push_boolean(ctx, DuktapeRunner.Clear(id));
                return 1;
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int SetInterval(IntPtr ctx)
        {
            DuktapeFunction fn;
            var idx = _GetTimerFunction(ctx, out fn);
            if (idx < 0)
            {
                var id = DuktapeRunner.SetInterval(fn, (float)DuktapeDLL.duk_get_number(ctx, 1));
                DuktapeDLL.duk_push_int(ctx, id);
                return 1;
            }
            return DuktapeDLL.duk_generic_error(ctx, "invalid arg " + idx);
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int SetTimeout(IntPtr ctx)
        {
            DuktapeFunction fn;
            var idx = _GetTimerFunction(ctx, out fn);
            if (idx < 0)
            {
                var id = DuktapeRunner.SetTimeout(fn, (float)DuktapeDLL.duk_get_number(ctx, 1));
                DuktapeDLL.duk_push_int(ctx, id);
                return 1;
            }
            return DuktapeDLL.duk_generic_error(ctx, "invalid arg " + idx);
        }

        private static int _GetTimerFunction(IntPtr ctx, out DuktapeFunction fn)
        {
            if (!DuktapeDLL.duk_is_function(ctx, 0))
            {
                fn = null;
                return 0;
            }
            if (!DuktapeDLL.duk_is_number(ctx, 1))
            {
                fn = null;
                return 1;
            }
            var top_index = DuktapeDLL.duk_get_top_index(ctx); 
            // Debug.Log($"_GetTimerFunction {top} ?? {DuktapeDLL.duk_get_top(ctx)}");
            DuktapeValue[] argv = null;
            if (top_index > 1)
            {
                argv = new DuktapeValue[top_index - 1];
                for (var i = 2; i <= top_index; i++)
                {
                    DuktapeDLL.duk_dup(ctx, i);
                    argv[i - 2] = new DuktapeValue(ctx, DuktapeDLL.duk_unity_ref(ctx));
                }
            }
            DuktapeDLL.duk_dup(ctx, 0);
            fn = new DuktapeFunction(ctx, DuktapeDLL.duk_unity_ref(ctx), argv);
            return -1;
        }

        //TODO: (ialex32x, 未完成) delegate 操作封装 (没想好)
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int DelegateAdder(IntPtr ctx)
        {
            return 0;
        }

        //TODO: (ialex32x, 未完成) delegate 操作封装 (没想好)
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int DelegateRemover(IntPtr ctx)
        {
            return 0;
        }

        //TODO: (ialex32x, 未完成) 传入 Type, 创建对应的 List<T>
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int CreateList(IntPtr ctx)
        {
            Type type;
            if (duk_get_classvalue(ctx, 1, out type))
            {
                var gtype = typeof(List<>).MakeGenericType(type);
                var ctors = gtype.GetConstructors(BindingFlags.Public);
                for (var i = 0; i < ctors.Length; i++)
                {
                    var ctor = ctors[i];
                    if (ctor.GetParameters().Length == 0)
                    {
                        var instance = ctor.Invoke(Type.EmptyTypes);
                        duk_push_any(ctx, instance);
                        return 1;
                    }
                }
            }
            return 0;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int GetType(IntPtr ctx)
        {
            string name;
            duk_get_primitive(ctx, 1, out name);
            //TODO: type 缓存
            //TODO: 从 jsobject hidden property 中读 refid
            var type = DuktapeAux.GetType(name);
            duk_push_any(ctx, type);
            return 1;
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int IsNull(IntPtr ctx)
        {
            object o;
            var res = DuktapeDLL.duk_is_null_or_undefined(ctx, 1);
            if (!res
            && duk_get_classvalue(ctx, 1, out o)
            && o != null
            && (!(o is UnityEngine.Object) || (o as UnityEngine.Object) != null))
            {
                res = false;
            }
            DuktapeDLL.duk_push_boolean(ctx, res);
            return 1;
        }

        //TODO: (ialex32x, 未完成) 强制关联 Special 
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind(IntPtr ctx)
        {
            // var vm = DuktapeVM.GetVM(ctx);
            // string name;
            // duk_get_primitive(ctx, 2, out name);
            // var special = vm.GetSpecial(name);
            // ...
            return 0;
        }

        public static void reg(IntPtr ctx)
        {
            duk_begin_namespace(ctx, "DuktapeJS");
            {
                duk_begin_special(ctx, DuktapeVM.SPECIAL_ENUM);
                duk_add_method(ctx, "GetName", Enum_GetName, -3);
                duk_end_special(ctx);
            }
            {
                duk_begin_special(ctx, DuktapeVM.SPECIAL_DELEGATE);
                duk_add_method(ctx, "add", DelegateAdder, -3);
                duk_add_method(ctx, "remove", DelegateRemover, -3);
                duk_end_special(ctx);
            }
            {
                duk_begin_special(ctx, DuktapeVM.SPECIAL_CSHARP);
                duk_add_method(ctx, "CreateList", CreateList, -3);
                duk_add_method(ctx, "GetType", GetType, -3);
                duk_add_method(ctx, "IsNull", IsNull, -3);
                duk_add_method(ctx, "Bind", Bind, -3);
                duk_end_special(ctx);
            }
            duk_end_namespace(ctx);
            duk_add_method(ctx, "setInterval", SetInterval, -1);
            duk_add_method(ctx, "setTimeout", SetTimeout, -1);
            duk_add_method(ctx, "clearInterval", ClearTimer, -1);
            duk_add_method(ctx, "clearTimeout", ClearTimer, -1);
        }
    }
}
