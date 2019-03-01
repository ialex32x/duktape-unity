using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    // 处理委托的绑定
    public partial class DuktapeBinding
    {
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
            //TODO: 封装委托处理
            if (DuktapeDLL.duk_is_function(ctx, idx))
            {
                // 默认赋值操作
                DuktapeDLL.duk_dup(ctx, idx);
                var fn = new DuktapeDelegate(ctx, DuktapeDLL.duk_unity_ref(ctx));
                var vm = DuktapeVM.GetVM(ctx);
                o = vm.CreateDelegate(typeof(T), fn) as T;
                return true;
            }
            else
            {
                //
            }
            o = null;
            return false;
        }
    }
}
