using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    // 处理委托的绑定
    public partial class DuktapeBinding
    {
        //TODO: 尝试还原 js function/dispatcher
        public static void duk_push_delegate(IntPtr ctx, Delegate o)
        {
            //TODO: delegate push
            duk_push_object(ctx, (object)o);
        }

        public static bool duk_get_delegate_array<T>(IntPtr ctx, int idx, out T[] o)
        where T : class
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                var nidx = DuktapeDLL.duk_normalize_index(ctx, idx);
                o = new T[length];
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    T e;
                    duk_get_delegate(ctx, -1, out e);
                    o[i] = e;
                    DuktapeDLL.duk_pop(ctx);
                }
                return true;
            }
            duk_get_classvalue<T[]>(ctx, idx, out o);
            return true;
        }

        public static bool duk_get_delegate<T>(IntPtr ctx, int idx, out T o)
        where T : class
        {
            if (DuktapeDLL.duk_is_object(ctx, idx)/* && check if js delegate type (hidden property) */
             || DuktapeDLL.duk_is_function(ctx, idx))
            {
                DuktapeDLL.duk_get_prop_string(ctx, idx, DuktapeVM.OBJ_PROP_NATIVE_WEAK);
                var refid = DuktapeDLL.duk_get_int(ctx, -1);
                DuktapeDLL.duk_pop(ctx);

                var cache = DuktapeVM.GetObjectCache(ctx);
                DuktapeDelegate fn;
                if (cache.TryGetTypedWeakObject(refid, out fn) && fn != null)
                {
                    o = fn.target as T;
                    return true;
                }
                // 默认赋值操作
                DuktapeDLL.duk_dup(ctx, idx);
                var heapptr = DuktapeDLL.duk_get_heapptr(ctx, idx);
                fn = new DuktapeDelegate(ctx, DuktapeDLL.duk_unity_ref(ctx));
                var vm = DuktapeVM.GetVM(ctx);
                o = vm.CreateDelegate(typeof(T), fn) as T;

                // DuktapeDelegate 拥有 js 对象的强引用, 此 js 对象无法释放 cache 中的 object, 所以这里用弱引用注册
                // 会出现的问题是, 如果 c# 没有对 DuktapeDelegate 的强引用, 那么反复 get_delegate 会重复创建 DuktapeDelegate
                refid = cache.AddWeakObject(fn);
                DuktapeDLL.duk_unity_set_prop_i(ctx, idx, DuktapeVM.OBJ_PROP_NATIVE_WEAK, refid);
                cache.AddJSValue(o, heapptr);
                return true;
            }
            o = null;
            return false;
        }
    }
}
