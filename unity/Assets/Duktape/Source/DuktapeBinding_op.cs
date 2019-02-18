using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public partial class DuktapeBinding
    {
        // variant push
        public static void duk_push_any(IntPtr ctx, object o)
        {
            var id = DuktapeVM.GetObjectCache(ctx).Add(o);
            DuktapeDLL.duk_push_int(ctx, id);
        }
        
        // variant push
        public static void duk_push_any(IntPtr ctx, UnityEngine.Object o)
        {
            var id = DuktapeVM.GetObjectCache(ctx).Add(o);
            DuktapeDLL.duk_push_int(ctx, id);
        }
        
        public static void duk_push_any(IntPtr ctx, bool o)
        {
            DuktapeDLL.duk_push_boolean(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, sbyte o)
        {
            DuktapeDLL.duk_push_int(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, byte o)
        {
            DuktapeDLL.duk_push_int(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, char o)
        {
            DuktapeDLL.duk_push_int(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, string o)
        {
            DuktapeDLL.duk_push_string(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, short o)
        {
            DuktapeDLL.duk_push_int(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, ushort o)
        {
            DuktapeDLL.duk_push_int(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, int o)
        {
            DuktapeDLL.duk_push_int(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, uint o)
        {
            DuktapeDLL.duk_push_uint(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, long o)
        {
            DuktapeDLL.duk_push_number(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, ulong o)
        {
            DuktapeDLL.duk_push_number(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, float o)
        {
            DuktapeDLL.duk_push_number(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, double o)
        {
            DuktapeDLL.duk_push_number(ctx, o);
        }

        public static void duk_push_any(IntPtr ctx, Color o)
        {
            DuktapeDLL.duk_unity_push4f(ctx, o.r, o.g, o.b, o.a);
        }

        public static void duk_push_any(IntPtr ctx, Color32 o)
        {
            DuktapeDLL.duk_unity_push4i(ctx, o.r, o.g, o.b, o.a);
        }

        public static void duk_push_any(IntPtr ctx, Vector2 o)
        {
            DuktapeDLL.duk_unity_push2f(ctx, o.x, o.y);
        }

        public static void duk_push_any(IntPtr ctx, Vector3 o)
        {
            DuktapeDLL.duk_unity_push3f(ctx, o.x, o.y, o.z);
        }

        public static void duk_push_any(IntPtr ctx, Vector4 o)
        {
            DuktapeDLL.duk_unity_push4f(ctx, o.x, o.y, o.z, o.w);
        }

        public static void duk_push_any(IntPtr ctx, Quaternion o)
        {
            DuktapeDLL.duk_unity_push4f(ctx, o.x, o.y, o.z, o.w);
        }


        public static bool duk_get_any(IntPtr ctx, int idx, out object o)
        {
            if (DuktapeDLL.duk_get_prop_string(ctx, -1, DuktapeVM.OBJ_PROP_NATIVE))
            {
                var id = DuktapeDLL.duk_get_int(ctx, -1);
                DuktapeDLL.duk_pop(ctx);
                return DuktapeVM.GetObjectCache(ctx).TryGetValue(id, out o);
            }
            else 
            {
                DuktapeDLL.duk_pop(ctx);
            }
            o = null;
            return false;
        }
    }
}
