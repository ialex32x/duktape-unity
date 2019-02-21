using System;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public partial class DuktapeBinding
    {
        public static bool duk_get_primitive(IntPtr ctx, int idx, out bool o)
        {
            o = DuktapeDLL.duk_get_boolean(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out sbyte o)
        {
            o = (sbyte)DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out byte o)
        {
            o = (byte)DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out char o)
        {
            o = (char)DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out string o)
        {
            o = DuktapeAux.duk_get_string(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out short o)
        {
            o = (short)DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out ushort o)
        {
            o = (ushort)DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out int o)
        {
            o = DuktapeDLL.duk_get_int(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out uint o)
        {
            o = DuktapeDLL.duk_get_uint(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out long o)
        {
            o = (long)DuktapeDLL.duk_get_number(ctx, idx); // no check, dangerous
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out ulong o)
        {
            o = (ulong)DuktapeDLL.duk_get_number(ctx, idx); // no check, dangerous
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out float o)
        {
            o = (float)DuktapeDLL.duk_get_number(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_primitive(IntPtr ctx, int idx, out double o)
        {
            o = DuktapeDLL.duk_get_number(ctx, idx); // no check
            return true;
        }

        public static bool duk_get_struct_object(IntPtr ctx, int idx, out LayerMask o)
        {
            o = (LayerMask)DuktapeDLL.duk_get_int(ctx, idx);
            return true;
        }

        public static bool duk_get_struct_object(IntPtr ctx, int idx, out Color o)
        {
            float r, g, b, a;
            var ret = DuktapeDLL.duk_unity_get4f(ctx, idx, out r, out g, out b, out a);
            o = new Color(r, g, b, a);
            return ret;
        }

        public static bool duk_get_struct_object(IntPtr ctx, int idx, out Color32 o)
        {
            int r, g, b, a;
            var ret = DuktapeDLL.duk_unity_get4i(ctx, idx, out r, out g, out b, out a);
            o = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
            return ret;
        }

        public static bool duk_get_struct_object(IntPtr ctx, int idx, out Vector2 o)
        {
            float x, y;
            var ret = DuktapeDLL.duk_unity_get2f(ctx, idx, out x, out y);
            o = new Vector2(x, y);
            return ret;
        }

        public static bool duk_get_struct_object(IntPtr ctx, int idx, out Vector2Int o)
        {
            int x, y;
            var ret = DuktapeDLL.duk_unity_get2i(ctx, idx, out x, out y);
            o = new Vector2Int(x, y);
            return ret;
        }

        public static bool duk_get_struct_object(IntPtr ctx, int idx, out Vector3 o)
        {
            float x, y, z;
            var ret = DuktapeDLL.duk_unity_get3f(ctx, idx, out x, out y, out z);
            o = new Vector3(x, y, z);
            return ret;
        }

        public static bool duk_get_struct_object(IntPtr ctx, int idx, out Vector3Int o)
        {
            int x, y, z;
            var ret = DuktapeDLL.duk_unity_get3i(ctx, idx, out x, out y, out z);
            o = new Vector3Int(x, y, z);
            return ret;
        }

        public static bool duk_get_struct_object(IntPtr ctx, int idx, out Vector4 o)
        {
            float x, y, z, w;
            var ret = DuktapeDLL.duk_unity_get4f(ctx, idx, out x, out y, out z, out w);
            o = new Vector4(x, y, z, w);
            return ret;
        }

        public static bool duk_get_struct_object(IntPtr ctx, int idx, out Quaternion o)
        {
            float x, y, z, w;
            var ret = DuktapeDLL.duk_unity_get4f(ctx, idx, out x, out y, out z, out w);
            o = new Quaternion(x, y, z, w);
            return ret;
        }

        // fallthrough
        public static bool duk_get_struct_object<T>(IntPtr ctx, int idx, out T o)
        where T : struct
        {
            object o_t;
            var ret = duk_get_object(ctx, idx, out o_t);
            o = (T)o_t;
            return ret;
        }

        public static bool duk_get_struct_object<T>(IntPtr ctx, int idx, out T? o)
        where T : struct
        {
            object o_t;
            var ret = duk_get_object(ctx, idx, out o_t);
            o = (T)o_t;
            return ret;
        }

        // not value type (except string/array)
        public static bool duk_get_class_object<T>(IntPtr ctx, int idx, out T o)
        where T : class
        {
            object o_t;
            var ret = duk_get_object(ctx, idx, out o_t);
            o = o_t as T;
            if (o_t != null && o == null)
            {
                throw new InvalidCastException(string.Format("{0} type mismatch {1}", o_t.GetType(), typeof(T)));
                // return false;
            }
            return ret;
        }

        public static bool duk_get_object(IntPtr ctx, int idx, out object o)
        {
            if (DuktapeDLL.duk_is_null_or_undefined(ctx, idx)) // or check for object?
            {
                o = null;
                return true;
            }
            if (DuktapeDLL.duk_get_prop_string(ctx, idx, DuktapeVM.OBJ_PROP_NATIVE))
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

        public static bool duk_get_struct_array<T>(IntPtr ctx, int idx, out T[] o)
        where T : struct
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                o = new T[length];
                idx = DuktapeDLL.duk_normalize_index(ctx, idx);
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    T e;
                    if (duk_get_struct_object(ctx, -1, out e))
                    {
                        o[i] = e;
                    }
                }
                return true;
            }
            o = null;
            return false;
        }

        public static bool duk_get_struct_array<T>(IntPtr ctx, int idx, out T?[] o)
        where T : struct
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                o = new T?[length];
                idx = DuktapeDLL.duk_normalize_index(ctx, idx);
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    T? e;
                    if (duk_get_struct_object(ctx, -1, out e))
                    {
                        o[i] = e;
                    }
                }
                return true;
            }
            o = null;
            return false;
        }

        public static bool duk_get_class_array<T>(IntPtr ctx, int idx, out T[] o)
        where T : class
        {
            if (DuktapeDLL.duk_is_array(ctx, idx))
            {
                var length = DuktapeDLL.duk_unity_get_length(ctx, idx);
                o = new T[length];
                idx = DuktapeDLL.duk_normalize_index(ctx, idx);
                for (var i = 0U; i < length; i++)
                {
                    DuktapeDLL.duk_get_prop_index(ctx, idx, i);
                    T e;
                    if (duk_get_class_object(ctx, -1, out e))
                    {
                        o[i] = e;
                    }
                }
                return true;
            }
            o = null;
            return false;
        }
    }
}
