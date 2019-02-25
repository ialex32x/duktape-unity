using System;
using AOT;

namespace Duktape
{
    using UnityEngine;

    public class DuktapeJSBuiltins : DuktapeBinding
    {
        [MonoPInvokeCallback(typeof(DuktapeDLL.duk_c_function))]
        public static int Bind_GetEnumName(IntPtr ctx)
        {
            if (DuktapeDLL.duk_get_prop_string(ctx, 0, DuktapeVM.OBJ_PROP_EXPORTED_REFID))
            {
                var refid = DuktapeDLL.duk_get_uint(ctx, -1);
                var type = DuktapeVM.GetVM(ctx).GetExportedType(refid);
                if (type != null)
                {
                    Debug.LogFormat("Type: {0}", type);
                }
                else
                {
                    Debug.LogWarning("no type bounded");
                }
            }
            return 0;
        }

        public static void reg(IntPtr ctx)
        {
            duk_begin_namespace(ctx, "DuktapeJS");
            duk_add_method(ctx, "GetEnumName", Bind_GetEnumName, -1);
            duk_end_namespace(ctx);
        }
    }
}
