
#include "duk_internal.h"

DUK_INTERNAL duk_bool_t duk_timer_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");

    {
    }

    duk_pop_2(ctx); // pop DuktapeJS and global  

    return 1;
}
