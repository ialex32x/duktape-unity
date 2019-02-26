#if UNITY_STANDALONE_OSX
// UserName: huliangjie @ 2/26/2019 9:23:05 AM
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Type: SampleClass
using System;
using System.Collections.Generic;

namespace DuktapeJS {
    using Duktape;
    [JSBindingAttribute(65537)]
    [UnityEngine.Scripting.Preserve]
    public class DuktapeJS_SampleClass : DuktapeBinding {
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindConstructor(IntPtr ctx)
        {
            try
            {
                var argc = DuktapeDLL.duk_get_top(ctx);
                string arg0;
                duk_get_primitive(ctx, 0, out arg0);
                string[] arg1 = new string[argc - 1];
                for (var i = 1; i < argc; i++)
                {
                    duk_get_primitive(ctx, i, out arg1[i - 1]);
                }
                var o = new SampleClass(arg0, arg1);
                DuktapeDLL.duk_push_this(ctx);
                duk_bind_native(ctx, -1, o);
                DuktapeDLL.duk_pop(ctx);
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind_SetEnum(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                SampleEnum arg0;
                duk_get_enumvalue(ctx, 0, out arg0);
                var ret = self.SetEnum(arg0);
                duk_push_any(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind_CheckingVA(IntPtr ctx)
        {
            try
            {
                var argc = DuktapeDLL.duk_get_top(ctx);
                SampleClass self;
                duk_get_this(ctx, out self);
                int[] arg0 = new int[argc];
                for (var i = 0; i < argc; i++)
                {
                    duk_get_primitive(ctx, i, out arg0[i]);
                }
                var ret = self.CheckingVA(arg0);
                duk_push_any(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind_CheckingVA2(IntPtr ctx)
        {
            try
            {
                var argc = DuktapeDLL.duk_get_top(ctx);
                SampleClass self;
                duk_get_this(ctx, out self);
                int arg0;
                duk_get_primitive(ctx, 0, out arg0);
                int[] arg1 = new int[argc - 1];
                for (var i = 1; i < argc; i++)
                {
                    duk_get_primitive(ctx, i, out arg1[i - 1]);
                }
                var ret = self.CheckingVA2(arg0, arg1);
                duk_push_any(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind_Sum(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                int[] arg0;
                duk_get_primitive_array(ctx, 0, out arg0);
                var ret = self.Sum(arg0);
                duk_push_any(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_name(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                var ret = self.name;
                duk_push_any(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_sampleEnum(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                var ret = self.sampleEnum;
                duk_push_any(ctx, (System.Int32)ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        public static int reg(IntPtr ctx)
        {
            duk_begin_namespace(ctx);
            duk_begin_class(ctx, "SampleClass", typeof(SampleClass), BindConstructor);
            duk_add_method(ctx, "SetEnum", Bind_SetEnum, false);
            duk_add_method(ctx, "CheckingVA", Bind_CheckingVA, false);
            duk_add_method(ctx, "CheckingVA2", Bind_CheckingVA2, false);
            duk_add_method(ctx, "Sum", Bind_Sum, false);
            duk_add_property(ctx, "name", BindRead_name, null, false);
            duk_add_property(ctx, "sampleEnum", BindRead_sampleEnum, null, false);
            duk_end_class(ctx);
            duk_end_namespace(ctx);
            return 0;
        }
    }
}
#endif
