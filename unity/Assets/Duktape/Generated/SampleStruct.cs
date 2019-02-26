#if UNITY_STANDALONE_WIN
// UserName: julio @ 2019/2/27 6:17:53
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// Type: SampleStruct
using System;
using System.Collections.Generic;

namespace DuktapeJS {
    using Duktape;
    [JSBindingAttribute(65537)]
    [UnityEngine.Scripting.Preserve]
    public class DuktapeJS_SampleStruct : DuktapeBinding {
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindConstructor(IntPtr ctx)
        {
            try
            {
                var o = new SampleStruct();
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
        public static int Bind_ChangeFieldA(IntPtr ctx)
        {
            try
            {
                SampleStruct self;
                duk_get_this(ctx, out self);
                int arg0;
                duk_get_primitive(ctx, 0, out arg0);
                self.ChangeFieldA(arg0);
                duk_rebind_this(ctx, self);
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindStatic_StaticMethodWithReturnAndNoOverride(IntPtr ctx)
        {
            try
            {
                UnityEngine.Vector3 arg0;
                duk_get_structvalue(ctx, 0, out arg0);
                float arg1;
                duk_get_primitive(ctx, 1, out arg1);
                string[] arg2;
                duk_get_primitive_array(ctx, 2, out arg2);
                var ret = SampleStruct.StaticMethodWithReturnAndNoOverride(arg0, ref arg1, out arg2);
                DuktapeDLL.duk_push_object(ctx);
                // fill object properties here;
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_readonly_property_c(IntPtr ctx)
        {
            try
            {
                SampleStruct self;
                duk_get_this(ctx, out self);
                var ret = self.readonly_property_c;
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
        public static int BindRead_readwrite_property_d(IntPtr ctx)
        {
            try
            {
                SampleStruct self;
                duk_get_this(ctx, out self);
                var ret = self.readwrite_property_d;
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
        public static int BindWrite_readwrite_property_d(IntPtr ctx)
        {
            try
            {
                SampleStruct self;
                duk_get_this(ctx, out self);
                float value;
                duk_get_primitive(ctx, 0, out value);
                self.readwrite_property_d = value;
                duk_rebind_this(ctx, self);
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_static_readwrite_property_d(IntPtr ctx)
        {
            try
            {
                var ret = SampleStruct.static_readwrite_property_d;
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
        public static int BindWrite_static_readwrite_property_d(IntPtr ctx)
        {
            try
            {
                double value;
                duk_get_primitive(ctx, 0, out value);
                SampleStruct.static_readwrite_property_d = value;
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_field_a(IntPtr ctx)
        {
            try
            {
                SampleStruct self;
                duk_get_this(ctx, out self);
                var ret = self.field_a;
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
        public static int BindWrite_field_a(IntPtr ctx)
        {
            try
            {
                SampleStruct self;
                duk_get_this(ctx, out self);
                int value;
                duk_get_primitive(ctx, 0, out value);
                self.field_a = value;
                duk_rebind_this(ctx, self);
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindStaticRead_static_field_b(IntPtr ctx)
        {
            try
            {
                var ret = SampleStruct.static_field_b;
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
        public static int BindStaticWrite_static_field_b(IntPtr ctx)
        {
            try
            {
                string value;
                duk_get_primitive(ctx, 0, out value);
                SampleStruct.static_field_b = value;
                return 0;
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
            duk_begin_class(ctx, "SampleStruct", typeof(SampleStruct), BindConstructor);
            duk_add_method(ctx, "ChangeFieldA", Bind_ChangeFieldA, false);
            duk_add_method(ctx, "StaticMethodWithReturnAndNoOverride", BindStatic_StaticMethodWithReturnAndNoOverride, true);
            duk_add_property(ctx, "readonly_property_c", BindRead_readonly_property_c, null, false);
            duk_add_property(ctx, "readwrite_property_d", BindRead_readwrite_property_d, BindWrite_readwrite_property_d, false);
            duk_add_property(ctx, "static_readwrite_property_d", BindRead_static_readwrite_property_d, BindWrite_static_readwrite_property_d, false);
            duk_add_field(ctx, "field_a", BindRead_field_a, BindWrite_field_a, false);
            duk_add_field(ctx, "static_field_b", BindStaticRead_static_field_b, BindStaticWrite_static_field_b, true);
            duk_end_class(ctx);
            duk_end_namespace(ctx);
            return 0;
        }
    }
}
#endif
