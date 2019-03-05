#if UNITY_STANDALONE_WIN
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
                string[] arg1 = null;
                if (argc - 1 > 0)
                {
                    arg1 = new string[argc - 1];
                    for (var i = 1; i < argc; i++)
                    {
                        duk_get_primitive(ctx, i, out arg1[i - 1]);
                    }
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
        public static int Bind_TestDelegate1(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                self.TestDelegate1();
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind_TestVector3(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                UnityEngine.Vector3 arg0;
                duk_get_structvalue(ctx, 0, out arg0);
                self.TestVector3(arg0);
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind_TestType1(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                System.Type arg0;
                duk_get_type(ctx, 0, out arg0);
                var ret = self.TestType1(arg0);
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
                int[] arg0 = null;
                if (argc > 0)
                {
                    arg0 = new int[argc];
                    for (var i = 0; i < argc; i++)
                    {
                        duk_get_primitive(ctx, i, out arg0[i]);
                    }
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
                int[] arg1 = null;
                if (argc - 1 > 0)
                {
                    arg1 = new int[argc - 1];
                    for (var i = 1; i < argc; i++)
                    {
                        duk_get_primitive(ctx, i, out arg1[i - 1]);
                    }
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
        public static int Bind_MethodOverride(IntPtr ctx)
        {
            try
            {
                var argc = DuktapeDLL.duk_get_top(ctx);
                do
                {
                    if (argc == 2)
                    {
                        SampleClass self;
                        duk_get_this(ctx, out self);
                        float arg0;
                        duk_get_primitive(ctx, 0, out arg0);
                        float arg1;
                        duk_get_primitive(ctx, 1, out arg1);
                        self.MethodOverride(arg0, arg1);
                        return 0;
                    }
                    if (argc == 1)
                    {
                        if (duk_match_types(ctx, argc, typeof(int)))
                        {
                            SampleClass self;
                            duk_get_this(ctx, out self);
                            int arg0;
                            duk_get_primitive(ctx, 0, out arg0);
                            self.MethodOverride(arg0);
                            return 0;
                        }
                        if (duk_match_types(ctx, argc, typeof(string)))
                        {
                            SampleClass self;
                            duk_get_this(ctx, out self);
                            string arg0;
                            duk_get_primitive(ctx, 0, out arg0);
                            self.MethodOverride(arg0);
                            return 0;
                        }
                        break;
                    }
                    if (argc == 0)
                    {
                        SampleClass self;
                        duk_get_this(ctx, out self);
                        self.MethodOverride();
                        return 0;
                    }
                } while(false);
                return DuktapeDLL.duk_generic_error(ctx, "no matched method variant");
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind_MethodOverride2(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                int arg0;
                duk_get_primitive(ctx, 0, out arg0);
                self.MethodOverride2(arg0);
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind_MethodOverride2F(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                float arg0;
                duk_get_primitive(ctx, 0, out arg0);
                self.MethodOverride2(arg0);
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind_MethodOverride3(IntPtr ctx)
        {
            try
            {
                var argc = DuktapeDLL.duk_get_top(ctx);
                do
                {
                    if (argc == 4)
                    {
                        SampleClass self;
                        duk_get_this(ctx, out self);
                        float arg0;
                        duk_get_primitive(ctx, 0, out arg0);
                        float arg1;
                        duk_get_primitive(ctx, 1, out arg1);
                        float arg2;
                        duk_get_primitive(ctx, 2, out arg2);
                        object arg3;
                        duk_get_classvalue(ctx, 3, out arg3);
                        self.MethodOverride3(arg0, arg1, arg2, arg3);
                        return 0;
                    }
                    if (argc >= 3)
                    {
                        if (argc == 3)
                        {
                            SampleClass self;
                            duk_get_this(ctx, out self);
                            float arg0;
                            duk_get_primitive(ctx, 0, out arg0);
                            float arg1;
                            duk_get_primitive(ctx, 1, out arg1);
                            float arg2;
                            duk_get_primitive(ctx, 2, out arg2);
                            self.MethodOverride3(arg0, arg1, arg2);
                            return 0;
                        }
                        if (duk_match_types(ctx, argc, typeof(float), typeof(float), typeof(float), typeof(int[]))
                         && duk_match_param_types(ctx, 3, argc, typeof(int)))
                        {
                            SampleClass self;
                            duk_get_this(ctx, out self);
                            float arg0;
                            duk_get_primitive(ctx, 0, out arg0);
                            float arg1;
                            duk_get_primitive(ctx, 1, out arg1);
                            float arg2;
                            duk_get_primitive(ctx, 2, out arg2);
                            int[] arg3 = null;
                            if (argc - 3 > 0)
                            {
                                arg3 = new int[argc - 3];
                                for (var i = 3; i < argc; i++)
                                {
                                    duk_get_primitive(ctx, i, out arg3[i - 3]);
                                }
                            }
                            self.MethodOverride3(arg0, arg1, arg2, arg3);
                            return 0;
                        }
                    }
                    if (argc >= 2)
                    {
                        if (argc == 2)
                        {
                            SampleClass self;
                            duk_get_this(ctx, out self);
                            float arg0;
                            duk_get_primitive(ctx, 0, out arg0);
                            float arg1;
                            duk_get_primitive(ctx, 1, out arg1);
                            self.MethodOverride3(arg0, arg1);
                            return 0;
                        }
                        if (duk_match_types(ctx, argc, typeof(float), typeof(float), typeof(int[]))
                         && duk_match_param_types(ctx, 2, argc, typeof(int)))
                        {
                            SampleClass self;
                            duk_get_this(ctx, out self);
                            float arg0;
                            duk_get_primitive(ctx, 0, out arg0);
                            float arg1;
                            duk_get_primitive(ctx, 1, out arg1);
                            int[] arg2 = null;
                            if (argc - 2 > 0)
                            {
                                arg2 = new int[argc - 2];
                                for (var i = 2; i < argc; i++)
                                {
                                    duk_get_primitive(ctx, i, out arg2[i - 2]);
                                }
                            }
                            self.MethodOverride3(arg0, arg1, arg2);
                            return 0;
                        }
                    }
                    if (argc == 1)
                    {
                        SampleClass self;
                        duk_get_this(ctx, out self);
                        float arg0;
                        duk_get_primitive(ctx, 0, out arg0);
                        self.MethodOverride3(arg0);
                        return 0;
                    }
                } while(false);
                return DuktapeDLL.duk_generic_error(ctx, "no matched method variant");
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
                duk_push_any(ctx, (int)ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_delegateFoo1(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                var ret = self.delegateFoo1;
                duk_push_delegate(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindWrite_delegateFoo1(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                SampleClass.DelegateFoo value;
                duk_get_delegate(ctx, 0, out value);
                self.delegateFoo1 = value;
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_delegateFoo2(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                var ret = self.delegateFoo2;
                duk_push_delegate(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindWrite_delegateFoo2(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                SampleClass.DelegateFoo2 value;
                duk_get_delegate(ctx, 0, out value);
                self.delegateFoo2 = value;
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_delegateFoo4(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                var ret = self.delegateFoo4;
                duk_push_delegate(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindWrite_delegateFoo4(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                SampleClass.DelegateFoo4 value;
                duk_get_delegate(ctx, 0, out value);
                self.delegateFoo4 = value;
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_action1(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                var ret = self.action1;
                duk_push_delegate(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindWrite_action1(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                System.Action value;
                duk_get_delegate(ctx, 0, out value);
                self.action1 = value;
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_action2(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                var ret = self.action2;
                duk_push_delegate(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindWrite_action2(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                System.Action<string> value;
                duk_get_delegate(ctx, 0, out value);
                self.action2 = value;
                return 0;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
        [UnityEngine.Scripting.Preserve]
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int BindRead_actions1(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                var ret = self.actions1;
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
        public static int BindWrite_actions1(IntPtr ctx)
        {
            try
            {
                SampleClass self;
                duk_get_this(ctx, out self);
                System.Action[] value;
                duk_get_delegate_array(ctx, 0, out value);
                self.actions1 = value;
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
            duk_begin_class(ctx, "SampleClass", typeof(SampleClass), BindConstructor);
            duk_add_method(ctx, "TestDelegate1", Bind_TestDelegate1, -1);
            duk_add_method(ctx, "TestVector3", Bind_TestVector3, -1);
            duk_add_method(ctx, "TestType1", Bind_TestType1, -1);
            duk_add_method(ctx, "SetEnum", Bind_SetEnum, -1);
            duk_add_method(ctx, "CheckingVA", Bind_CheckingVA, -1);
            duk_add_method(ctx, "CheckingVA2", Bind_CheckingVA2, -1);
            duk_add_method(ctx, "MethodOverride", Bind_MethodOverride, -1);
            duk_add_method(ctx, "MethodOverride2", Bind_MethodOverride2, -1);
            duk_add_method(ctx, "MethodOverride2F", Bind_MethodOverride2F, -1);
            duk_add_method(ctx, "MethodOverride3", Bind_MethodOverride3, -1);
            duk_add_method(ctx, "Sum", Bind_Sum, -1);
            duk_add_property(ctx, "name", BindRead_name, null, -1);
            duk_add_property(ctx, "sampleEnum", BindRead_sampleEnum, null, -1);
            duk_add_field(ctx, "delegateFoo1", BindRead_delegateFoo1, BindWrite_delegateFoo1, -1);
            duk_add_field(ctx, "delegateFoo2", BindRead_delegateFoo2, BindWrite_delegateFoo2, -1);
            duk_add_field(ctx, "delegateFoo4", BindRead_delegateFoo4, BindWrite_delegateFoo4, -1);
            duk_add_field(ctx, "action1", BindRead_action1, BindWrite_action1, -1);
            duk_add_field(ctx, "action2", BindRead_action2, BindWrite_action2, -1);
            duk_add_field(ctx, "actions1", BindRead_actions1, BindWrite_actions1, -1);
            duk_end_class(ctx);
            duk_end_namespace(ctx);
            return 0;
        }
    }
}
#endif
