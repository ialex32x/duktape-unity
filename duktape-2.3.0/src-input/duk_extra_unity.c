
#include "duk_internal.h"

#include <librws.h>

// 获取 idx 的属性 key, 不存在则创建, 栈上为该属性 object
DUK_LOCAL void duk_unity_get_prop_object(duk_context *ctx, duk_idx_t idx, const char *key) {
    if (!duk_get_prop_string(ctx, idx, key)) {
        duk_pop(ctx);
        idx = duk_normalize_index(ctx, idx);
        duk_push_object(ctx);
        duk_dup_top(ctx);
        duk_put_prop_string(ctx, idx, key);
    }
}

DUK_LOCAL void duk_unity_begin_class(duk_context *ctx, const char *key, duk_c_function ctor, duk_c_function dtor) {
    duk_push_c_function(ctx, ctor, DUK_VARARGS); // ctor
    duk_dup_top(ctx);
    duk_put_prop_string(ctx, -3, key);
    duk_push_object(ctx); // ctor, prototype
    duk_push_c_function(ctx, dtor, 1); // ctor, prototype, finalizer
    duk_set_finalizer(ctx, -2); // ctor, prototype
    duk_dup_top(ctx); // ctor, prototype, prototype
    duk_put_prop_string(ctx, -3, "prototype"); // ctor, prototype    
}

DUK_LOCAL void duk_unity_add_member(duk_context *ctx, const char *name, duk_c_function fn, duk_idx_t idx) {
    idx = duk_normalize_index(ctx, idx);
    duk_push_c_function(ctx, fn, DUK_VARARGS);
    duk_put_prop_string(ctx, idx, name);
}

DUK_LOCAL void duk_unity_add_property(duk_context *ctx, const char *name, duk_c_function getter, duk_c_function setter, duk_idx_t idx) {
    idx = duk_normalize_index(ctx, idx);
    duk_uint_t flags = 0;
    duk_push_string(ctx, name);
    if (getter != NULL)
    {
        flags |= DUK_DEFPROP_HAVE_GETTER;
        duk_push_c_function(ctx, getter, 0);
    }
    if (setter != NULL)
    {
        flags |= DUK_DEFPROP_HAVE_SETTER;
        duk_push_c_function(ctx, setter, 1);
    }
    // [ctor, prototype, name, ?getter, ?setter]
    duk_def_prop(ctx, idx, flags | DUK_DEFPROP_SET_ENUMERABLE | DUK_DEFPROP_CLEAR_CONFIGURABLE);
}

DUK_LOCAL void duk_unity_end_class(duk_context *ctx) {
    duk_pop_2(ctx);
}


/*
 *  unity helpers
 */

DUK_EXTERNAL void duk_unity_error_raw(duk_hthread *thr, duk_errcode_t err_code, const char *filename, duk_int_t line, const char *fmt) {
    duk_error_raw(thr, err_code, filename, line, fmt);
}

DUK_EXTERNAL const char *duk_unity_get_lstring(duk_context *ctx, duk_idx_t idx, duk_uint_t *out_len) {
    duk_size_t size;
    const char *ret = duk_get_lstring(ctx, idx, &size);
    if (out_len != NULL) {
        *out_len = (duk_uint_t)size;
    } 
    return ret;
}

DUK_EXTERNAL duk_int_t duk_unity_compile_raw(duk_context *ctx, const char *src_buffer, duk_uint_t src_length, duk_uint_t flags) {
    return duk_compile_raw(ctx, src_buffer, (duk_size_t)src_length, flags);
}

DUK_EXTERNAL duk_int_t duk_unity_eval_raw(duk_context *ctx, const char *src_buffer, duk_uint_t src_length, duk_uint_t flags) {
    return duk_eval_raw(ctx, src_buffer, (duk_size_t)src_length, flags);
}

DUK_EXTERNAL duk_codepoint_t duk_unity_char_code_at(duk_context *ctx, duk_idx_t idx, duk_uint_t char_offset) {
    return duk_char_code_at(ctx, idx, (duk_size_t)char_offset);
}

DUK_EXTERNAL void duk_unity_substring(duk_context *ctx, duk_idx_t idx, duk_uint_t start_char_offset, duk_uint_t end_char_offset) {
    duk_substring(ctx, idx, (duk_size_t)start_char_offset, (duk_size_t)end_char_offset);
}

DUK_EXTERNAL void *duk_unity_resize_buffer(duk_context *ctx, duk_idx_t idx, duk_uint_t new_size) {
    void *ptr = duk_resize_buffer(ctx, idx, (duk_size_t)new_size);
    return ptr;
}

DUK_EXTERNAL void *duk_unity_steal_buffer(duk_context *ctx, duk_idx_t idx, duk_uint_t *out_size) {
    duk_size_t size;
    void *ptr = duk_steal_buffer(ctx, idx, &size);
    if (out_size != NULL) {
        *out_size = (duk_uint_t)size;
    }
    return ptr;
}

DUK_EXTERNAL void duk_unity_config_buffer(duk_context *ctx, duk_idx_t idx, void *ptr, duk_uint_t len) {
    duk_config_buffer(ctx, idx, ptr, (duk_size_t)len);
}

DUK_EXTERNAL duk_uint_t duk_unity_get_length(duk_context *ctx, duk_idx_t idx) {
    return (duk_uint_t)duk_get_length(ctx, idx);
}

DUK_EXTERNAL void duk_unity_set_length(duk_context *ctx, duk_idx_t idx, duk_uint_t len) {
    duk_set_length(ctx, idx, (duk_size_t)len);
}

DUK_EXTERNAL const char *duk_unity_safe_to_lstring(duk_context *ctx, duk_idx_t idx, duk_uint_t *out_len) {
    duk_size_t size;
    const char *ret = duk_safe_to_lstring(ctx, idx, &size);
    if (out_len != NULL) {
        *out_len = (duk_uint_t)size;
    }
    return ret;
}

DUK_EXTERNAL void *duk_unity_alloc_raw(duk_context *ctx, duk_uint_t size) {
    return duk_alloc_raw(ctx, (duk_size_t)size);
}

DUK_EXTERNAL void *duk_unity_realloc_raw(duk_context *ctx, void *ptr, duk_uint_t size) {
    return duk_realloc_raw(ctx, ptr, (duk_size_t)size);
}

DUK_EXTERNAL void *duk_unity_alloc(duk_context *ctx, duk_uint_t size) {
    return duk_alloc(ctx, (duk_size_t)size);
}

DUK_EXTERNAL void *duk_unity_realloc(duk_context *ctx, void *ptr, duk_uint_t size) {
    return duk_realloc(ctx, ptr, (duk_size_t)size);
}

DUK_EXTERNAL const char *duk_unity_push_lstring(duk_context *ctx, const char *str, duk_uint_t len) {
    return duk_push_lstring(ctx, str, (duk_size_t)len);
}

DUK_EXTERNAL const char *duk_unity_require_lstring(duk_context *ctx, duk_idx_t idx, duk_uint_t *out_len) {
    duk_size_t size;
    const char *ret = duk_require_lstring(ctx, idx, &size);
    if (out_len != NULL) {
        *out_len = (duk_uint_t)size;
    }
    return ret;
}

DUK_EXTERNAL void *duk_unity_require_buffer(duk_context *ctx, duk_idx_t idx, duk_uint_t *out_size) {
    duk_size_t size;
    void *ret = duk_require_buffer(ctx, idx, &size);
    if (out_size != NULL) {
        *out_size = (duk_uint_t)size;
    }
    return ret;
}
DUK_EXTERNAL void *duk_unity_require_buffer_data(duk_context *ctx, duk_idx_t idx, duk_uint_t *out_size) {
    duk_size_t size;
    void *ret = duk_require_buffer_data(ctx, idx, &size);
    if (out_size != NULL) {
        *out_size = (duk_uint_t)size;
    }
    return ret;
}

DUK_EXTERNAL const char *duk_unity_to_lstring(duk_context *ctx, duk_idx_t idx, duk_uint_t *out_len) {
    duk_size_t size;
    const char *ret = duk_to_lstring(ctx, idx, &size);
    if (out_len != NULL) {
        *out_len = (duk_uint_t)size;
    }
    return ret;
}

DUK_EXTERNAL void *duk_unity_to_buffer_raw(duk_context *ctx, duk_idx_t idx, duk_uint_t *out_size, duk_uint_t flags) {
    duk_size_t size;
    void *ret = duk_to_buffer_raw(ctx, idx, &size, flags);
    if (out_size != NULL) {
        *out_size = (duk_uint_t)size;
    }
    return ret;
}


DUK_EXTERNAL void *duk_unity_get_buffer(duk_context *ctx, duk_idx_t idx, duk_uint_t *out_size) {
    duk_size_t size;
    void *ptr = duk_get_buffer(ctx, idx, &size);
    if (out_size != NULL) {
        *out_size = (duk_uint_t)size;
    }
    return ptr;
}

DUK_EXTERNAL void *duk_unity_get_buffer_data(duk_context *ctx, duk_idx_t idx, duk_uint_t *out_size) {
    duk_size_t size;
    void *ptr = duk_get_buffer_data(ctx, idx, &size);
    if (out_size != NULL) {
        *out_size = (duk_uint_t)size;
    }
    return ptr;
}

// int 

DUK_EXTERNAL void duk_unity_put2i(duk_context *ctx, duk_int_t v1, duk_int_t v2) {
    // duk_push_array(ctx);
    duk_push_int(ctx, v1);
    duk_put_prop_index(ctx, -2, 0);
    duk_push_int(ctx, v2);
    duk_put_prop_index(ctx, -2, 1);
}

DUK_EXTERNAL void duk_unity_put3i(duk_context *ctx, duk_int_t v1, duk_int_t v2, duk_int_t v3) {
    // duk_push_array(ctx);
    duk_push_int(ctx, v1);
    duk_put_prop_index(ctx, -2, 0);
    duk_push_int(ctx, v2);
    duk_put_prop_index(ctx, -2, 1);
    duk_push_int(ctx, v3);
    duk_put_prop_index(ctx, -2, 2);
}

DUK_EXTERNAL void duk_unity_put4i(duk_context *ctx, duk_int_t v1, duk_int_t v2, duk_int_t v3, duk_int_t v4) {
    // duk_push_array(ctx);
    duk_push_int(ctx, v1);
    duk_put_prop_index(ctx, -2, 0);
    duk_push_int(ctx, v2);
    duk_put_prop_index(ctx, -2, 1);
    duk_push_int(ctx, v3);
    duk_put_prop_index(ctx, -2, 2);
    duk_push_int(ctx, v4);
    duk_put_prop_index(ctx, -2, 3);
}

// float

DUK_EXTERNAL void duk_unity_put2f(duk_context *ctx, float v1, float v2) {
    // duk_push_array(ctx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, -2, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, -2, 1);
}

DUK_EXTERNAL void duk_unity_put3f(duk_context *ctx, float v1, float v2, float v3) {
    // duk_push_array(ctx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, -2, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, -2, 1);
    duk_push_number(ctx, v3);
    duk_put_prop_index(ctx, -2, 2);
}

DUK_EXTERNAL void duk_unity_put4f(duk_context *ctx, float v1, float v2, float v3, float v4) {
    // duk_push_array(ctx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, -2, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, -2, 1);
    duk_push_number(ctx, v3);
    duk_put_prop_index(ctx, -2, 2);
    duk_push_number(ctx, v4);
    duk_put_prop_index(ctx, -2, 3);
}

DUK_EXTERNAL duk_bool_t duk_unity_get2f(duk_context *ctx, duk_idx_t idx, float *v1, float *v2) {
    idx = duk_normalize_index(ctx, idx);
    /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        duk_pop_2(ctx);
        return 1;
    }
    return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get3f(duk_context *ctx, duk_idx_t idx, float *v1, float *v2, float *v3) {
    idx = duk_normalize_index(ctx, idx);
    /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 2)) {
            *v3 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        duk_pop_3(ctx);
        return 1;
    }
    return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get4f(duk_context *ctx, duk_idx_t idx, float *v1, float *v2, float *v3, float *v4) {
    idx = duk_normalize_index(ctx, idx);
    /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 2)) {
            *v3 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 3)) {
            *v4 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        duk_pop_n(ctx, 4);
        return 1;
    }
    return 0;
}

// int

DUK_EXTERNAL duk_bool_t duk_unity_get2i(duk_context *ctx, duk_idx_t idx, duk_int_t *v1, duk_int_t *v2) {
    idx = duk_normalize_index(ctx, idx);
    /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = duk_get_int_default(ctx, -1, 0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = duk_get_int_default(ctx, -1, 0);
        }
        duk_pop_2(ctx);
        return 1;
    }
    return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get3i(duk_context *ctx, duk_idx_t idx, duk_int_t *v1, duk_int_t *v2, duk_int_t *v3) {
    idx = duk_normalize_index(ctx, idx);
    /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = duk_get_int_default(ctx, -1, 0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = duk_get_int_default(ctx, -1, 0);
        }
        if (duk_get_prop_index(ctx, idx, 2)) {
            *v3 = duk_get_int_default(ctx, -1, 0);
        }
        duk_pop_3(ctx);
        return 1;
    }
    return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get4i(duk_context *ctx, duk_idx_t idx, duk_int_t *v1, duk_int_t *v2, duk_int_t *v3, duk_int_t *v4) {
    idx = duk_normalize_index(ctx, idx);
    /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = duk_get_int_default(ctx, -1, 0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = duk_get_int_default(ctx, -1, 0);
        }
        if (duk_get_prop_index(ctx, idx, 2)) {
            *v3 = duk_get_int_default(ctx, -1, 0);
        }
        if (duk_get_prop_index(ctx, idx, 3)) {
            *v4 = duk_get_int_default(ctx, -1, 0);
        }
        duk_pop_n(ctx, 4);
        return 1;
    }
    return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_set_prop_i(duk_context *ctx, duk_idx_t idx, const char *key, duk_int_t val) {
    idx = duk_normalize_index(ctx, idx);
    duk_push_int(ctx, val);
    return duk_put_prop_string(ctx, idx, key);
}

DUK_LOCAL void duk_refsys_open(duk_context *ctx) {
    // begin: ref system
    duk_push_heap_stash(ctx); // [stash]
    duk_push_array(ctx); // [stash, array]
    duk_dup_top(ctx); // [stash, array, array]
    duk_put_prop_string(ctx, -3, "c_registry"); // [stash, array]
    duk_push_int(ctx, 0); // [stash, array, 0]
    duk_put_prop_index(ctx, -2, 0); // [stash, array]
    duk_pop_2(ctx);
    // end: ref system
}

/// Creates and returns a reference for the object at the top of the stack (and pops the object).
DUK_EXTERNAL duk_uint_t duk_unity_ref(duk_context *ctx) {
    if (duk_is_null_or_undefined(ctx, -1)) {
        return 0;
    }
    duk_push_heap_stash(ctx); // obj, stash
    duk_get_prop_string(ctx, -1, "c_registry"); // obj, stash, array
    duk_get_prop_index(ctx, -1, 0); // obj, stash, array, array[0]
    duk_uint_t refid = duk_get_uint(ctx, -1); // obj, stash, array, array[0]
    if (refid > 0) {
        duk_get_prop_index(ctx, -2, refid); // obj, stash, array, array[0], array[refid]
        duk_uint_t freeid = duk_get_uint(ctx, -1);
        duk_put_prop_index(ctx, -3, 0); // obj, stash, array, array[0] ** update free ptr
        duk_dup(ctx, -4); // obj, stash, array, array[0], obj
        duk_put_prop_index(ctx, -3, refid); // obj, stash, array, array[0]
        duk_pop_n(ctx, 4); // []
    } else {
        refid = (int)duk_unity_get_length(ctx, -2);
        duk_dup(ctx, -4); // obj, stash, array, array[0], obj
        duk_put_prop_index(ctx, -3, refid); // obj, stash, array, array[0]
        duk_pop_n(ctx, 4); // []
    }
    return refid;
}

// push object referenced by refid to top of the stack
DUK_EXTERNAL void duk_unity_getref(duk_context *ctx, duk_uint_t refid) {
    if (refid == 0) {
        duk_push_undefined(ctx);
        return;
    }
    duk_push_heap_stash(ctx); // stash
    duk_get_prop_string(ctx, -1, "c_registry"); // stash, array
    duk_get_prop_index(ctx, -1, refid); // stash, array, array[refid]
    duk_remove(ctx, -2);
    duk_remove(ctx, -2);
}

/// Releases reference refid
DUK_EXTERNAL void duk_unity_unref(duk_context *ctx, duk_uint_t refid) {
    if (refid == 0) {
        // do nothing for null/undefined reference
        return;
    }
    duk_push_heap_stash(ctx); // stash
    duk_get_prop_string(ctx, -1, "c_registry"); // stash, array
    duk_get_prop_index(ctx, -1, 0); // stash, array, array[0]
    duk_uint_t freeid = duk_get_int(ctx, -1); // stash, array, array[0]
    duk_push_uint(ctx, refid); // stash, array, array[0], refid
    duk_put_prop_index(ctx, -3, 0); // stash, array, array[0] ** set t[freeid] = refid
    duk_push_uint(ctx, freeid); // stash, array, array[0], freeid
    duk_put_prop_index(ctx, -3, refid); // stash, array, array[0] ** set t[refid] = freeid
    duk_pop_3(ctx); // []
}

DUK_EXTERNAL void *duk_unity_push_buffer_raw(duk_context *ctx, duk_uint_t size, duk_uint_t flags) {
    return duk_push_buffer_raw(ctx, (duk_size_t) size, (duk_small_uint_t) flags);
}

DUK_EXTERNAL void duk_unity_push_buffer_object(duk_context *ctx, duk_idx_t idx_buffer, duk_uint_t byte_offset, duk_uint_t byte_length, duk_uint_t flags) {
    duk_push_buffer_object(ctx, idx_buffer, byte_offset, byte_length, flags);
}

DUK_EXTERNAL duk_idx_t duk_unity_push_error_object_raw(duk_context *ctx, duk_errcode_t err_code, const char *filename, duk_int_t line, const char *fmt) {
    return duk_push_error_object_raw(ctx, err_code, filename, line, fmt);
}

// debugger support

struct duk_unity_debugger {
	duk_unity_debug_read_function dbg_read_cb;                /* required, NULL implies detached */
	duk_unity_debug_write_function dbg_write_cb;              /* required */
	duk_unity_debug_peek_function dbg_peek_cb;
    duk_unity_debug_read_flush_function dbg_read_flush_cb;
    duk_unity_debug_write_flush_function dbg_write_flush_cb;
    duk_unity_debug_request_function dbg_request_cb;
    duk_unity_debug_detached_function dbg_detached_cb;
    duk_int_t udata;
};
typedef struct duk_unity_debugger duk_unity_debugger;

DUK_LOCAL duk_size_t duk_unity_wrap_debug_read_function(void *udata, char *buffer, duk_size_t length) {
    duk_unity_debugger *debugger = (duk_unity_debugger *)udata;
    if (debugger != NULL) {
        // 
        return (duk_size_t)debugger->dbg_read_cb(debugger->udata, buffer, (duk_int_t)length);
    }
    return 0;
}

DUK_LOCAL duk_size_t duk_unity_wrap_debug_write_function(void *udata, const char *buffer, duk_size_t length) {
    duk_unity_debugger *debugger = (duk_unity_debugger *)udata;
    if (debugger != NULL) {
        return (duk_size_t)debugger->dbg_write_cb(debugger->udata, buffer, (duk_int_t)length);
    }
    return 0;
}

DUK_LOCAL duk_size_t duk_unity_wrap_debug_peek_function(void *udata) {
    duk_unity_debugger *debugger = (duk_unity_debugger *)udata;
    if (debugger != NULL) {
        return (duk_size_t)debugger->dbg_peek_cb(debugger->udata);
    }
    return 0;
}

DUK_LOCAL void duk_unity_wrap_debug_read_flush_function(void *udata) {
    duk_unity_debugger *debugger = (duk_unity_debugger *)udata;
    if (debugger != NULL) {
        debugger->dbg_read_flush_cb(debugger->udata);
    }
}

DUK_LOCAL void duk_unity_wrap_debug_write_flush_function(void *udata) {
    duk_unity_debugger *debugger = (duk_unity_debugger *)udata;
    if (debugger != NULL) {
        debugger->dbg_write_flush_cb(debugger->udata);
    }
}

DUK_LOCAL duk_idx_t duk_unity_wrap_debug_request_function(duk_context *ctx, void *udata, duk_idx_t nvalues) {
    duk_unity_debugger *debugger = (duk_unity_debugger *)udata;
    if (debugger != NULL) {
        return debugger->dbg_request_cb(ctx, debugger->udata, nvalues);
    }
    return 0;
}

DUK_LOCAL void duk_unity_wrap_debug_detached_function(duk_context *ctx, void *udata) {
    duk_unity_debugger *debugger = (duk_unity_debugger *)udata;
    if (debugger != NULL) {
        debugger->dbg_detached_cb(ctx, debugger->udata);
    }
}

DUK_EXTERNAL void *duk_unity_attach_debugger(duk_context *ctx, 
                                           duk_unity_debug_read_function read_cb, 
                                           duk_unity_debug_write_function write_cb, 
                                           duk_unity_debug_peek_function peek_cb, 
                                           duk_unity_debug_read_flush_function read_flush_cb,
                                           duk_unity_debug_write_flush_function write_flush_cb,
                                           duk_unity_debug_request_function request_cb,
                                           duk_unity_debug_detached_function detached_cb, 
                                           duk_int_t udata) {
#if defined(DUK_USE_DEBUGGER_SUPPORT)
    duk_unity_debugger *debugger = duk_alloc(ctx, sizeof(duk_unity_debugger));
    if (debugger != NULL) {
        debugger->dbg_read_cb = read_cb;
        debugger->dbg_write_cb = write_cb;
        debugger->dbg_peek_cb = peek_cb;
        debugger->dbg_read_flush_cb = read_flush_cb;
        debugger->dbg_write_flush_cb = write_flush_cb;
        debugger->dbg_request_cb = request_cb;
        debugger->dbg_detached_cb = detached_cb;
        debugger->udata = udata;
        duk_debugger_attach(ctx, 
                            duk_unity_wrap_debug_read_function,     /* read callback */
                            duk_unity_wrap_debug_write_function,    /* write callback */
                            duk_unity_wrap_debug_peek_function,     /* peek callback (optional) */
                            duk_unity_wrap_debug_read_flush_function,    /* read flush callback (optional) */
                            duk_unity_wrap_debug_write_flush_function,   /* write flush callback (optional) */
                            duk_unity_wrap_debug_request_function,       /* app request callback (optional) */
                            duk_unity_wrap_debug_detached_function,    /* debugger detached callback */
                            debugger);                              /* debug udata */
    }
    return debugger;
#else
    return NULL;
#endif
}

DUK_EXTERNAL void duk_unity_detach_debugger(duk_context *ctx, void *debugger) {
    duk_debugger_detach(ctx);
    duk_free(ctx, debugger);
}

/**
 * handler, dispatcher, eventdispatcher
 */

DUK_LOCAL duk_ret_t duk_events_handler_constructor(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_events_handler_finalizer(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_events_dispatcher_constructor(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_events_dispatcher_finalizer(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_events_eventdispatcher_constructor(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_events_eventdispatcher_finalizer(duk_context *ctx) {
    return 0;
}

DUK_LOCAL void duk_events_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");
    duk_unity_begin_class(ctx, "Handler", duk_events_handler_constructor, duk_events_handler_finalizer);
    duk_unity_end_class(ctx);
    duk_unity_begin_class(ctx, "Dispatcher", duk_events_dispatcher_constructor, duk_events_dispatcher_finalizer);
    duk_unity_end_class(ctx);
    duk_unity_begin_class(ctx, "EventDispatcher", duk_events_eventdispatcher_constructor, duk_events_eventdispatcher_finalizer);
    duk_unity_end_class(ctx);
    duk_pop_2(ctx); // pop DuktapeJS and global
}

/**
 * websocket support (librws)
 */

#define DUK_RWS_EVENT_CONNECTED 1
#define DUK_RWS_EVENT_DISCONNECTED 2
#define DUK_RWS_EVENT_BIN 3
#define DUK_RWS_EVENT_TEXT 4

struct duk_rws_event {
    duk_uint_t event;
    const void *data;
    duk_size_t len;
    duk_rws_event *next;
};

typedef duk_rws_event duk_rws_event;

struct duk_rws_context {
    duk_bool_t finalized;
    rws_mutex mutex;
    duk_rws_event *head;
    duk_rws_event *tail;
};

typedef duk_rws_context duk_rws_context;

DUK_LOCAL void duk_rws_context_push_event(duk_rws_context *context, duk_uint_t event, const void *data, duk_size_t len) {
    if (context != NULL) {
        rws_mutex_lock(context->mutex);
        if (!context->finalized) {
            duk_rws_event *ev = rws_malloc(sizeof(duk_rws_event));
            void *buf = NULL;
            if (data != NULL && len > 0) {
                buf = rws_malloc(len);
                memcpy(buf, data, len);
            }
            ev->event = event;
            ev->data = buf;
            ev->len = len;
            ev->next = NULL;
            if (context->head == NULL) {
                context->head = context->tail = ev;
            } else {
                context->tail->next = ev;
                context->tail = ev;
            }
        }
        rws_mutex_unlock(context->mutex);
    }
}

DUK_LOCAL duk_rws_event * duk_rws_context_pop_event(duk_rws_context *context) {
    duk_rws_event *ev = NULL;
    rws_mutex_lock(context->mutex);
    if (context->head != NULL) {
        ev = context->head;
        if (context->head == context->tail) {
            context->head = context->tail = NULL;
        } else {
            context->head = ev->next;
        }
        ev->next = NULL;
    }
    rws_mutex_unlock(context->mutex);
    return ev;
}

DUK_LOCAL void duk_rws_context_destroy(duk_rws_context *context) {
    if (context != NULL) {
        rws_mutex mutex = context->mutex;
        duk_rws_event *ev = NULL;
        rws_mutex_lock(mutex);
        context->mutex = NULL;
        context->finalized = 1;
        ev = context->head;
        context->head = NULL;
        rws_mutex_unlock(mutex);
        while (ev != NULL) {
            if (ev->data != NULL) {
                rws_free(ev->data);
            }
            ev = ev->next;
        }
        rws_mutex_delete(mutex);
        rws_free(context);
    }
}

DUK_LOCAL void _rws_on_socket_connected(rws_socket sock) {
    duk_rws_context *context = rws_socket_get_user_object(sock);
    duk_rws_context_push_event(context, DUK_RWS_EVENT_CONNECTED, NULL, 0);
}

DUK_LOCAL void _rws_on_socket_disconnected(rws_socket sock) {
    duk_rws_context *context = rws_socket_get_user_object(sock);
    duk_rws_context_push_event(context, DUK_RWS_EVENT_DISCONNECTED, NULL, 0);
}

DUK_LOCAL void _rws_on_socket_received_bin(rws_socket sock, const void * data, const unsigned int data_size) {
    duk_rws_context *context = rws_socket_get_user_object(sock);
    duk_rws_context_push_event(context, DUK_RWS_EVENT_BIN, data, (duk_size_t)data_size);
}

DUK_LOCAL void _rws_on_socket_received_text(rws_socket sock, const char * text, const unsigned int text_size) {
    duk_rws_context *context = rws_socket_get_user_object(sock);
    duk_rws_context_push_event(context, DUK_RWS_EVENT_TEXT, text, (duk_size_t)text_size);
}

DUK_LOCAL duk_ret_t duk_rws_socket_constructor(duk_context *ctx) {
    duk_push_this(ctx);
    duk_rws_context *rws_context = rws_malloc(sizeof(duk_rws_context));
    if (rws_context != NULL) {
        rws_socket sock = rws_socket_create();
        rws_context->mutex = rws_mutex_create_recursive();
        rws_context->head = NULL;
        rws_context->tail = NULL;
        rws_context->finalized = 0;
        rws_socket_set_on_connected(sock, _rws_on_socket_connected);
        rws_socket_set_on_disconnected(sock, _rws_on_socket_disconnected);
        rws_socket_set_on_received_bin(sock, _rws_on_socket_received_bin);
        rws_socket_set_on_received_text(sock, _rws_on_socket_received_text);
        duk_push_pointer(ctx, sock);
        duk_put_prop_string(ctx, -2, DUK_HIDDEN_SYMBOL("__ptr"));
        duk_pop(ctx);
    } 
    return 0;
}

DUK_LOCAL duk_ret_t duk_rws_socket_connect(duk_context *ctx) {
    duk_push_this(ctx);
    if (duk_get_prop_string(ctx, -1, DUK_HIDDEN_SYMBOL("__ptr"))) {
        rws_socket sock = (rws_socket)duk_get_pointer_default(ctx, -1, NULL);
        duk_pop_2(ctx);
        if (sock != NULL) {
            //TODO: socket.connect
            // connect(scheme: string, host: string, port: number, path: string)
            if (duk_get_top(ctx) >= 4) {
                if (!duk_is_string(ctx, 0)) {
                    return duk_generic_error(ctx, "scheme should be string");
                }
                if (!duk_is_string(ctx, 1)) {
                    return duk_generic_error(ctx, "host should be string");
                }
                if (!duk_is_number(ctx, 2)) {
                    return duk_generic_error(ctx, "port should be number");
                }
                if (!duk_is_string(ctx, 3)) {
                    return duk_generic_error(ctx, "path should be string");
                }
                char *scheme = duk_get_string_default(ctx, 0, NULL);
                if (scheme == NULL) {
                    return duk_generic_error(ctx, "scheme is null");
                }
                char *host = duk_get_string_default(ctx, 1, NULL);
                if (host == NULL) {
                    return duk_generic_error(ctx, "host is null");
                }
                duk_int_t port = duk_get_int_default(ctx, 2, 0);
                if (port <= 0 || port > 65535) {
                    return duk_generic_error(ctx, "port is invalid");
                }
                char *path = duk_get_string_default(ctx, 3, NULL);
                if (path == NULL) {
                    return duk_generic_error(ctx, "path is null");
                }
                rws_socket_set_url(sock, scheme, host, port, path);
                rws_socket_connect(sock);
            } else {
                return duk_generic_error(ctx, "4 arguments required");
            }
        }
    }
    return 0;
}

DUK_LOCAL duk_ret_t duk_rws_socket_send(duk_context *ctx) {
    duk_push_this(ctx);
    if (duk_get_prop_string(ctx, -1, DUK_HIDDEN_SYMBOL("__ptr"))) {
        rws_socket sock = (rws_socket)duk_get_pointer_default(ctx, -1, NULL);
        duk_pop_2(ctx);
        if (sock != NULL) {
            if (duk_is_string(ctx, 0)) {
                char * text = duk_get_string_default(ctx, 0, NULL);
                if (text != NULL) {
                    rws_socket_send_text(sock, text);
                    duk_push_boolean(ctx, 1);
                } else {
                    duk_push_boolean(ctx, 0);
                }
            } else {
                size_t data_size;
                void * data = duk_get_buffer_data(ctx, 0, &data_size);
                if (data != NULL) {
                    rws_socket_send_bin(sock, data, data_size);
                    duk_push_boolean(ctx, 1);
                } else {
                    duk_push_boolean(ctx, 0);
                }
            }
        } else {
            duk_push_boolean(ctx, 0);
        }
    } else {
        duk_push_boolean(ctx, 0);
    }
    return 1;
}

DUK_LOCAL duk_ret_t duk_rws_socket_is_connected(duk_context *ctx) {
    duk_push_this(ctx);
    if (duk_get_prop_string(ctx, -1, DUK_HIDDEN_SYMBOL("__ptr"))) {
        rws_socket sock = (rws_socket)duk_get_pointer_default(ctx, -1, NULL);
        duk_pop_2(ctx);
        if (sock != NULL) {
            duk_push_boolean(ctx, rws_socket_is_connected(sock));
            return 1;
        }
    }
    return 0;
}

DUK_LOCAL duk_ret_t duk_rws_socket_proc(duk_context *ctx) {
    duk_push_this(ctx);
    if (duk_get_prop_string(ctx, -1, DUK_HIDDEN_SYMBOL("__ptr"))) {
        rws_socket sock = (rws_socket)duk_get_pointer_default(ctx, -1, NULL);
        duk_pop(ctx);
        if (sock != NULL) {
            duk_rws_context *context = rws_socket_get_user_object(sock);
            duk_rws_event *ev = duk_rws_context_pop_event(context);
            if (ev != NULL) {
                switch (ev->event) {
                    case DUK_RWS_EVENT_CONNECTED:
                        duk_push_string(ctx, "dispatch");
                        duk_push_string(ctx, "connected");
                        if (duk_pcall_prop(ctx, -3, 1) != DUK_EXEC_SUCCESS) {
                            return duk_throw(ctx);
                        }
                        duk_pop(ctx);
                        break;
                    case DUK_RWS_EVENT_DISCONNECTED:
                        duk_push_string(ctx, "dispatch");
                        duk_push_string(ctx, "disconnected");
                        if (duk_pcall_prop(ctx, -3, 1) != DUK_EXEC_SUCCESS) {
                            return duk_throw(ctx);
                        }
                        duk_pop(ctx);
                        break;
                    case DUK_RWS_EVENT_TEXT:
                        duk_push_string(ctx, "dispatch");
                        duk_push_string(ctx, "text");
                        // duk_push_lstring(ctx, ev->data, ev->len);
                        void *buf = duk_push_fixed_buffer_nozero(ctx, ev->len);
                        memcpy(buf, ev->data, ev->len);
                        if (duk_pcall_prop(ctx, -3, 2) != DUK_EXEC_SUCCESS) {
                            return duk_throw(ctx);
                        }
                        duk_pop(ctx);
                        break;
                    case DUK_RWS_EVENT_BIN:
                        duk_push_string(ctx, "dispatch");
                        duk_push_string(ctx, "data");
                        void *buf = duk_push_fixed_buffer_nozero(ctx, ev->len);
                        memcpy(buf, ev->data, ev->len);
                        if (duk_pcall_prop(ctx, -3, 2) != DUK_EXEC_SUCCESS) {
                            return duk_throw(ctx);
                        }
                        duk_pop(ctx);
                        break;
                    default: break;
                }
            }
            return 0;
        }
    }
    return 0;
}

DUK_LOCAL duk_ret_t duk_rws_socket_close(duk_context *ctx) {
    duk_push_this(ctx);
    if (duk_get_prop_string(ctx, -1, DUK_HIDDEN_SYMBOL("__ptr"))) {
        rws_socket sock = (rws_socket)duk_get_pointer_default(ctx, -1, NULL);
        duk_pop_2(ctx);
        if (sock != NULL) {
            duk_bool_t is_connected = rws_socket_is_connected(sock);
            duk_rws_context *context = rws_socket_get_user_object(sock);
            duk_rws_context_destroy(context);
            rws_socket_disconnect_and_release(sock);
            duk_del_prop_string(ctx, 0, DUK_HIDDEN_SYMBOL("__ptr"));
            //TODO: dispatch 'close' event here 
            if (is_connected) {
                
            }
        }
    }
    return 0;
}

DUK_LOCAL duk_ret_t duk_rws_socket_finalizer(duk_context *ctx) {
    if (duk_get_prop_string(ctx, 0, DUK_HIDDEN_SYMBOL("__ptr"))) {
        rws_socket sock = (rws_socket)duk_get_pointer_default(ctx, -1, NULL);
        duk_pop(ctx);
        if (sock != NULL) {
            duk_rws_context *context = rws_socket_get_user_object(sock);
            duk_rws_context_destroy(context);
            rws_socket_disconnect_and_release(sock);
            duk_del_prop_string(ctx, 0, DUK_HIDDEN_SYMBOL("__ptr"));
        }
    }
    return 0;
}

DUK_LOCAL void duk_rws_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");
    //TODO: extends EventDispatcher
    duk_unity_begin_class(ctx, "WebSocket", duk_rws_socket_constructor, duk_rws_socket_finalizer);
    // add members of WebSocket
    duk_unity_add_member(ctx, "connect", duk_rws_socket_connect, -1);
    duk_unity_add_property(ctx, "connected", duk_rws_socket_is_connected, NULL, -1);
    duk_unity_add_member(ctx, "send", duk_rws_socket_send, -1);
    duk_unity_add_member(ctx, "close", duk_rws_socket_close, -1);
    duk_unity_add_member(ctx, "_proc", duk_rws_socket_proc, -1);
    duk_unity_end_class(ctx);
    duk_pop_2(ctx); // pop DuktapeJS and global
}

DUK_EXTERNAL void duk_unity_open(duk_context *ctx) {
    duk_refsys_open(ctx);
    duk_rws_open(ctx);
    duk_events_open(ctx);
}
