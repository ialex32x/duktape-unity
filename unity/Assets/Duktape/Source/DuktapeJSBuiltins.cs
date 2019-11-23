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

        // 创建一个 native array
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int Array_Create(IntPtr ctx)
        {
            Type type;
            int size;
            if (duk_get_type(ctx, 0, out type))
            {
                if (type != null)
                {
                    if (DuktapeDLL.duk_is_number(ctx, 1) && duk_get_primitive(ctx, 1, out size))
                    {
                        var o = Array.CreateInstance(type, size);
                        duk_push_classvalue(ctx, o);
                        return 1;
                    }
                    else
                    {
                        return DuktapeDLL.duk_generic_error(ctx, "invalid size");
                    }
                }
                else
                {
                    return DuktapeDLL.duk_generic_error(ctx, "invalid type");
                }
            }
            return DuktapeDLL.duk_generic_error(ctx, "invalid args");
        }

        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int ClearTimer(IntPtr ctx)
        {
            if (DuktapeDLL.duk_is_number(ctx, 0))
            {
                var id = DuktapeDLL.duk_get_uint(ctx, 0);
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
                var id = DuktapeRunner.SetInterval(fn, DuktapeDLL.duk_get_int(ctx, 1));
                DuktapeDLL.duk_push_uint(ctx, id);
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
                var id = DuktapeRunner.SetTimeout(fn, DuktapeDLL.duk_get_int(ctx, 1));
                DuktapeDLL.duk_push_uint(ctx, id);
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
                        duk_push_classvalue(ctx, instance);
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
            duk_push_classvalue(ctx, type);
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
            // DuktapeDLL.duk_push_global_object(ctx);
            // DuktapeDLL.duk_put_global_string(ctx, "global");
            DuktapeDLL.duk_push_global_object(ctx);
            DuktapeDLL.duk_put_global_string(ctx, "window");

            {
                duk_begin_namespace(ctx, "console");
                DuktapeDLL.duk_push_c_function(ctx, DuktapeAux.duk_print_log, DuktapeDLL.DUK_VARARGS);
                DuktapeDLL.duk_put_prop_string(ctx, -2, "log");
                DuktapeDLL.duk_push_c_function(ctx, DuktapeAux.duk_print_log, DuktapeDLL.DUK_VARARGS);
                DuktapeDLL.duk_put_prop_string(ctx, -2, "info");
                DuktapeDLL.duk_push_c_function(ctx, DuktapeAux.duk_print_log, DuktapeDLL.DUK_VARARGS);
                DuktapeDLL.duk_put_prop_string(ctx, -2, "debug");
                DuktapeDLL.duk_push_c_function(ctx, DuktapeAux.duk_print_warn, DuktapeDLL.DUK_VARARGS);
                DuktapeDLL.duk_put_prop_string(ctx, -2, "warn");
                DuktapeDLL.duk_push_c_function(ctx, DuktapeAux.duk_print_err, DuktapeDLL.DUK_VARARGS);
                DuktapeDLL.duk_put_prop_string(ctx, -2, "error");
                DuktapeDLL.duk_push_c_function(ctx, DuktapeAux.duk_assert, DuktapeDLL.DUK_VARARGS);
                DuktapeDLL.duk_put_prop_string(ctx, -2, "assert");
                duk_end_namespace(ctx);
            }

            duk_begin_namespace(ctx, "DuktapeJS");
            {
                duk_begin_special(ctx, DuktapeVM.SPECIAL_ENUM);
                duk_add_method(ctx, "GetName", Enum_GetName, -3);
                duk_add_method(ctx, "Array", Array_Create, -3);
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

        private const uint DUK_UNITY_BUILTINS_VECTOR2 = 0;
        private const uint DUK_UNITY_BUILTINS_VECTOR2I = 1;
        private const uint DUK_UNITY_BUILTINS_VECTOR3 = 2;
        private const uint DUK_UNITY_BUILTINS_VECTOR3I = 3;
        private const uint DUK_UNITY_BUILTINS_VECTOR4 = 4;
        private const uint DUK_UNITY_BUILTINS_QUATERNION = 5;
        private const uint DUK_UNITY_BUILTINS_COLOR = 6;
        private const uint DUK_UNITY_BUILTINS_COLOR32 = 7;
        private const uint DUK_UNITY_BUILTINS_MATRIX33 = 8;
        private const uint DUK_UNITY_BUILTINS_MATRIX44 = 9;

        private static void replace_by_builtin(IntPtr ctx, string t, uint k)
        {
            DuktapeDLL.duk_builtins_reg_get(ctx, k);    // c
            DuktapeDLL.duk_get_prop_string(ctx, -2, t); // cs

            if (DuktapeDLL.duk_is_constructable(ctx, -1))
            {
                // copy static fields from c# to c
                DuktapeDLL.duk_enum(ctx, -1, 0);
                while (DuktapeDLL.duk_next(ctx, -1, true))
                {
                    DuktapeDLL.duk_dup(ctx, -2);
                    if (!DuktapeDLL.duk_has_prop(ctx, -6))
                    {
                        DuktapeDLL.duk_put_prop(ctx, -5);
                    }
                    else
                    {
                        DuktapeDLL.duk_pop_2(ctx); // pop key and value
                    }
                }
                DuktapeDLL.duk_pop(ctx); // pop enum

                DuktapeDLL.duk_get_prop_string(ctx, -2, "prototype"); // c  prototype
                DuktapeDLL.duk_get_prop_string(ctx, -2, "prototype"); // cs prototype   <stack> [c, cs, c.prototype, cs.prototype]

                DuktapeDLL.duk_set_prototype(ctx, -2);  // c.prototype = cs.prototype   <stack> [c, cs, c.prototype]
                DuktapeDLL.duk_pop(ctx); // pop c.prototype
                DuktapeDLL.duk_put_prop_string(ctx, -2, "_raw"); // cs._raw = c
                DuktapeDLL.duk_put_prop_string(ctx, -2, t); // <global>
            }
            else
            {
                // Debug.LogWarning($"builtin type {t} does not exist");
                DuktapeDLL.duk_pop_2(ctx);
            }
        }

        public static void postreg(IntPtr ctx)
        {
            duk_begin_namespace(ctx, "UnityEngine");
            replace_by_builtin(ctx, "Vector2", DUK_UNITY_BUILTINS_VECTOR2);
            replace_by_builtin(ctx, "Vector2Int", DUK_UNITY_BUILTINS_VECTOR2I);
            replace_by_builtin(ctx, "Vector3", DUK_UNITY_BUILTINS_VECTOR3);
            replace_by_builtin(ctx, "Vector3Int", DUK_UNITY_BUILTINS_VECTOR3I);
            replace_by_builtin(ctx, "Vector4", DUK_UNITY_BUILTINS_VECTOR4);
            replace_by_builtin(ctx, "Quaternion", DUK_UNITY_BUILTINS_QUATERNION);
            replace_by_builtin(ctx, "Color", DUK_UNITY_BUILTINS_COLOR);
            replace_by_builtin(ctx, "Color32", DUK_UNITY_BUILTINS_COLOR32);
            replace_by_builtin(ctx, "Matrix4x4", DUK_UNITY_BUILTINS_MATRIX44);
            duk_end_namespace(ctx);
        }
    }
}
