using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    // 处理委托的绑定
    public partial class DuktapeBinding
    {
        public static bool duk_get_delegate<T>(IntPtr ctx, int idx, out T o)
        where T : class
        {
            //TODO: 封装委托处理
            o = null;
            return false;
        }
    }
}
