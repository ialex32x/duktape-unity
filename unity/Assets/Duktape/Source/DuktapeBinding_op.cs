using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

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
            var id = cache.Add(o);
            DuktapeDLL.duk_unity_set_prop_i(ctx, idx, DuktapeVM.OBJ_PROP_NATIVE, id);
            if (!o.GetType().IsValueType)
            {
                var heapptr = DuktapeDLL.duk_get_heapptr(ctx, idx);
                cache.AddJSValue(o, heapptr);
            }
        }
    }
}
