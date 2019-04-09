
#include "duk_internal.h"

// #include "libwebsockets.h"

DUK_LOCAL duk_ret_t duk_WebSocket_constructor(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_WebSocket_connect(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_WebSocket_close(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_WebSocket_poll(duk_context *ctx) {
    return 0;
}

DUK_INTERNAL duk_bool_t duk_websocket_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");
    duk_unity_begin_class(ctx, "WebSocket", DUK_UNITY_BUILTINS_WEBSOCKET, duk_WebSocket_constructor, NULL);
    {
        duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_EVENTDISPATCHER);
        duk_unity_inherit(ctx);
        duk_pop(ctx);
    }
    duk_unity_add_member(ctx, "connect", duk_WebSocket_connect, -1);
    duk_unity_add_member(ctx, "close", duk_WebSocket_close, -1);
    duk_unity_add_member(ctx, "poll", duk_WebSocket_poll, -1);
    duk_unity_end_class(ctx);
    duk_pop_2(ctx); // pop DuktapeJS and global    
    return 1;
}
