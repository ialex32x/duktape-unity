#include "duk_internal.h"

/**
 * handler, dispatcher, eventdispatcher
 */

DUK_LOCAL duk_ret_t duk_events_handler_constructor(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_string(ctx, -2, "caller");
    duk_dup(ctx, 1);
    duk_put_prop_string(ctx, -2, "fn");
    duk_dup(ctx, 2);
    duk_to_boolean(ctx, -1);
    duk_put_prop_string(ctx, -2, "once");
    return 0;
}

DUK_LOCAL duk_ret_t duk_events_handler_equals(duk_context *ctx) {
    // equals(caller, fn?)
    duk_idx_t nargs = duk_get_top(ctx);
    duk_push_this(ctx);
    duk_get_prop_string(ctx, -1, "caller");
    if (duk_equals(ctx, 0, -1)) {
        duk_pop(ctx);  // [this]
        if (nargs < 2 || duk_is_null_or_undefined(ctx, 1)) {
            duk_pop(ctx); // []
            duk_push_true(ctx);
            return 1;
        }
        
        duk_get_prop_string(ctx, -1, "fn"); // [this, fn]
        if (duk_equals(ctx, 1, -1)) {
            duk_pop(ctx); // [this]
            duk_pop(ctx); // []
            duk_push_true(ctx);
            return 1;
        }
        duk_pop(ctx);
    }
    duk_push_false(ctx);
    return 1;
}

DUK_LOCAL duk_ret_t duk_events_handler_invoke(duk_context *ctx) {
    duk_idx_t nargs = duk_get_top(ctx);
    duk_push_this(ctx);
    duk_get_prop_string(ctx, -1, "fn");
    duk_get_prop_string(ctx, -2, "caller");
    for (duk_idx_t i = 0; i < nargs; i++) {
        duk_dup(ctx, i);
    }
    if (duk_pcall_method(ctx, nargs) != DUK_EXEC_SUCCESS) {
        return duk_throw(ctx);
    }
    return 1;
}

DUK_LOCAL duk_ret_t duk_events_dispatcher_constructor(duk_context *ctx) {
    duk_push_this(ctx);
    duk_push_array(ctx);
    duk_push_uint(ctx, 0);
    duk_put_prop_index(ctx, -2, 0);
    duk_put_prop_string(ctx, -2, "handlers");
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_events_dispatcher_on(duk_context *ctx) {
    if (!duk_is_function(ctx, 1)) {
        return duk_generic_error(ctx, "fn not a function");
    }
    duk_bool_t once = duk_get_boolean_default(ctx, 2, 0);
    duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_HANDLER);
    duk_dup(ctx, 0);
    duk_dup(ctx, 1);
    duk_push_boolean(ctx, once);
    if (duk_pnew(ctx, 3) != 0) {
        return duk_throw(ctx);
    }
    duk_push_this(ctx);
    duk_get_prop_string(ctx, -1, "handlers"); // handler, this, handlers
    duk_get_prop_index(ctx, -1, 0); // handler, this, handlers, freeslot
    duk_uint_t freeslot = duk_get_uint_default(ctx, -1, 0);
    duk_pop(ctx); // handler, this, handlers
    if (freeslot == 0) {
        duk_size_t length = duk_get_length(ctx, -1);
        duk_dup(ctx, -3); // handler, this, handlers, handler
        duk_put_prop_index(ctx, -2, (duk_uarridx_t)length); // // handler, this, handlers
        duk_pop_3(ctx);
    } else {
        duk_get_prop_index(ctx, -1, freeslot);
        duk_uint_t next_slot = duk_get_uint(ctx, -1);
        duk_pop(ctx);
        duk_push_uint(ctx, next_slot);
        duk_put_prop_index(ctx, -2, 0); // handler, this, handlers
        duk_dup(ctx, -3);
        duk_put_prop_index(ctx, -2, freeslot); // handler, this, handlers
        duk_pop_3(ctx);
    }
    duk_push_this(ctx);
    return 1;
}

DUK_LOCAL duk_ret_t duk_events_dispatcher_off(duk_context *ctx) {
    duk_idx_t nargs = duk_get_top(ctx);
    duk_push_this(ctx);
    duk_get_prop_string(ctx, -1, "handlers"); // [argN], this, handlers
    duk_size_t length = duk_get_length(ctx, -1);
    for (duk_size_t i = 1; i < length; i++) {
        duk_get_prop_index(ctx, -1, (duk_uarridx_t)i); // [argN], this, handlers, el
        if (!duk_is_number(ctx, -1)) {
            duk_push_string(ctx, "equals");
            duk_dup(ctx, 0);
            if (nargs > 1) {
                duk_dup(ctx, 1);
            } else {
                duk_push_undefined(ctx);
            }
            if (duk_pcall_prop(ctx, -4, 2) != DUK_EXEC_SUCCESS) {
                return duk_throw(ctx);
            }
            // if (duk_get_top(ctx) != 4 + nargs) {
            //     return duk_generic_error(ctx, "aaa, nargs: %d top: %d", nargs, duk_get_top(ctx));
            // }
            // [argN], this, handlers, el, el.ret
            duk_bool_t eq = duk_get_boolean_default(ctx, -1, 0);
            duk_pop(ctx); // pop el.ret
            // [argN], this, handlers, el
            if (eq) {
                // this._handlers[i] = this._handlers[0]
                // this._handlers[0] = i
                duk_unity_array_assign(ctx, -2, (duk_uarridx_t)i, 0);
                duk_push_uint(ctx, (duk_uint_t)i); // [argN], this, handlers, el, i
                duk_put_prop_index(ctx, -3, 0);
            }
        }
        duk_pop(ctx); // pop el
        // if (duk_get_top(ctx) != 2 + nargs) {
        //     return duk_generic_error(ctx, "aaa, nargs: %d top: %d", nargs, duk_get_top(ctx));
        // }
    }
    duk_pop_2(ctx); // .
    return 0;
}

DUK_LOCAL duk_ret_t duk_events_dispatcher_dispatch(duk_context *ctx) {
    duk_idx_t nargs = duk_get_top(ctx);
    duk_push_undefined(ctx);
    duk_push_this(ctx);
    duk_get_prop_string(ctx, -1, "handlers"); // stub, this, handlers
    duk_size_t length = duk_get_length(ctx, -1);
    for (duk_size_t i = 1; i < length; i++) {
        duk_get_prop_index(ctx, -1, (duk_uarridx_t)i); // stub, this, handlers, el
        if (!duk_is_number(ctx, -1)) {
            duk_push_string(ctx, "invoke");
            for (duk_idx_t argi = 0; argi < nargs; argi++) {
                duk_dup(ctx, argi);
            }
            if (duk_pcall_prop(ctx, -nargs - 2, nargs) != DUK_EXEC_SUCCESS) {
                return duk_throw(ctx);
            }
            // stub, this, handlers, el, ret
            duk_replace(ctx, -5); // ret, this, handlers, el
        } 
        duk_pop(ctx); // pop el
    }
    duk_pop_2(ctx); // .
    return 1;
}

/*
for (let i = 1; i < this._handlers.length; i++) {
    let el = this._handlers[i]
    if (typeof el != "number") {
        this._handlers[i] = this._handlers[0]
        this._handlers[0] = i
    }
}
*/
DUK_LOCAL duk_ret_t duk_events_dispatcher_clear(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_string(ctx, -1, "handlers"); // this, handlers
    duk_size_t length = duk_get_length(ctx, -1);
    for (duk_size_t i = 1; i < length; i++) {
        duk_get_prop_index(ctx, -1, (duk_uarridx_t)i); // this, handlers, el
        if (!duk_is_number(ctx, -1)) {
            duk_unity_array_assign(ctx, -2, (duk_uarridx_t)i, 0);
            duk_push_uint(ctx, (duk_uint_t)i);
            duk_put_prop_index(ctx, -3, 0);
        }
        duk_pop(ctx);
    }
    duk_pop_2(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_events_eventdispatcher_constructor(duk_context *ctx) {
    duk_push_this(ctx);
    duk_push_object(ctx);
    duk_put_prop_string(ctx, -2, "events");
    duk_pop(ctx);
    return 0;
}

DUK_INTERNAL duk_ret_t duk_events_eventdispatcher_on(duk_context *ctx) {
    // on(type, caller, fn, once?)
    duk_idx_t nargs = duk_get_top(ctx);
    if (nargs < 3) {
        return duk_generic_error(ctx, "3 args at least");
    }
    if (!duk_is_string(ctx, 0)) {
        return duk_generic_error(ctx, "event type require string");
    }
    if (!duk_is_function(ctx, 2)) {
        return duk_generic_error(ctx, "fn require function");
    }
    duk_push_this(ctx);
    duk_get_prop_string(ctx, -1, "events"); // this, events
    const char *type = duk_get_string_default(ctx, 0, NULL);
    if (type == NULL) {
        return duk_generic_error(ctx, "type is null");
    }
    if (!duk_get_prop_string(ctx, -1, type)) { // this, events, events[type]
        duk_pop(ctx);
        duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_DISPATCHER); 
        if (duk_pnew(ctx, 0) != 0) {
            return duk_throw(ctx);
        }
        duk_dup(ctx, -1);
        // this, events, newdispatcher, newdispatcher
        duk_put_prop_string(ctx, -3, type); // this, events, dispatcher
    }
    duk_push_string(ctx, "on");
    duk_dup(ctx, 1); // caller
    duk_dup(ctx, 2); // fn
    if (nargs < 4) {
        duk_push_false(ctx); // once:false
    } else {
        duk_dup(ctx, 3); // once
    }
    if (duk_pcall_prop(ctx, -5, 3) != 0) {
        return duk_throw(ctx);
    }
    // this, events, dispatcher, ret
    duk_remove(ctx, -2);
    duk_remove(ctx, -2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_INTERNAL duk_ret_t duk_events_eventdispatcher_off(duk_context *ctx) {
    // off(type, caller, fn)
    duk_idx_t nargs = duk_get_top(ctx);
    if (nargs < 2) {
        return duk_generic_error(ctx, "2 args at least");
    }
    if (!duk_is_string(ctx, 0)) {
        return duk_generic_error(ctx, "event type require string");
    }
    if (nargs > 2 && !duk_is_function(ctx, 2) && !duk_is_null_or_undefined(ctx, 2)) {
        return duk_generic_error(ctx, "fn require function or null");
    }
    duk_push_this(ctx);
    duk_get_prop_string(ctx, -1, "events"); // this, events
    const char *type = duk_get_string_default(ctx, 0, NULL);
    if (type == NULL) {
        duk_pop_2(ctx);
        return 0;
    }
    if (!duk_get_prop_string(ctx, -1, type)) { // this, events, events[type]
        duk_pop_3(ctx);
        return 0;
    }
    // this, events, dispatcher
    duk_push_string(ctx, "off");
    duk_dup(ctx, 1); // caller
    if (nargs < 3) {
        duk_push_undefined(ctx);
    } else {
        duk_dup(ctx, 2); // fn
    }
    if (duk_pcall_prop(ctx, -4, 2) != 0) {
        return duk_throw(ctx);
    }
    // this, events, dispatcher, ret
    duk_remove(ctx, -2);
    duk_remove(ctx, -2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_INTERNAL duk_ret_t duk_events_eventdispatcher_clear(duk_context *ctx) {
    // clear(type)
    duk_idx_t nargs = duk_get_top(ctx);
    if (nargs < 1) {
        return duk_generic_error(ctx, "1 args at least");
    }
    if (!duk_is_string(ctx, 0)) {
        return duk_generic_error(ctx, "event type require string");
    }
    duk_push_this(ctx);
    duk_get_prop_string(ctx, -1, "events"); // this, events
    const char *type = duk_get_string_default(ctx, 0, NULL);
    if (type == NULL) {
        duk_pop_2(ctx);
        return 0;
    }
    if (!duk_get_prop_string(ctx, -1, type)) { // this, events, events[type]
        duk_pop_3(ctx);
        return 0;
    }
    // this, events, dispatcher
    duk_push_string(ctx, "clear");
    if (duk_pcall_prop(ctx, -2, 0) != 0) {
        return duk_throw(ctx);
    }
    // this, events, dispatcher, ret
    duk_remove(ctx, -2);
    duk_remove(ctx, -2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_INTERNAL duk_ret_t duk_events_eventdispatcher_dispatch(duk_context *ctx) {
    // dispatch(type, ...args)
    duk_idx_t nargs = duk_get_top(ctx);
    if (nargs < 1) {
        return duk_generic_error(ctx, "1 args at least");
    }
    if (!duk_is_string(ctx, 0)) {
        return duk_generic_error(ctx, "event type require string");
    }
    duk_push_this(ctx);
    duk_get_prop_string(ctx, -1, "events"); // this, events
    const char *type = duk_get_string_default(ctx, 0, NULL);
    if (type == NULL) {
        duk_pop_2(ctx);
        return 0;
    }
    if (!duk_get_prop_string(ctx, -1, type)) { // this, events, events[type]
        duk_pop_3(ctx);
        return 0;
    }
    // this, events, dispatcher
    duk_push_string(ctx, "dispatch");
    for (duk_idx_t i = 1; i < nargs; i++) {
        duk_dup(ctx, i);
    }
    if (duk_pcall_prop(ctx, -nargs - 1, nargs - 1) != 0) {
        return duk_throw(ctx);
    }
    // this, events, dispatcher, ret
    duk_remove(ctx, -2);
    duk_remove(ctx, -2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_INTERNAL void duk_events_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");
    duk_unity_begin_class(ctx, "Handler", DUK_UNITY_BUILTINS_HANDLER, duk_events_handler_constructor, NULL);
    duk_unity_add_member(ctx, "equals", duk_events_handler_equals, -1);
    duk_unity_add_member(ctx, "invoke", duk_events_handler_invoke, -1);
    duk_unity_end_class(ctx);
    duk_unity_begin_class(ctx, "Dispatcher", DUK_UNITY_BUILTINS_DISPATCHER, duk_events_dispatcher_constructor, NULL);
    duk_unity_add_member(ctx, "on", duk_events_dispatcher_on, -1);
    duk_unity_add_member(ctx, "off", duk_events_dispatcher_off, -1);
    duk_unity_add_member(ctx, "clear", duk_events_dispatcher_clear, -1);
    duk_unity_add_member(ctx, "dispatch", duk_events_dispatcher_dispatch, -1);
    duk_unity_end_class(ctx);
    duk_unity_begin_class(ctx, "EventDispatcher", DUK_UNITY_BUILTINS_EVENTDISPATCHER, duk_events_eventdispatcher_constructor, NULL);
    duk_unity_add_member(ctx, "on", duk_events_eventdispatcher_on, -1);
    duk_unity_add_member(ctx, "off", duk_events_eventdispatcher_off, -1);
    duk_unity_add_member(ctx, "clear", duk_events_eventdispatcher_clear, -1);
    duk_unity_add_member(ctx, "dispatch", duk_events_eventdispatcher_dispatch, -1);
    duk_unity_end_class(ctx);
    duk_pop_2(ctx); // pop DuktapeJS and global
}
