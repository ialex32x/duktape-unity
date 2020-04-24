
#include "duk_internal.h"

DUK_EXTERNAL void duk_builtins_reg_put(duk_context *ctx, duk_uarridx_t key) {
    duk_push_heap_stash(ctx);
    duk_get_prop_index(ctx, -1, DUK_UNITY_STASH_BUILTINS); //duk_get_prop_string(ctx, -1, "c_builtins"); // obj, stash, builtins
    duk_dup(ctx, -3); // obj, stash, builtins, obj
    duk_put_prop_index(ctx, -2, key); // obj, stash, builtins
    duk_pop_3(ctx);
}

DUK_EXTERNAL void duk_builtins_reg_get(duk_context *ctx, duk_uarridx_t key) {
    duk_push_heap_stash(ctx);
    duk_get_prop_index(ctx, -1, DUK_UNITY_STASH_BUILTINS); //duk_get_prop_string(ctx, -1, "c_builtins"); // stash, builtins
    duk_get_prop_index(ctx, -1, key); // stash, builtins, obj
    duk_remove(ctx, -2);
    duk_remove(ctx, -2);
}

DUK_EXTERNAL void duk_unity_inherit(duk_context *ctx) {
    duk_get_prop_string(ctx, -2, "prototype");
    duk_get_prop_string(ctx, -2, "prototype");
    duk_set_prototype(ctx, -2);
    duk_pop(ctx);
}

DUK_INTERNAL void duk_unity_get_prop_object(duk_context *ctx, duk_idx_t idx, const char *key) {
    if (!duk_get_prop_string(ctx, idx, key)) {
        duk_pop(ctx);
        idx = duk_normalize_index(ctx, idx);
        duk_push_object(ctx);
        duk_dup_top(ctx);
        duk_put_prop_string(ctx, idx, key);
    }
}

DUK_LOCAL void duk_unity_array_assign(duk_context *ctx, duk_idx_t obj_idx, duk_uarridx_t dst, duk_uarridx_t from) {
    obj_idx = duk_normalize_index(ctx, obj_idx);
    duk_get_prop_index(ctx, obj_idx, from);
    duk_put_prop_index(ctx, obj_idx, dst);
}

DUK_INTERNAL void duk_unity_begin_class(duk_context *ctx, const char *key, duk_uarridx_t reg_idx, duk_c_function ctor, duk_c_function dtor) {
    duk_push_c_function(ctx, ctor, DUK_VARARGS); // ctor
    duk_dup_top(ctx);
    duk_builtins_reg_put(ctx, reg_idx);
    duk_dup_top(ctx);
    duk_put_prop_string(ctx, -3, key);
    duk_push_object(ctx); // ctor, prototype
    if (dtor != NULL) {
        duk_push_c_function(ctx, dtor, 1); // ctor, prototype, finalizer
        duk_set_finalizer(ctx, -2); // ctor, prototype
    }
    duk_dup_top(ctx); // ctor, prototype, prototype
    duk_put_prop_string(ctx, -3, "prototype"); // ctor, prototype    
}

DUK_INTERNAL void duk_unity_add_member(duk_context *ctx, const char *name, duk_c_function fn, duk_idx_t idx) {
    idx = duk_normalize_index(ctx, idx);
    duk_push_c_function(ctx, fn, DUK_VARARGS);
    duk_put_prop_string(ctx, idx, name);
}

DUK_INTERNAL void duk_unity_add_property(duk_context *ctx, const char *name, duk_c_function getter, duk_c_function setter, duk_idx_t idx) {
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

DUK_INTERNAL void duk_unity_end_class(duk_context *ctx) {
    duk_pop_2(ctx);
}

DUK_INTERNAL void duk_unity_add_const_number(duk_context *ctx, duk_idx_t idx, const char *key, duk_double_t num) {
    idx = duk_normalize_index(ctx, idx);
    duk_push_number(ctx, num);
    duk_put_prop_string(ctx, idx, key);
}

DUK_INTERNAL void duk_unity_add_const_int(duk_context *ctx, duk_idx_t idx, const char *key, duk_int_t num) {
    idx = duk_normalize_index(ctx, idx);
    duk_push_int(ctx, num);
    duk_put_prop_string(ctx, idx, key);
}

/*
 *  unity helpers
 */

DUK_EXTERNAL void duk_unity_error_raw(duk_hthread *thr, duk_errcode_t err_code, const char *filename, duk_int_t line, const char *fmt) {
    duk_error_raw(thr, err_code, filename, line, "%s", fmt); // no plain error call ...
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

DUK_EXTERNAL void duk_unity_put2i(duk_context *ctx, duk_idx_t idx, duk_int_t v1, duk_int_t v2) {
    // duk_push_array(ctx);
    idx = duk_normalize_index(ctx, idx);
    duk_push_int(ctx, v1);
    duk_put_prop_index(ctx, idx, 0);
    duk_push_int(ctx, v2);
    duk_put_prop_index(ctx, idx, 1);
}

DUK_EXTERNAL void duk_unity_put3i(duk_context *ctx, duk_idx_t idx, duk_int_t v1, duk_int_t v2, duk_int_t v3) {
    // duk_push_array(ctx);
    idx = duk_normalize_index(ctx, idx);
    duk_push_int(ctx, v1);
    duk_put_prop_index(ctx, idx, 0);
    duk_push_int(ctx, v2);
    duk_put_prop_index(ctx, idx, 1);
    duk_push_int(ctx, v3);
    duk_put_prop_index(ctx, idx, 2);
}

DUK_EXTERNAL void duk_unity_put4i(duk_context *ctx, duk_idx_t idx, duk_int_t v1, duk_int_t v2, duk_int_t v3, duk_int_t v4) {
    // duk_push_array(ctx);
    idx = duk_normalize_index(ctx, idx);
    duk_push_int(ctx, v1);
    duk_put_prop_index(ctx, idx, 0);
    duk_push_int(ctx, v2);
    duk_put_prop_index(ctx, idx, 1);
    duk_push_int(ctx, v3);
    duk_put_prop_index(ctx, idx, 2);
    duk_push_int(ctx, v4);
    duk_put_prop_index(ctx, idx, 3);
}

// float

DUK_EXTERNAL void duk_unity_put2f(duk_context *ctx, duk_idx_t idx, float v1, float v2) {
    // duk_push_array(ctx);
    idx = duk_normalize_index(ctx, idx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, idx, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, idx, 1);
}

DUK_EXTERNAL void duk_unity_put3f(duk_context *ctx, duk_idx_t idx, float v1, float v2, float v3) {
    // duk_push_array(ctx);
    idx = duk_normalize_index(ctx, idx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, idx, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, idx, 1);
    duk_push_number(ctx, v3);
    duk_put_prop_index(ctx, idx, 2);
}

DUK_EXTERNAL void duk_unity_put4f(duk_context *ctx, duk_idx_t idx, float v1, float v2, float v3, float v4) {
    // duk_push_array(ctx);
    idx = duk_normalize_index(ctx, idx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, idx, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, idx, 1);
    duk_push_number(ctx, v3);
    duk_put_prop_index(ctx, idx, 2);
    duk_push_number(ctx, v4);
    duk_put_prop_index(ctx, idx, 3);
}

DUK_EXTERNAL void duk_unity_put2d(duk_context *ctx, duk_idx_t idx, double v1, double v2) {
    // duk_push_array(ctx);
    idx = duk_normalize_index(ctx, idx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, idx, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, idx, 1);
}

DUK_EXTERNAL void duk_unity_put3d(duk_context *ctx, duk_idx_t idx, double v1, double v2, double v3) {
    // duk_push_array(ctx);
    idx = duk_normalize_index(ctx, idx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, idx, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, idx, 1);
    duk_push_number(ctx, v3);
    duk_put_prop_index(ctx, idx, 2);
}

DUK_EXTERNAL void duk_unity_put4d(duk_context *ctx, duk_idx_t idx, double v1, double v2, double v3, double v4) {
    // duk_push_array(ctx);
    idx = duk_normalize_index(ctx, idx);
    duk_push_number(ctx, v1);
    duk_put_prop_index(ctx, idx, 0);
    duk_push_number(ctx, v2);
    duk_put_prop_index(ctx, idx, 1);
    duk_push_number(ctx, v3);
    duk_put_prop_index(ctx, idx, 2);
    duk_push_number(ctx, v4);
    duk_put_prop_index(ctx, idx, 3);
}

DUK_EXTERNAL duk_bool_t duk_unity_get2f(duk_context *ctx, duk_idx_t idx, float *v1, float *v2) {
    idx = duk_normalize_index(ctx, idx);
    // /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = (float)duk_get_number_default(ctx, -1, 0.0);
        }
        duk_pop_2(ctx);
        return 1;
    // }
    // return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get2d(duk_context *ctx, duk_idx_t idx, double *v1, double *v2) {
    idx = duk_normalize_index(ctx, idx);
    // /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = (double)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = (double)duk_get_number_default(ctx, -1, 0.0);
        }
        duk_pop_2(ctx);
        return 1;
    // }
    // return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get3f(duk_context *ctx, duk_idx_t idx, float *v1, float *v2, float *v3) {
    idx = duk_normalize_index(ctx, idx);
    // /*if (duk_is_array(ctx, idx))*/ {
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
    // }
    // return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get3d(duk_context *ctx, duk_idx_t idx, double *v1, double *v2, double *v3) {
    idx = duk_normalize_index(ctx, idx);
    // /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = (double)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = (double)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 2)) {
            *v3 = (double)duk_get_number_default(ctx, -1, 0.0);
        }
        duk_pop_3(ctx);
        return 1;
    // }
    // return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get4f(duk_context *ctx, duk_idx_t idx, float *v1, float *v2, float *v3, float *v4) {
    idx = duk_normalize_index(ctx, idx);
    // /*if (duk_is_array(ctx, idx))*/ {
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
    // }
    // return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get4d(duk_context *ctx, duk_idx_t idx, double *v1, double *v2, double *v3, double *v4) {
    idx = duk_normalize_index(ctx, idx);
    // /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = (double)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = (double)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 2)) {
            *v3 = (double)duk_get_number_default(ctx, -1, 0.0);
        }
        if (duk_get_prop_index(ctx, idx, 3)) {
            *v4 = (double)duk_get_number_default(ctx, -1, 0.0);
        }
        duk_pop_n(ctx, 4);
        return 1;
    // }
    // return 0;
}

// int

DUK_EXTERNAL duk_bool_t duk_unity_get2i(duk_context *ctx, duk_idx_t idx, duk_int_t *v1, duk_int_t *v2) {
    idx = duk_normalize_index(ctx, idx);
    // /*if (duk_is_array(ctx, idx))*/ {
        if (duk_get_prop_index(ctx, idx, 0)) {
            *v1 = duk_get_int_default(ctx, -1, 0);
        }
        if (duk_get_prop_index(ctx, idx, 1)) {
            *v2 = duk_get_int_default(ctx, -1, 0);
        }
        duk_pop_2(ctx);
        return 1;
    // }
    // return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get3i(duk_context *ctx, duk_idx_t idx, duk_int_t *v1, duk_int_t *v2, duk_int_t *v3) {
    idx = duk_normalize_index(ctx, idx);
    // /*if (duk_is_array(ctx, idx))*/ {
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
    // }
    // return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_get4i(duk_context *ctx, duk_idx_t idx, duk_int_t *v1, duk_int_t *v2, duk_int_t *v3, duk_int_t *v4) {
    idx = duk_normalize_index(ctx, idx);
    // /*if (duk_is_array(ctx, idx))*/ {
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
    // }
    // return 0;
}

DUK_EXTERNAL void duk_unity_put4x4f(duk_context *ctx, duk_idx_t idx, const float *c0, const float *c1, const float *c2, const float *c3) {
    idx = duk_normalize_index(ctx, idx);

    duk_push_number(ctx, c0[0]);
    duk_put_prop_index(ctx, idx, 0);
    duk_push_number(ctx, c0[1]);
    duk_put_prop_index(ctx, idx, 1);
    duk_push_number(ctx, c0[2]);
    duk_put_prop_index(ctx, idx, 2);
    duk_push_number(ctx, c0[3]);
    duk_put_prop_index(ctx, idx, 3);

    duk_push_number(ctx, c1[0]);
    duk_put_prop_index(ctx, idx, 4);
    duk_push_number(ctx, c1[1]);
    duk_put_prop_index(ctx, idx, 5);
    duk_push_number(ctx, c1[2]);
    duk_put_prop_index(ctx, idx, 6);
    duk_push_number(ctx, c1[3]);
    duk_put_prop_index(ctx, idx, 7);

    duk_push_number(ctx, c2[0]);
    duk_put_prop_index(ctx, idx, 8);
    duk_push_number(ctx, c2[1]);
    duk_put_prop_index(ctx, idx, 9);
    duk_push_number(ctx, c2[2]);
    duk_put_prop_index(ctx, idx, 10);
    duk_push_number(ctx, c2[3]);
    duk_put_prop_index(ctx, idx, 11);

    duk_push_number(ctx, c3[0]);
    duk_put_prop_index(ctx, idx, 12);
    duk_push_number(ctx, c3[1]);
    duk_put_prop_index(ctx, idx, 13);
    duk_push_number(ctx, c3[2]);
    duk_put_prop_index(ctx, idx, 14);
    duk_push_number(ctx, c3[3]);
    duk_put_prop_index(ctx, idx, 15);
}

DUK_EXTERNAL duk_bool_t duk_unity_get4x4f(duk_context *ctx, duk_idx_t idx, float *c0, float *c1, float *c2, float *c3) {
    idx = duk_normalize_index(ctx, idx);

    if (duk_get_prop_index(ctx, idx, 0)) {
        c0[0] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 1)) {
        c0[1] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 2)) {
        c0[2] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 3)) {
        c0[3] = (float)duk_get_number_default(ctx, -1, 0);
    }

    if (duk_get_prop_index(ctx, idx, 4)) {
        c1[0] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 5)) {
        c1[1] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 6)) {
        c1[2] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 7)) {
        c1[3] = (float)duk_get_number_default(ctx, -1, 0);
    }

    if (duk_get_prop_index(ctx, idx, 8)) {
        c2[0] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 9)) {
        c2[1] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 10)) {
        c2[2] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 11)) {
        c2[3] = (float)duk_get_number_default(ctx, -1, 0);
    }

    if (duk_get_prop_index(ctx, idx, 12)) {
        c3[0] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 13)) {
        c3[1] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 14)) {
        c3[2] = (float)duk_get_number_default(ctx, -1, 0);
    }
    if (duk_get_prop_index(ctx, idx, 15)) {
        c3[3] = (float)duk_get_number_default(ctx, -1, 0);
    }
    duk_pop_n(ctx, 16);
    return 1;
}

DUK_EXTERNAL duk_bool_t duk_unity_set_prop_i(duk_context *ctx, duk_idx_t idx, const char *key, duk_int_t val) {
    idx = duk_normalize_index(ctx, idx);
    duk_push_int(ctx, val);
    return duk_put_prop_string(ctx, idx, key);
}

DUK_LOCAL void duk_refsys_open(duk_context *ctx) {
    duk_push_heap_stash(ctx); // [stash]
    
    duk_push_array(ctx); // [stash, array]
    duk_dup_top(ctx); // [stash, array, array]
    duk_put_prop_index(ctx, -3, DUK_UNITY_STASH_REGISTRY); //duk_put_prop_string(ctx, -3, "c_registry"); // [stash, array]
    duk_push_int(ctx, 0); // [stash, array, 0]
    duk_put_prop_index(ctx, -2, 0); // [stash, array]
    duk_pop(ctx); // [stash]

    duk_push_object(ctx);
    duk_put_prop_index(ctx, -2, DUK_UNITY_STASH_BUILTINS);//duk_put_prop_string(ctx, -2, "c_builtins"); // [stash, builtins]

    duk_pop(ctx); // .
}

/// Creates and returns a reference for the object at the top of the stack (and pops the object).
DUK_EXTERNAL duk_uint_t duk_unity_ref(duk_context *ctx) {
    if (duk_is_null_or_undefined(ctx, -1)) {
        return 0;
    }
    duk_push_heap_stash(ctx); // obj, stash
    duk_get_prop_index(ctx, -1, DUK_UNITY_STASH_REGISTRY); //duk_get_prop_string(ctx, -1, "c_registry"); // obj, stash, array
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
    duk_get_prop_index(ctx, -1, DUK_UNITY_STASH_REGISTRY); //duk_get_prop_string(ctx, -1, "c_registry"); // stash, array
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
    duk_get_prop_index(ctx, -1, DUK_UNITY_STASH_REGISTRY); //duk_get_prop_string(ctx, -1, "c_registry"); // stash, array
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

DUK_EXTERNAL duk_bool_t duk_unity_put_target_i(duk_context *ctx, duk_idx_t idx) {
    if (!duk_is_null_or_undefined(ctx, idx)) {
        return duk_put_prop_string(ctx, idx, "target");
    } 
    duk_pop(ctx);
    return 0;
}

/**************************
 * memory allocation
 **************************/

struct duk_unity_memory_state {
    duk_size_t malloc_count; 
    duk_size_t malloc_size;
};

typedef struct {
	/* The double value in the union is there to ensure alignment is
	 * good for IEEE doubles too.  In many 32-bit environments 4 bytes
	 * would be sufficiently aligned and the double value is unnecessary.
	 */
	union {
		size_t sz;
		double d;
	} u;
} alloc_hdr;

// DUK_INTERNAL DUK_INLINE duk_size_t duk_unity_malloc_size(void *ptr) {
// #if defined(__APPLE__)
//     return malloc_size(ptr);
// #elif defined(_WIN32)
//     return _msize(ptr);
// #elif defined(__linux__)
//     return malloc_usable_size(ptr);
// #else
//     return 0;
// #endif
// }

DUK_EXTERNAL void duk_unity_get_memory_state(duk_hthread *thr, duk_uint_t *malloc_count, duk_uint_t *malloc_size) {
    if (thr) {
        duk_heap *heap = thr->heap;
        if (heap) {
            struct duk_unity_memory_state *state = (struct duk_unity_memory_state *)(heap->heap_udata);
            if (state) {
                if (malloc_count) {
                    *malloc_count = (duk_uint_t)(state->malloc_count);
                }
                if (malloc_size) {
                    *malloc_size = (duk_uint_t)(state->malloc_size);
                }
            }
        }
    }
}

DUK_INTERNAL void *duk_unity_default_alloc_function(void *udata, duk_size_t size) {
    alloc_hdr *hdr;
	void *res;
	struct duk_unity_memory_state *state = (struct duk_unity_memory_state *)udata;
	hdr = (alloc_hdr *)malloc(size + sizeof(alloc_hdr));
    if (!hdr) {
        return NULL;
    }
    if (state) {
        state->malloc_count++;
        state->malloc_size += size;
    }
    hdr->u.sz = size;
    res = (void*)(hdr + 1);
	return res;
}

DUK_INTERNAL void *duk_unity_default_realloc_function(void *udata, void *ptr, duk_size_t size) {
	alloc_hdr *hdr;
	size_t old_size;
	void *t;
	void *ret;

	struct duk_unity_memory_state *state = (struct duk_unity_memory_state *)udata;

	/* Handle the ptr-NULL vs. size-zero cases explicitly to minimize
	 * platform assumptions.  You can get away with much less in specific
	 * well-behaving environments.
	 */

	if (ptr) {
		hdr = (alloc_hdr *) (void *) ((unsigned char *) ptr - sizeof(alloc_hdr));
		old_size = hdr->u.sz;

        if (state) {
            state->malloc_count--;
            state->malloc_size -= old_size;
        }
		if (size == 0) {
			free((void *) hdr);
			return NULL;
		} else {
			t = realloc((void *) hdr, size + sizeof(alloc_hdr));
			if (!t) {
				return NULL;
			}
			hdr = (alloc_hdr *) t;
			hdr->u.sz = size;
			ret = (void *) (hdr + 1);
            if (state) {
                state->malloc_count++;
                state->malloc_size += size;
            }
			return ret;
		}
	} else {
		if (size == 0) {
			return NULL;
		} else {
			hdr = (alloc_hdr *) malloc(size + sizeof(alloc_hdr));
			if (!hdr) {
				return NULL;
			}
            if (state) {
                state->malloc_count++;
                state->malloc_size += size;
            }
			hdr->u.sz = size;
			ret = (void *) (hdr + 1);
			return ret;
		}
	}
}

DUK_INTERNAL void duk_unity_default_free_function(void *udata, void *ptr) {
    alloc_hdr *hdr;
	struct duk_unity_memory_state *state = (struct duk_unity_memory_state *)udata;
	if (!ptr) {
		return;
	}
	hdr = (alloc_hdr *) (void *) ((unsigned char *) ptr - sizeof(alloc_hdr));
    if (state) {
        state->malloc_count--;
        state->malloc_size -= hdr->u.sz;
    }
	free((void *) hdr);
}

DUK_EXTERNAL duk_hthread *duk_unity_create_heap() {
    struct duk_unity_memory_state *state = malloc(sizeof(struct duk_unity_memory_state));
    if (state) {
        // memset(state, 0, sizeof(struct duk_unity_memory_state));
        state->malloc_count = 0;
        state->malloc_size = 0;
    }
    return duk_create_heap(
        duk_unity_default_alloc_function, 
        duk_unity_default_realloc_function, 
        duk_unity_default_free_function, 
        state, 
        NULL);
}

DUK_EXTERNAL void duk_unity_destroy_heap(duk_hthread *thr) {
    duk_heap *heap;
    if (!thr) {
        return;
    }
    heap = thr->heap;
	struct duk_unity_memory_state *state = (struct duk_unity_memory_state *)heap->heap_udata;
    if (state) {
        free(state);
        heap->heap_udata = NULL;
    }
    duk_destroy_heap(thr);
}

DUK_EXTERNAL duk_int_t duk_unity_get_refid(duk_context *ctx, duk_idx_t idx) {
    duk_int_t refid = -1;
    if (!duk_is_valid_index(ctx, idx) || duk_is_null_or_undefined(ctx, idx)) {
        return refid;
    }
    if (duk_get_prop_literal(ctx, idx, DUK_HIDDEN_SYMBOL("!ref"))) {
        if (duk_is_number(ctx, -1)) {
            refid = duk_get_int_default(ctx, -1, -1);
            duk_pop(ctx);
            return refid;
        }
    }
    duk_pop(ctx);
    return refid;
}

DUK_EXTERNAL duk_bool_t duk_unity_set_refid(duk_context *ctx, duk_idx_t idx, duk_int_t refid) {
    if (!duk_is_valid_index(ctx, idx) || duk_is_null_or_undefined(ctx, idx)) {
        return 0;
    }
    idx = duk_normalize_index(ctx, idx);
    duk_push_int(ctx, refid);
    return duk_put_prop_literal(ctx, idx, DUK_HIDDEN_SYMBOL("!ref"));
}

DUK_EXTERNAL duk_int_t duk_unity_get_type_refid(duk_context *ctx, duk_idx_t idx) {
    duk_int_t refid = -1;
    if (!duk_is_valid_index(ctx, idx) || duk_is_null_or_undefined(ctx, idx)) {
        return refid;
    }
    if (duk_get_prop_literal(ctx, idx, DUK_HIDDEN_SYMBOL("!type"))) {
        if (duk_is_number(ctx, -1)) {
            refid = duk_get_int_default(ctx, -1, -1);
            duk_pop(ctx);
            return refid;
        }
    }
    duk_pop(ctx);
    return refid;
}

DUK_EXTERNAL duk_bool_t duk_unity_set_type_refid(duk_context *ctx, duk_idx_t idx, duk_int_t refid) {
    if (!duk_is_valid_index(ctx, idx) || duk_is_null_or_undefined(ctx, idx)) {
        return 0;
    }
    idx = duk_normalize_index(ctx, idx);
    duk_push_int(ctx, refid);
    return duk_put_prop_literal(ctx, idx, DUK_HIDDEN_SYMBOL("!type"));
}

DUK_EXTERNAL duk_bool_t duk_unity_get_weak_refid(duk_context *ctx, duk_idx_t idx, duk_int_t *refid) {
    if (refid) {
        if (duk_get_prop_literal(ctx, idx, DUK_HIDDEN_SYMBOL("!weak"))) {
            *refid = duk_get_int(ctx, -1);
            duk_pop(ctx);
            return 1;
        }
        duk_pop(ctx);
    }
    return 0;
}

DUK_EXTERNAL duk_bool_t duk_unity_set_weak_refid(duk_context *ctx, duk_idx_t idx, duk_int_t refid) {
    idx = duk_normalize_index(ctx, idx);
    duk_push_int(ctx, refid);
    return duk_put_prop_literal(ctx, idx, DUK_HIDDEN_SYMBOL("!weak"));
}

DUK_EXTERNAL duk_ret_t duk_unity_thread_resume(duk_context *ctx) {
    return duk_bi_thread_resume(ctx);
}

DUK_EXTERNAL duk_int_t duk_unity_thread_state(duk_context *ctx) {
    return (duk_int_t) (ctx->state);
}

DUK_EXTERNAL duk_bool_t duk_unity_open(duk_context *ctx) {
    duk_refsys_open(ctx);
    // duk_rws_open(ctx);
    duk_events_open(ctx);
    duk_unity_valuetypes_open(ctx);
    duk_sock_open(ctx);
    duk_websocket_open(ctx);
    duk_fmath_open(ctx);
    duk_timer_open(ctx);
    return 1;
}

// DUK_EXTERNAL void duk_unity_suspend(duk_hthread *thr) {
//     duk_suspend(thr, &thr->__state);
// }

// DUK_EXTERNAL void duk_unity_resume(duk_hthread *thr) {
//     duk_resume(thr, &thr->__state);
// }
