using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    // 处理特殊操作, 关联本地对象等
    public partial class DuktapeBinding
    {
        // 返回当前 this 对应的 native object
        public static bool duk_get_this<T>(IntPtr ctx, out T self)
        {
            DuktapeDLL.duk_push_this(ctx);
            object o_t;
            var ret = duk_get_object(ctx, -1, out o_t);
            self = (T)o_t;
            DuktapeDLL.duk_pop(ctx); // pop this 
            return ret;
        }

        public static void duk_bind_native(IntPtr ctx, int idx, object o)
        {
            var cache = DuktapeVM.GetObjectCache(ctx);
            var id = cache.AddObject(o);
            DuktapeDLL.duk_unity_set_prop_i(ctx, idx, DuktapeVM.OBJ_PROP_NATIVE, id);
            if (!o.GetType().IsValueType)
            {
                var heapptr = DuktapeDLL.duk_get_heapptr(ctx, idx);
                cache.AddJSValue(o, heapptr);
            }
        }

        public static bool duk_rebind_this(IntPtr ctx, object o)
        {
            DuktapeDLL.duk_push_this(ctx);
            var ret = duk_rebind_native(ctx, -1, o);
            DuktapeDLL.duk_pop(ctx);
            return ret;
        }

        public static bool duk_get_native_refid(IntPtr ctx, int idx, out int id)
        {
            if (!DuktapeDLL.duk_is_null_or_undefined(ctx, idx))
            {
                if (DuktapeDLL.duk_get_prop_string(ctx, idx, DuktapeVM.OBJ_PROP_NATIVE))
                {
                    id = DuktapeDLL.duk_get_int(ctx, -1);
                    return true;
                }
                DuktapeDLL.duk_pop(ctx); // pop OBJ_PROP_NATIVE
            }
            id = 0;
            return false;
        }

        public static bool duk_rebind_native(IntPtr ctx, int idx, object o)
        {
            if (DuktapeDLL.duk_is_null_or_undefined(ctx, idx)) // or check for object?
            {
                return true;
            }
            if (DuktapeDLL.duk_get_prop_string(ctx, idx, DuktapeVM.OBJ_PROP_NATIVE))
            {
                var id = DuktapeDLL.duk_get_int(ctx, -1);
                DuktapeDLL.duk_pop(ctx); // pop OBJ_PROP_NATIVE
                return DuktapeVM.GetObjectCache(ctx).ReplaceObject(id, o);
            }
            else
            {
                DuktapeDLL.duk_pop(ctx);
            }
            return false;
        }
    }
}
