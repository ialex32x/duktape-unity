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

        public DuktapeObject(IntPtr ctx, uint refid, IntPtr heapPtr)
        : base(ctx, refid, heapPtr)
        {
        }

        public void PushPrototype(IntPtr ctx)
        {
            if (_refPtr != IntPtr.Zero)
            {
                DuktapeDLL.duk_push_heapptr(ctx, _refPtr);
            } 
            else
            {
                DuktapeDLL.duk_unity_getref(ctx, this._refid);
            }
            DuktapeDLL.duk_get_prototype(ctx, -1);
            DuktapeDLL.duk_remove(ctx, -2);
        }

        public DuktapeFunction GetMember(string name)
        {
            DuktapeFunction method = null;
            if (!_methodCache.TryGetValue(name, out method))
            {
                var ctx = _context.rawValue;
                if (ctx != IntPtr.Zero)
                {
                    this.PushProperty(ctx, name);
                    if (DuktapeDLL.duk_is_function(ctx, -1))
                    {
                        var ptr = DuktapeDLL.duk_get_heapptr(ctx, -1);
                        var refid = DuktapeDLL.duk_unity_ref(ctx);
                        method = new DuktapeFunction(ctx, refid, ptr);
                    }
                    else
                    {
                        DuktapeDLL.duk_pop(ctx);
                    }
                }
                _methodCache[name] = method;
            }
            return method;
        }

        public object GetProperty(string name)
        {
            var ctx = _context.rawValue;
            if (ctx != IntPtr.Zero)
            {
                PushProperty(ctx, name);
                object o;
                DuktapeBinding.duk_get_var(ctx, -1, out o);
                DuktapeDLL.duk_pop(ctx);
                return o;
            }

            return null;
        }

        public bool InvokeMemberWithBooleanReturn(string name)
        {
            var member = GetMember(name);
            if (member != null)
            {
                var ctx = member.context.rawValue;
                if (ctx != IntPtr.Zero)
                {
                    member.Push(ctx);
                    this.Push(ctx);
                    var ret = DuktapeDLL.duk_pcall_method(ctx, 0);
                    if (ret != DuktapeDLL.DUK_EXEC_SUCCESS)
                    {
                        DuktapeAux.PrintError(ctx, -1);
                        // throw new Exception(err); 
                    }

                    var o = DuktapeDLL.duk_get_boolean_default(ctx, -1, false);
                    DuktapeDLL.duk_pop(ctx);
                    return o;
                }
            }

            return false;
        }

        public void InvokeMember(string name)
        {
            var member = GetMember(name);
            if (member != null)
            {
                var ctx = member.context.rawValue;
                if (ctx != IntPtr.Zero)
                {
                    member.Push(ctx);
                    this.Push(ctx);
                    var ret = DuktapeDLL.duk_pcall_method(ctx, 0);
                    if (ret != DuktapeDLL.DUK_EXEC_SUCCESS)
                    {
                        DuktapeAux.PrintError(ctx, -1);
                        // throw new Exception(err); 
                    }
                    DuktapeDLL.duk_pop(ctx);
                }
            }
        }

        public void InvokeMember(string name, float arg0)
        {
            var member = GetMember(name);
            if (member != null)
            {
                var ctx = member.context.rawValue;
                if (ctx != IntPtr.Zero)
                {
                    member.Push(ctx);
                    this.Push(ctx);
                    DuktapeDLL.duk_push_number(ctx, arg0);
                    var ret = DuktapeDLL.duk_pcall_method(ctx, 1);
                    if (ret != DuktapeDLL.DUK_EXEC_SUCCESS)
                    {
                        DuktapeAux.PrintError(ctx, -1);
                        // throw new Exception(err); 
                    }
                    DuktapeDLL.duk_pop(ctx);
                }
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
