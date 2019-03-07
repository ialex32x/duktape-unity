using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;

    // 引用一个 js 对象, 提供比 DuktapeValue 更丰富的交互接口
    public class DuktapeObject : DuktapeValue
    {
        // 方法缓存, 首次访问时初始化
        private Dictionary<string, DuktapeFunction> _methodCache = new Dictionary<string, DuktapeFunction>();

        public DuktapeObject(IntPtr ctx, uint refid)
        : base(ctx, refid)
        {
        }

        public void PushPrototype(IntPtr ctx)
        {
            DuktapeDLL.duk_unity_getref(ctx, this._refid);
            DuktapeDLL.duk_get_prototype(ctx, -1);
            DuktapeDLL.duk_remove(ctx, -2);
        }

        public DuktapeFunction GetMember(string name)
        {
            DuktapeFunction method = null;
            if (!_methodCache.TryGetValue(name, out method))
            {
                this.PushProperty(ctx, name);
                if (DuktapeDLL.duk_is_function(ctx, -1))
                {
                    var refid = DuktapeDLL.duk_unity_ref(ctx);
                    method = new DuktapeFunction(ctx, refid);
                }
                else
                {
                    DuktapeDLL.duk_pop(ctx);
                }
                _methodCache[name] = method;
            }
            return method;
        }

        public void InvokeMember(string name)
        {
            var member = GetMember(name);
            if (member != null)
            {
                member.Invoke();
            }
        }

        protected override void Dispose(bool bManaged)
        {
            _methodCache.Clear();
            _methodCache = null;
            base.Dispose(bManaged);
        }
    }
}
