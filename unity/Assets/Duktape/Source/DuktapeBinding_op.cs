using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public partial class DuktapeBinding
    {
        // 返回当前 this 对应的 native object
        public static object duk_get_this(IntPtr ctx)
        {
            DuktapeDLL.duk_push_this(ctx);
            DuktapeDLL.duk_get_prop_string(ctx, -1, DuktapeVM.OBJ_PROP_NATIVE);
            var id = DuktapeDLL.duk_get_int(ctx, -1);
            DuktapeDLL.duk_pop_2(ctx); // pop [this, object-id]
            object o;
            return DuktapeVM.GetObjectCache(ctx).TryGetValue(id, out o) ? o : null;
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
