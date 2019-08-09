using System;

namespace Duktape
{
    using UnityEngine;

    public class DuktapeArray : DuktapeValue
    {
        public DuktapeArray(DuktapeContext context, uint refid)
        : base(context, refid)
        {
        }

        public DuktapeArray(IntPtr ctx, uint refid)
        : base(ctx, refid)
        {
        }

        public int length
        {
            get
            {
                var ctx = _context.rawValue;
                this.Push(ctx);
                var length_ = DuktapeDLL.duk_unity_get_length(ctx, -1);
                DuktapeDLL.duk_pop(ctx);
                return (int)length_;
            }
        }

        public void SetIntValue(int index, int value)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeDLL.duk_push_int(ctx, value);
            DuktapeDLL.duk_put_prop_index(ctx, -2, (uint)index);
            DuktapeDLL.duk_pop(ctx);
        }

        public void SetFloatValue(int index, float value)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeDLL.duk_push_number(ctx, value);
            DuktapeDLL.duk_put_prop_index(ctx, -2, (uint)index);
            DuktapeDLL.duk_pop(ctx);
        }

        public void SetDoubleValue(int index, double value)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeDLL.duk_push_number(ctx, value);
            DuktapeDLL.duk_put_prop_index(ctx, -2, (uint)index);
            DuktapeDLL.duk_pop(ctx);
        }

        public void SetStringValue(int index, string value)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeDLL.duk_push_string(ctx, value);
            DuktapeDLL.duk_put_prop_index(ctx, -2, (uint)index);
            DuktapeDLL.duk_pop(ctx);
        }

        //
        public int GetIntValue(int index)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeDLL.duk_get_prop_index(ctx, -1, (uint)index);
            var res = 0;
            if (DuktapeDLL.duk_is_number(ctx, -1))
            {
                res = DuktapeDLL.duk_get_int(ctx, -1);
            }
            DuktapeDLL.duk_pop_2(ctx);
            return res;
        }

        public float GetFloatValue(int index)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeDLL.duk_get_prop_index(ctx, -1, (uint)index);
            var res = 0f;
            if (DuktapeDLL.duk_is_number(ctx, -1))
            {
                res = (float)DuktapeDLL.duk_get_number(ctx, -1);
            }
            DuktapeDLL.duk_pop_2(ctx);
            return res;
        }

        public double GetDoubleValue(int index)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeDLL.duk_get_prop_index(ctx, -1, (uint)index);
            var res = 0.0;
            if (DuktapeDLL.duk_is_number(ctx, -1))
            {
                res = DuktapeDLL.duk_get_number(ctx, -1);
            }
            DuktapeDLL.duk_pop_2(ctx);
            return res;
        }

        public string GetStringValue(int index)
        {
            var ctx = _context.rawValue;
            this.Push(ctx);
            DuktapeDLL.duk_get_prop_index(ctx, -1, (uint)index);
            string res = null;
            if (DuktapeDLL.duk_is_string(ctx, -1))
            {
                res = DuktapeDLL.duk_get_string(ctx, -1);
            }
            DuktapeDLL.duk_pop_2(ctx);
            return res;
        }
    }
}
