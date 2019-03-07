using System;

namespace Duktape
{
    using UnityEngine;

    /// 持有脚本对象的引用
    public class DuktapeValue : IDisposable, IContextualValue
    {
        protected IntPtr _ctx;
        protected uint _refid;

        public bool isValid { get { return _refid > 0; } }

        public uint rawValue { get { return _refid; } }

        public IntPtr ctx
        {
            get
            {
                return _ctx;
            }
        }

        public DuktapeValue(IntPtr ctx, uint refid)
        {
            this._ctx = ctx;
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

        private static void duk_unity_unref(IntPtr ctx, uint refid, object target)
        {
            DuktapeDLL.duk_unity_unref(ctx, refid);
        }

        protected virtual void Dispose(bool bManaged)
        {
            if (this._refid != 0 && this._ctx != null)
            {
                var vm = DuktapeContext.GetVM(this._ctx);
                vm.GC(this._refid, null, duk_unity_unref);
                this._refid = 0;
            }
        }

        public void Push(IntPtr ctx)
        {
            DuktapeDLL.duk_unity_getref(ctx, this._refid);
        }

        public void PushProperty(IntPtr ctx, string property)
        {
            this.Push(ctx); // push this
            DuktapeDLL.duk_get_prop_string(ctx, -1, property);
            DuktapeDLL.duk_remove(ctx, -2); // remove this
        }

        public void PushProperty(IntPtr ctx, uint index)
        {
            this.Push(ctx); // push this
            DuktapeDLL.duk_get_prop_index(ctx, -1, index);
            DuktapeDLL.duk_remove(ctx, -2); // remove this
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
            var ctx = x._ctx;
            x.Push(ctx);
            y.Push(ctx);
            var eq = DuktapeDLL.duk_equals(ctx, -1, -2);
            DuktapeDLL.duk_pop_2(ctx);
            return eq;
        }
    }
}
