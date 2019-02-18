
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
