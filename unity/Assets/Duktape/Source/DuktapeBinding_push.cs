using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public partial class DuktapeBinding
    {
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

        public static void duk_push_any(IntPtr ctx, LayerMask o)
        {
            DuktapeDLL.duk_push_int(ctx, (int)o);
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

        public static void duk_push_any(IntPtr ctx, Vector2Int o)
        {
            DuktapeDLL.duk_unity_push2i(ctx, o.x, o.y);
        }

        public static void duk_push_any(IntPtr ctx, Vector3 o)
        {
            DuktapeDLL.duk_unity_push3f(ctx, o.x, o.y, o.z);
        }

        public static void duk_push_any(IntPtr ctx, Vector3Int o)
        {
            DuktapeDLL.duk_unity_push3i(ctx, o.x, o.y, o.z);
        }

        public static void duk_push_any(IntPtr ctx, Vector4 o)
        {
            DuktapeDLL.duk_unity_push4f(ctx, o.x, o.y, o.z, o.w);
        }

        public static void duk_push_any(IntPtr ctx, Quaternion o)
        {
            DuktapeDLL.duk_unity_push4f(ctx, o.x, o.y, o.z, o.w);
        }

        // variant push
        public static void duk_push_any(IntPtr ctx, UnityEngine.Object o)
        {
            if (o == null)
            {
                DuktapeDLL.duk_push_null(ctx);
                return;
            }
            duk_push_object(ctx, (object)o);
        }

        public static void duk_push_any(IntPtr ctx, Array o)
        {
            duk_push_any(ctx, (object)o);
        }

        public static void duk_push_delegate(IntPtr ctx, Delegate @delegate)
        {

        }

        // variant push
        public static void duk_push_any(IntPtr ctx, object o)
        {
            if (o == null)
            {
                DuktapeDLL.duk_push_null(ctx);
                return;
            }
            var type = o.GetType();
            if (type.IsEnum)
            {
                duk_push_any(ctx, Convert.ToInt32(o));
                return;
            }
            if (type.IsArray)
            {
                duk_push_any(ctx, (Array)o);
                return;
            }
            if (type.BaseType == typeof(MulticastDelegate))
            {
                duk_push_delegate(ctx, (Delegate)o);
                return;
            }
            duk_push_object(ctx, (object)o);
        }

        // push 一个对象实例 （调用者需要自行负责提前null检查） 
        private static void duk_push_object(IntPtr ctx, object o)
        {
            var cache = DuktapeVM.GetObjectCache(ctx);
            IntPtr heapptr;
            if (cache.TryGetJSValue(o, out heapptr))
            {
                DuktapeDLL.duk_push_heapptr(ctx, heapptr);
                return;
            }
            var id = cache.Add(o);
            DuktapeDLL.duk_push_object(ctx);
            DuktapeDLL.duk_unity_set_prop_i(ctx, -1, DuktapeVM.OBJ_PROP_NATIVE, id);
            if (DuktapeVM.GetVM(ctx).PushChainedPrototypeOf(ctx, o.GetType()))
            {
                DuktapeDLL.duk_set_prototype(ctx, -2);
            }
            if (!o.GetType().IsValueType)
            {
                heapptr = DuktapeDLL.duk_get_heapptr(ctx, -1);
                cache.AddJSValue(o, heapptr);
            }
            DuktapeDLL.duk_pop(ctx);
        }

        // 自动判断类型
        public static void duk_push_var(IntPtr ctx, object o)
        {
            if (o == null)
            {
                DuktapeDLL.duk_push_null(ctx);
                return;
            }
            var type = o.GetType();
            if (type.IsEnum)
            {
                duk_push_any(ctx, Convert.ToInt32(o));
                return;
            }
            //TODO: 1. push as simple types
            //TODO: 2. fallthrough, push as object
            duk_push_any(ctx, o);
        }
    }
}
