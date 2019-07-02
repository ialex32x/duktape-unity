#if !defined(DUK_EXTRA_UNITY_H_INCLUDED)
#define DUK_EXTRA_UNITY_H_INCLUDED

#define DUK_UNITY_STASH_REGISTRY 0
#define DUK_UNITY_STASH_BUILTINS 1
    #define DUK_UNITY_BUILTINS_VECTOR2 0
    #define DUK_UNITY_BUILTINS_VECTOR2I 1
    #define DUK_UNITY_BUILTINS_VECTOR3 2
    #define DUK_UNITY_BUILTINS_VECTOR3I 3
    #define DUK_UNITY_BUILTINS_VECTOR4 4
    #define DUK_UNITY_BUILTINS_QUATERNION 5
    #define DUK_UNITY_BUILTINS_COLOR 6
    #define DUK_UNITY_BUILTINS_COLOR32 7
    #define DUK_UNITY_BUILTINS_MATRIX33 8
    #define DUK_UNITY_BUILTINS_MATRIX44 9
    #define DUK_UNITY_BUILTINS_DISPATCHER 10
    #define DUK_UNITY_BUILTINS_HANDLER 11
    #define DUK_UNITY_BUILTINS_EVENTDISPATCHER 12
    #define DUK_UNITY_BUILTINS_WEBSOCKET 13
    #define DUK_UNITY_BUILTINS_TCPSERVER 14
    #define DUK_UNITY_BUILTINS_TCPCLIENT 15
    #define DUK_UNITY_BUILTINS_UDP 16
    #define DUK_UNITY_BUILTINS_KCP 17
    #define DUK_UNITY_BUILTINS_RNG 18

DUK_INTERNAL_DECL void duk_unity_valuetypes_open(duk_context *ctx);

DUK_INTERNAL_DECL void duk_unity_get_prop_object(duk_context *ctx, duk_idx_t idx, const char *key);
DUK_INTERNAL_DECL void duk_unity_begin_class(duk_context *ctx, const char *key, duk_uarridx_t reg_idx, duk_c_function ctor, duk_c_function dtor);
DUK_INTERNAL_DECL void duk_unity_add_member(duk_context *ctx, const char *name, duk_c_function fn, duk_idx_t idx);
DUK_INTERNAL_DECL void duk_unity_add_property(duk_context *ctx, const char *name, duk_c_function getter, duk_c_function setter, duk_idx_t idx);
DUK_INTERNAL_DECL void duk_unity_end_class(duk_context *ctx);

DUK_INTERNAL_DECL void duk_unity_add_const_number(duk_context *ctx, duk_idx_t idx, const char *key, duk_double_t num);
DUK_INTERNAL_DECL void duk_unity_add_const_int(duk_context *ctx, duk_idx_t idx, const char *key, duk_int_t num);

DUK_INTERNAL_DECL duk_ret_t duk_events_eventdispatcher_on(duk_context *ctx);
DUK_INTERNAL_DECL duk_ret_t duk_events_eventdispatcher_off(duk_context *ctx);
DUK_INTERNAL_DECL duk_ret_t duk_events_eventdispatcher_clear(duk_context *ctx);
DUK_INTERNAL_DECL duk_ret_t duk_events_eventdispatcher_dispatch(duk_context *ctx);

#endif // DUK_EXTRA_UNITY_H_INCLUDED
