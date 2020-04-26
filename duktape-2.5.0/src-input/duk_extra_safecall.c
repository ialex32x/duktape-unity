#include "duk_internal.h"

// DUK_EXTERNAL void
// DUK_LOCAL DUK_INLINE

DUK_LOCAL DUK_INLINE duk_int_t duk_unity_safecall(duk_context *ctx) {
    duk_int_t magic = duk_get_current_magic(ctx);
    duk_unity_getref(ctx, (duk_uint_t) magic); // unsafe cast
    duk_c_function func = (duk_c_function) duk_get_pointer_default(ctx, -1, NULL);
    duk_pop(ctx);
    if (func != NULL) {
        duk_int_t ret = func(ctx);
        if (ret == -1) {
            duk_throw(ctx);
        }
        return ret;
    }
    return duk_generic_error(ctx, "no underlying function");
}

// wrap csharp function call, 
// csharp return -1 if exception should throw in duktape (exception object on the top of stack)
DUK_EXTERNAL duk_idx_t duk_unity_push_safe_function(duk_context *ctx, duk_c_function func, duk_idx_t nargs) {
    duk_push_pointer(ctx, (void *) func);
    duk_uint_t magic = duk_unity_ref(ctx);
    duk_idx_t func_idx = duk_push_c_function(ctx, duk_unity_safecall, nargs);
    duk_set_magic(ctx, -1, (duk_int_t) magic); // unsafe cast
    return func_idx;
}
