using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public partial class DuktapeBinding
    {
        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int _AddComponent(IntPtr ctx)
        {
            try
            {
                UnityEngine.GameObject self;
                duk_get_this(ctx, out self);
                System.Type arg0 = null;
                if (DuktapeDLL.duk_get_prop_string(ctx, 0, DuktapeVM.OBJ_PROP_EXPORTED_REFID))
                {
                    var refid = DuktapeDLL.duk_get_uint(ctx, -1);
                    DuktapeDLL.duk_pop(ctx);
                    arg0 = DuktapeVM.GetVM(ctx).GetExportedType(refid);
                    if (arg0 == null)
                    {
                        // fallthrough
                        return DuktapeDLL.duk_generic_error(ctx, $"no such type");
                    }
                    var ret = self.AddComponent(arg0);
                    duk_push_any(ctx, ret);
                    return 1;
                }
                else
                {
                    var jsb = self.AddComponent<DuktapeBehaviour>();
                    DuktapeDLL.duk_dup(ctx, 0);
                    if (DuktapeDLL.duk_pnew(ctx, 0) != DuktapeDLL.DUK_EXEC_SUCCESS)
                    {
                        DuktapeAux.PrintError(ctx, -1);
                    }
                    DuktapeDLL.duk_dup(ctx, -1);
                    var refid = DuktapeDLL.duk_unity_ref(ctx);
                    var obj = new DuktapeObject(ctx, refid);
                    jsb.MakeBridge(obj);
                    var cache = DuktapeVM.GetObjectCache(ctx);
                    var nid = cache.AddObject(jsb);
                    DuktapeDLL.duk_unity_set_prop_i(ctx, -1, DuktapeVM.OBJ_PROP_NATIVE, nid);
                    return 1;
                }
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int _GetComponent(IntPtr ctx)
        {
            try
            {
                var argc = DuktapeDLL.duk_get_top(ctx);
                do
                {
                    if (argc == 1)
                    {
                        if (duk_match_types(ctx, argc, typeof(System.Type)))
                        {
                            UnityEngine.GameObject self;
                            duk_get_this(ctx, out self);
                            System.Type arg0;
                            duk_get_type(ctx, 0, out arg0);
                            var ret = self.GetComponent(arg0);
                            duk_push_any(ctx, ret);
                            return 1;
                        }
                        if (duk_match_types(ctx, argc, typeof(string)))
                        {
                            UnityEngine.GameObject self;
                            duk_get_this(ctx, out self);
                            string arg0;
                            duk_get_primitive(ctx, 0, out arg0);
                            var ret = self.GetComponent(arg0);
                            duk_push_any(ctx, ret);
                            return 1;
                        }
                        break;
                    }
                } while (false);
                return DuktapeDLL.duk_generic_error(ctx, "no matched method variant");
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int _GetComponentInChildren(IntPtr ctx)
        {
            try
            {
                var argc = DuktapeDLL.duk_get_top(ctx);
                do
                {
                    if (argc == 2)
                    {
                        UnityEngine.GameObject self;
                        duk_get_this(ctx, out self);
                        System.Type arg0;
                        duk_get_type(ctx, 0, out arg0);
                        bool arg1;
                        duk_get_primitive(ctx, 1, out arg1);
                        var ret = self.GetComponentInChildren(arg0, arg1);
                        duk_push_any(ctx, ret);
                        return 1;
                    }
                    if (argc == 1)
                    {
                        UnityEngine.GameObject self;
                        duk_get_this(ctx, out self);
                        System.Type arg0;
                        duk_get_type(ctx, 0, out arg0);
                        var ret = self.GetComponentInChildren(arg0);
                        duk_push_any(ctx, ret);
                        return 1;
                    }
                } while (false);
                return DuktapeDLL.duk_generic_error(ctx, "no matched method variant");
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int _GetComponentInParent(IntPtr ctx)
        {
            try
            {
                UnityEngine.GameObject self;
                duk_get_this(ctx, out self);
                System.Type arg0;
                duk_get_type(ctx, 0, out arg0);
                var ret = self.GetComponentInParent(arg0);
                duk_push_any(ctx, ret);
                return 1;
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int _GetComponents(IntPtr ctx)
        {
            try
            {
                var argc = DuktapeDLL.duk_get_top(ctx);
                do
                {
                    if (argc == 2)
                    {
                        UnityEngine.GameObject self;
                        duk_get_this(ctx, out self);
                        System.Type arg0;
                        duk_get_type(ctx, 0, out arg0);
                        System.Collections.Generic.List<UnityEngine.Component> arg1;
                        duk_get_classvalue(ctx, 1, out arg1);
                        self.GetComponents(arg0, arg1);
                        return 0;
                    }
                    if (argc == 1)
                    {
                        UnityEngine.GameObject self;
                        duk_get_this(ctx, out self);
                        System.Type arg0;
                        duk_get_type(ctx, 0, out arg0);
                        var ret = self.GetComponents(arg0);
                        duk_push_any(ctx, ret);
                        return 1;
                    }
                } while (false);
                return DuktapeDLL.duk_generic_error(ctx, "no matched method variant");
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int _GetComponentsInChildren(IntPtr ctx)
        {
            try
            {
                var argc = DuktapeDLL.duk_get_top(ctx);
                do
                {
                    if (argc == 2)
                    {
                        UnityEngine.GameObject self;
                        duk_get_this(ctx, out self);
                        System.Type arg0;
                        duk_get_type(ctx, 0, out arg0);
                        bool arg1;
                        duk_get_primitive(ctx, 1, out arg1);
                        var ret = self.GetComponentsInChildren(arg0, arg1);
                        duk_push_any(ctx, ret);
                        return 1;
                    }
                    if (argc == 1)
                    {
                        UnityEngine.GameObject self;
                        duk_get_this(ctx, out self);
                        System.Type arg0;
                        duk_get_type(ctx, 0, out arg0);
                        var ret = self.GetComponentsInChildren(arg0);
                        duk_push_any(ctx, ret);
                        return 1;
                    }
                } while (false);
                return DuktapeDLL.duk_generic_error(ctx, "no matched method variant");
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }

        [AOT.MonoPInvokeCallbackAttribute(typeof(DuktapeDLL.duk_c_function))]
        public static int _GetComponentsInParent(IntPtr ctx)
        {
            try
            {
                var argc = DuktapeDLL.duk_get_top(ctx);
                do
                {
                    if (argc == 2)
                    {
                        UnityEngine.GameObject self;
                        duk_get_this(ctx, out self);
                        System.Type arg0;
                        duk_get_type(ctx, 0, out arg0);
                        bool arg1;
                        duk_get_primitive(ctx, 1, out arg1);
                        var ret = self.GetComponentsInParent(arg0, arg1);
                        duk_push_any(ctx, ret);
                        return 1;
                    }
                    if (argc == 1)
                    {
                        UnityEngine.GameObject self;
                        duk_get_this(ctx, out self);
                        System.Type arg0;
                        duk_get_type(ctx, 0, out arg0);
                        var ret = self.GetComponentsInParent(arg0);
                        duk_push_any(ctx, ret);
                        return 1;
                    }
                } while (false);
                return DuktapeDLL.duk_generic_error(ctx, "no matched method variant");
            }
            catch (Exception exception)
            {
                return DuktapeDLL.duk_generic_error(ctx, exception.ToString());
            }
        }
    }
}