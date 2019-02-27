
#include "duk_internal.h"

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

DUK_EXTERNAL void duk_unity_push2i(duk_context *ctx, duk_int_t v1, duk_int_t v2) {
    duk_push_array(ctx);
    duk_push_int(ctx, v1);
    duk_put_prop_index(ctx, -2, 0);
    duk_push_int(ctx, v2);
    duk_put_prop_index(ctx, -2, 1);
}

DUK_EXTERNAL void duk_unity_push3i(duk_context *ctx, duk_int_t v1, duk_int_t v2, duk_int_t v3) {
    duk_push_array(ctx);
    duk_push_int(ctx, v1);
    duk_put_prop_index(ctx, -2, 0);
    duk_push_int(ctx, v2);
    duk_put_prop_index(ctx, -2, 1);
    duk_push_int(ctx, v3);
    duk_put_prop_index(ctx, -2, 2);
}

DUK_EXTERNAL void duk_unity_push4i(duk_context *ctx, duk_int_t v1, duk_int_t v2, duk_int_t v3, duk_int_t v4) {
    duk_push_array(ctx);
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

DUK_EXTERNAL void duk_unity_push2f(duk_context *ctx, float v1, float v2) {
    duk_push_array(ctx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, -2, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, -2, 1);
}

DUK_EXTERNAL void duk_unity_push3f(duk_context *ctx, float v1, float v2, float v3) {
    duk_push_array(ctx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, -2, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, -2, 1);
    duk_push_number(ctx, v3);
    duk_put_prop_index(ctx, -2, 2);
}

DUK_EXTERNAL void duk_unity_push4f(duk_context *ctx, float v1, float v2, float v3, float v4) {
    duk_push_array(ctx);
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
    if (duk_is_array(ctx, idx)) {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = (float)duk_get_number(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = (float)duk_get_number(ctx, -1);
        }
        duk_pop_2(ctx);
        return 1;
    }
    return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get3f(duk_context *ctx, duk_idx_t idx, float *v1, float *v2, float *v3) {
    idx = duk_normalize_index(ctx, idx);
    if (duk_is_array(ctx, idx)) {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = (float)duk_get_number(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = (float)duk_get_number(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 2)) {
            *v3 = (float)duk_get_number(ctx, -1);
        }
        duk_pop_3(ctx);
        return 1;
    }
    return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get4f(duk_context *ctx, duk_idx_t idx, float *v1, float *v2, float *v3, float *v4) {
    idx = duk_normalize_index(ctx, idx);
    if (duk_is_array(ctx, idx)) {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = (float)duk_get_number(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = (float)duk_get_number(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 2)) {
            *v3 = (float)duk_get_number(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 3)) {
            *v4 = (float)duk_get_number(ctx, -1);
        }
        duk_pop_n(ctx, 4);
        return 1;
    }
    return 0;
}

// int

DUK_EXTERNAL duk_bool_t duk_unity_get2i(duk_context *ctx, duk_idx_t idx, duk_int_t *v1, duk_int_t *v2) {
    idx = duk_normalize_index(ctx, idx);
    if (duk_is_array(ctx, idx)) {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = duk_get_int(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = duk_get_int(ctx, -1);
        }
        duk_pop_2(ctx);
        return 1;
    }
    return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get3i(duk_context *ctx, duk_idx_t idx, duk_int_t *v1, duk_int_t *v2, duk_int_t *v3) {
    idx = duk_normalize_index(ctx, idx);
    if (duk_is_array(ctx, idx)) {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = duk_get_int(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = duk_get_int(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 2)) {
            *v3 = duk_get_int(ctx, -1);
        }
        duk_pop_3(ctx);
        return 1;
    }
    return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get4i(duk_context *ctx, duk_idx_t idx, duk_int_t *v1, duk_int_t *v2, duk_int_t *v3, duk_int_t *v4) {
    idx = duk_normalize_index(ctx, idx);
    if (duk_is_array(ctx, idx)) {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = duk_get_int(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = duk_get_int(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 2)) {
            *v3 = duk_get_int(ctx, -1);
        }
        if (duk_get_prop_index(ctx, idx, 3)) {
            *v4 = duk_get_int(ctx, -1);
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

DUK_EXTERNAL void duk_unity_open(duk_context *ctx) {
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
        return (duk_size_t)debugger->dbg_peek_cb(debugger->udata, buffer, (duk_int_t)length);
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
        return debugger->dbg_request_cb(debugger->udata);
    }
    return 0;
}

DUK_LOCAL void duk_unity_wrap_debug_detached_function(duk_context *ctx, void *udata) {
    duk_unity_debugger *debugger = (duk_unity_debugger *)udata;
    if (debugger != NULL) {
        debugger->dbg_detached_cb(ctx, debugger->udata);
    }
}

DUK_EXTERNAL void *duk_unity_attch_debugger(duk_context *ctx, 
                                           duk_unity_debug_read_function read_cb, 
                                           duk_unity_debug_write_function write_cb, 
                                           duk_unity_debug_peek_function peek_cb, 
                                           duk_unity_debug_read_flush_function read_flush_cb,
                                           duk_unity_debug_write_flush_function write_flush_cb,
                                           duk_unity_debug_request_function request_cb,
                                           duk_unity_debug_detached_function detached_cb, 
                                           duk_int_t udata) {
    // 
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
                            duk_unity_wrap_debugger_detached_cb,    /* debugger detached callback */
                            debugger);                              /* debug udata */
    }
    return debugger;
}

DUK_EXTERNAL void duk_unity_detach_debugger(duk_context *ctx, void *debugger) {
    duk_debugger_detach(ctx);
    duk_free(ctx, debugger);
}
