using System;
using UnityEngine;

namespace Duktape
{
    /// 持有脚本对象的引用
    public abstract class DuktapeValue : IDisposable
    {
        protected DuktapeContext _ctx;
        protected int _refid;

        public DuktapeValue(IntPtr ctx, int refid)
        {
            this._ctx = DuktapeContext.GetContext(ctx);
            this._refid = refid;
        }

        ~DuktapeValue()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool bManaged)
        {
            if (this._refid != 0)
            {
                this._ctx.GC(this._refid);
                this._refid = 0;
            }
        }

        public void Push(IntPtr ctx)
        {
            DuktapeAux.duk_push_ref(ctx, this._refid);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is DuktapeValue ? this == (DuktapeValue)obj : false;
        }

        public static bool operator ==(DuktapeValue x, DuktapeValue y)
        {
            if ((object)x == null || (object)y == null)
            {
                return (object)x == (object)y;
            }
            return Equals(x, y);
        }

        public static bool operator !=(DuktapeValue x, DuktapeValue y)
        {
            if ((object)x == null || (object)y == null)
            {
                return (object)x != (object)y;
            }
            return !Equals(x, y);
        }

        private static bool Equals(DuktapeValue x, DuktapeValue y)
        {
            var ctx = x._ctx.rawValue;
            x.Push(ctx);
            y.Push(ctx);
            var eq = DuktapeDLL.duk_equals(ctx, -1, -2);
            DuktapeDLL.duk_pop_2(ctx);
            return eq;
        }
    }
}
