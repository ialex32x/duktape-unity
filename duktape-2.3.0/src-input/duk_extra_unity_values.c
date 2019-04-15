#include "duk_internal.h"

/**
 * Vector3
 */
#define UNITY_VECTOR3_kEpsilon 0.00001F
#define UNITY_SINGLE_Epsilon 1.401298E-45F
#define UNITY_VECTOR3_kEpsilonNormalSqrt 1e-15F
#define UNITY_VECTOR3_k1OverSqrt2 0.7071067811865475244008443621048490F
#define UNITY_MATH_PI 3.1415926535897932384626433832795028841971693993F
#define UNITY_DEG2RAD 0.017453292519943295F
#define UNITY_RAD2DEG 57.29577951308232F

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

DUK_INTERNAL DUK_INLINE float float_clamped(float a, float b, float t) {
    float d = b - a;
    return d > 0.0F ? a + fminf(d, t) : a - fminf(-d, t);
}

DUK_INTERNAL DUK_INLINE void vec2_sub(const float* a, const float* b, float* res) {
    res[0] = a[0] - b[0];
    res[1] = a[1] - b[1];
}

DUK_INTERNAL DUK_INLINE float vec2_dot(const float* a, const float* b) {
    return a[0] * b[0] + a[1] * b[1];
}

DUK_INTERNAL DUK_INLINE float vec2_angle(const float* from, const float* to) {
    float denominator = sqrtf((from[0] * from[0] + from[1] * from[1]) * (to[0] * to[0] + to[1] * to[1]));
    if (denominator < UNITY_VECTOR3_kEpsilonNormalSqrt) {
        return 0;
    } 
    float dot = vec2_dot(from, to) / denominator;
    if (dot > 1) {
        dot = 1;
    } else if (dot < -1) {
        dot = -1;
    }
    return acosf(dot) * UNITY_RAD2DEG;
}

DUK_INTERNAL DUK_INLINE float vec2_magnitude(const float* a) {
    return sqrtf(a[0] * a[0] + a[1] * a[1]);
}

DUK_INTERNAL DUK_INLINE void vec2_move_towards(const float* current, const float* target, float maxDistanceDelta, float* res) {
    vec2_sub(target, current, res);
    float dist = vec2_magnitude(res);
    if (dist <= maxDistanceDelta || dist == 0) {
        res[0] = target[0];
        res[1] = target[1];
    } else {
        res[0] = current[0] + res[0] / dist * maxDistanceDelta; 
        res[1] = current[1] + res[1] / dist * maxDistanceDelta;
    }
}

DUK_INTERNAL DUK_INLINE void vec2_push_new(duk_context *ctx, float x, float y) {
    duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_VECTOR2);
    duk_push_number(ctx, x);
    duk_push_number(ctx, y);
    duk_new(ctx, 2);
}

DUK_INTERNAL DUK_INLINE void vec2i_push_new(duk_context *ctx, duk_int_t x, duk_int_t y) {
    duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_VECTOR2I);
    duk_push_int(ctx, x);
    duk_push_int(ctx, y);
    duk_new(ctx, 2);
}

DUK_INTERNAL DUK_INLINE void vec3_push_new(duk_context *ctx, float x, float y, float z) {
    duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_VECTOR3);
    duk_push_number(ctx, x);
    duk_push_number(ctx, y);
    duk_push_number(ctx, z);
    duk_new(ctx, 3);
}

DUK_INTERNAL DUK_INLINE void vec3i_push_new(duk_context *ctx, duk_int_t x, duk_int_t y, duk_int_t z) {
    duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_VECTOR3I);
    duk_push_int(ctx, x);
    duk_push_int(ctx, y);
    duk_push_int(ctx, z);
    duk_new(ctx, 3);
}

DUK_INTERNAL DUK_INLINE void vec4_push_new(duk_context *ctx, float x, float y, float z, float w) {
    duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_VECTOR4);
    duk_push_number(ctx, x);
    duk_push_number(ctx, y);
    duk_push_number(ctx, z);
    duk_push_number(ctx, w);
    duk_new(ctx, 4);
}

DUK_INTERNAL DUK_INLINE void quaternion_push_new(duk_context *ctx, float x, float y, float z, float w) {
    duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_QUATERNION);
    duk_push_number(ctx, x);
    duk_push_number(ctx, y);
    duk_push_number(ctx, z);
    duk_push_number(ctx, w);
    duk_new(ctx, 4);
}

DUK_INTERNAL DUK_INLINE void color_push_new(duk_context *ctx, float r, float g, float b, float a) {
    duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_COLOR);
    duk_push_number(ctx, r);
    duk_push_number(ctx, g);
    duk_push_number(ctx, b);
    duk_push_number(ctx, a);
    duk_new(ctx, 4);
}

DUK_INTERNAL DUK_INLINE void color32_push_new(duk_context *ctx, unsigned char r, unsigned char g, unsigned char b, unsigned char a) {
    duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_COLOR32);
    duk_push_int(ctx, r);
    duk_push_int(ctx, g);
    duk_push_int(ctx, b);
    duk_push_int(ctx, a);
    duk_new(ctx, 4);
}

DUK_INTERNAL DUK_INLINE void matrix4x4_push_new(duk_context *ctx, const float *c0, const float *c1, const float *c2, const float *c3) {
    duk_builtins_reg_get(ctx, DUK_UNITY_BUILTINS_MATRIX44);
    if (c0) {
        vec4_push_new(ctx, c0[0], c0[1], c0[2], c0[3]);
    } else {
        vec4_push_new(ctx, 0, 0, 0, 0);
    }
    if (c1) {
        vec4_push_new(ctx, c1[0], c1[1], c1[2], c1[3]);
    } else {
        vec4_push_new(ctx, 0, 0, 0, 0);
    }
    if (c2) {
        vec4_push_new(ctx, c2[0], c2[1], c2[2], c2[3]);
    } else {
        vec4_push_new(ctx, 0, 0, 0, 0);
    }
    if (c3) {
        vec4_push_new(ctx, c3[0], c3[1], c3[2], c3[3]);
    } else {
        vec4_push_new(ctx, 0, 0, 0, 0);
    }
    duk_new(ctx, 4);
}

DUK_INTERNAL DUK_INLINE float vec3_magnitude(const float* lhs) {
    return sqrtf(lhs[0] * lhs[0] + lhs[1] * lhs[1] + lhs[2] * lhs[2]);
}

DUK_INTERNAL DUK_INLINE float vec3_dot(const float* lhs, const float* rhs) {
    return lhs[0] * rhs[0] + lhs[1] * rhs[1] + lhs[2] * rhs[2];
}

DUK_INTERNAL DUK_INLINE void vec3_normalize(float* lhs) {
    float mag = sqrtf(lhs[0] * lhs[0] + lhs[1] * lhs[1] + lhs[2] * lhs[2]);
    lhs[0] /= mag;
    lhs[1] /= mag;
    lhs[2] /= mag;
}

DUK_INTERNAL DUK_INLINE void vec3_cross(const float* lhs, const float* rhs, float* res) {
    res[0] = lhs[1] * rhs[2] - lhs[2] * rhs[1];
    res[1] = lhs[2] * rhs[0] - lhs[0] * rhs[2];
    res[2] = lhs[0] * rhs[1] - lhs[1] * rhs[0];
}

DUK_INTERNAL DUK_INLINE void vec3_multiply(float* out_vec3, float a) {
    out_vec3[0] *= a;
    out_vec3[1] *= a;
    out_vec3[2] *= a;
}

DUK_INTERNAL DUK_INLINE void vec3_move_towards(const float *lhs, const float *rhs, float maxDistanceDelta, float *res) {
    float to[3];
    to[0] = rhs[0] - lhs[0];
    to[1] = rhs[1] - lhs[1];
    to[2] = rhs[2] - lhs[2];
    float dist = sqrtf(to[0] * to[0] + to[1] * to[1] + to[2] * to[2]);
    if (dist < maxDistanceDelta || dist < UNITY_SINGLE_Epsilon) {
        memcpy(res, rhs, sizeof(float) * 3);
        return;
    }
    res[0] = lhs[0] + to[0] / dist * maxDistanceDelta;
    res[1] = lhs[1] + to[1] / dist * maxDistanceDelta;
    res[2] = lhs[2] + to[2] / dist * maxDistanceDelta;
}

DUK_INTERNAL DUK_INLINE void m3x3_multiply_vec3(float* mat3x3, const float* vec3, float* out_vec3) {
	out_vec3[0] = mat3x3[0] * vec3[0] + mat3x3[3] * vec3[1] + mat3x3[6] * vec3[2];
	out_vec3[1] = mat3x3[1] * vec3[0] + mat3x3[4] * vec3[1] + mat3x3[7] * vec3[2];
	out_vec3[2] = mat3x3[2] * vec3[0] + mat3x3[5] * vec3[1] + mat3x3[8] * vec3[2];
}

DUK_INTERNAL DUK_INLINE void m3x3_set_axis_angle(float* mat3x3, const float* vec, float radians) {
    /* This function contributed by Erich Boleyn (erich@uruk.org) */
    /* This function used from the Mesa OpenGL code (matrix.c)  */
    float s, c;
    float vx, vy, vz, xx, yy, zz, xy, yz, zx, xs, ys, zs, one_c;

    s = sinf(radians);
    c = cosf(radians);

    vx = vec[0];
    vy = vec[1];
    vz = vec[2];

    xx = vx * vx;
    yy = vy * vy;
    zz = vz * vz;
    xy = vx * vy;
    yz = vy * vz;
    zx = vz * vx;
    xs = vx * s;
    ys = vy * s;
    zs = vz * s;
    one_c = 1.0F - c;

    mat3x3[0*3+0] = (one_c * xx) + c;
    mat3x3[1*3+0] = (one_c * xy) - zs;
    mat3x3[2*3+0] = (one_c * zx) + ys;

    mat3x3[0*3+1] = (one_c * xy) + zs;
    mat3x3[1*3+1] = (one_c * yy) + c;
    mat3x3[2*3+1] = (one_c * yz) - xs;

    mat3x3[0*3+2] = (one_c * zx) - ys;
    mat3x3[1*3+2] = (one_c * yz) + xs;
    mat3x3[2*3+2] = (one_c * zz) + c;
}

// UnityCG.cginc
DUK_INTERNAL DUK_INLINE float unitycg_LinearToGammaSpaceExact(float value) {
	if (value <= 0.0F)
		return 0.0F;
	else if (value <= 0.0031308F)
		return 12.92F * value;
	else if (value < 1.0F)
		return 1.055F * powf(value, 0.4166667F) - 0.055F;
	else
		return powf(value, 0.45454545F);
}

DUK_INTERNAL DUK_INLINE float unitycg_GammaToLinearSpaceExact(float value) {
	if (value <= 0.04045F)
		return value / 12.92F;
	else if (value < 1.0F)
		return powf((value + 0.055F)/1.055F, 2.4F);
	else
		return powf(value, 2.2F);
}


DUK_LOCAL void duk_unity_Vector2_add_const(duk_context *ctx, duk_idx_t idx, const char *key, float x, float y) {
    idx = duk_normalize_index(ctx, idx);
    vec2_push_new(ctx, x, y);
    duk_put_prop_string(ctx, idx, key);
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_constructor(duk_context *ctx) {
    float c0[4];
    float c1[4];
    float c2[4];
    float c3[4];
    duk_unity_get4f(ctx, 0, &c0[0], &c0[1], &c0[2], &c0[3]);
    duk_unity_get4f(ctx, 1, &c1[0], &c1[1], &c1[2], &c1[3]);
    duk_unity_get4f(ctx, 2, &c2[0], &c2[1], &c2[2], &c2[3]);
    duk_unity_get4f(ctx, 3, &c3[0], &c3[1], &c3[2], &c3[3]);
    duk_push_this(ctx);
    duk_unity_put4x4f(ctx, -1, c0, c1, c2, c3);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL void duk_unity_Matrix4x4_add_const(duk_context *ctx, duk_idx_t idx, const char *key, const float *c0, const float *c1, const float *c2, const float *c3) {
    idx = duk_normalize_index(ctx, idx);
    matrix4x4_push_new(ctx, c0, c1, c2, c3);
    duk_put_prop_string(ctx, idx, key);
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m00(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 0);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m00(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 0);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m10(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 1);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m10(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 1);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m20(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m20(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 2);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m30(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 3);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m30(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 3);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m01(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 4);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m01(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 4);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m11(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 5);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m11(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 5);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m21(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 6);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m21(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 6);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m31(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 7);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m31(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 7);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m02(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 8);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m02(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 8);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m12(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 9);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m12(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 9);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m22(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 10);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m22(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 10);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m32(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 11);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m32(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 11);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m03(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 12);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m03(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 12);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m13(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 13);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m13(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 13);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m23(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 14);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m23(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 14);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_get_m33(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 15);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Matrix4x4_set_m33(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 15);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2Int_constructor(duk_context *ctx) {
    duk_int_t x = duk_get_int_default(ctx, 0, 0);
    duk_int_t y = duk_get_int_default(ctx, 1, 0);
    duk_push_this(ctx);
    duk_unity_put2i(ctx, -1, x, y);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3Int_constructor(duk_context *ctx) {
    duk_int_t x = duk_get_int_default(ctx, 0, 0);
    duk_int_t y = duk_get_int_default(ctx, 1, 0);
    duk_int_t z = duk_get_int_default(ctx, 2, 0);
    duk_push_this(ctx);
    duk_unity_put3i(ctx, -1, x, y, z);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector4_constructor(duk_context *ctx) {
    float x = (float)duk_get_number_default(ctx, 0, 0.0);
    float y = (float)duk_get_number_default(ctx, 1, 0.0);
    float z = (float)duk_get_number_default(ctx, 2, 0.0);
    float w = (float)duk_get_number_default(ctx, 3, 0.0);
    duk_push_this(ctx);
    duk_unity_put4f(ctx, -1, x, y, z, w);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector4_getx(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 0);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector4_setx(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 0);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector4_gety(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 1);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector4_sety(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 1);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector4_getz(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector4_setz(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 2);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector4_getw(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 3);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector4_setw(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 3);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color_constructor(duk_context *ctx) {
    float r = (float)duk_get_number_default(ctx, 0, 0.0);
    float g = (float)duk_get_number_default(ctx, 1, 0.0);
    float b = (float)duk_get_number_default(ctx, 2, 0.0);
    float a = (float)duk_get_number_default(ctx, 3, 1.0);
    duk_push_this(ctx);
    duk_unity_put4f(ctx, -1, r, g, b, a);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color_static_Add(duk_context *ctx) {
    float a[4];
    float b[4];
    duk_unity_get4f(ctx, 0, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_get4f(ctx, 1, &b[0], &b[1], &b[2], &b[3]);
    color_push_new(ctx, a[0] + b[0], a[1] + b[1], a[2] + b[2], a[3] + b[3]);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_static_Sub(duk_context *ctx) {
    float a[4];
    float b[4];
    duk_unity_get4f(ctx, 0, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_get4f(ctx, 1, &b[0], &b[1], &b[2], &b[3]);
    color_push_new(ctx, a[0] - b[0], a[1] - b[1], a[2] - b[2], a[3] - b[3]);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_static_Neg(duk_context *ctx) {
    float a[4];
    duk_unity_get4f(ctx, 0, &a[0], &a[1], &a[2], &a[3]);
    color_push_new(ctx, -a[0], -a[1], -a[2], -a[3]);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_Inverse(duk_context *ctx) {
    float a[4];
    duk_push_this(ctx);
    duk_unity_get4f(ctx, -1, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_put4f(ctx, -1, 1.0F / a[0], 1.0F / a[1], 1.0F / a[2], 1.0F / a[3]);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color_static_Mul(duk_context *ctx) {
    float a[4];
    float b[4];
    duk_unity_get4f(ctx, 0, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_get4f(ctx, 1, &b[0], &b[1], &b[2], &b[3]);
    color_push_new(ctx, a[0] * b[0], a[1] * b[1], a[2] * b[2], a[3] * b[3]);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_static_Div(duk_context *ctx) {
    float a[4];
    float b[4];
    duk_unity_get4f(ctx, 0, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_get4f(ctx, 1, &b[0], &b[1], &b[2], &b[3]);
    color_push_new(ctx, a[0] / b[0], a[1] / b[1], a[2] / b[2], a[3] / b[3]);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_static_Equals(duk_context *ctx) {
    float a[4];
    float b[4];
    duk_unity_get4f(ctx, 0, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_get4f(ctx, 1, &b[0], &b[1], &b[2], &b[3]);
    duk_push_boolean(ctx, a[0] == b[0] && a[1] == b[1] && a[2] == b[2] && a[3] == b[3]);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_Equals(duk_context *ctx) {
    float a[4];
    float b[4];
    duk_push_this(ctx);
    duk_unity_get4f(ctx, -1, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_get4f(ctx, 0, &b[0], &b[1], &b[2], &b[3]);
    duk_pop(ctx);
    duk_push_boolean(ctx, a[0] == b[0] && a[1] == b[1] && a[2] == b[2] && a[3] == b[3]);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_ToString(duk_context *ctx) {
    float a[4];
    duk_push_this(ctx);
    duk_unity_get4f(ctx, -1, &a[0], &a[1], &a[2], &a[3]);
    duk_pop(ctx);
    duk_push_sprintf(ctx, "RGBA(%f, %f, %f, %f)", a[0], a[1], a[2], a[3]);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_static_Lerp(duk_context *ctx) {
    float a[4];
    float b[4];
    float t;
    duk_unity_get4f(ctx, 0, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_get4f(ctx, 1, &b[0], &b[1], &b[2], &b[3]);
    t = (float)duk_get_number_default(ctx, 2, 0);
    t = t > 1.0f ? 1.0f : (t < 0.0f ? 0.0f : t);
    color_push_new(ctx, a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t, a[2] + (b[2] - a[2]) * t, a[3] + (b[3] - a[3]) * t);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_static_LerpUnclamped(duk_context *ctx) {
    float a[4];
    float b[4];
    float t;
    duk_unity_get4f(ctx, 0, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_get4f(ctx, 1, &b[0], &b[1], &b[2], &b[3]);
    t = (float)duk_get_number_default(ctx, 2, 0);
    color_push_new(ctx, a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t, a[2] + (b[2] - a[2]) * t, a[3] + (b[3] - a[3]) * t);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_grayscale(duk_context *ctx) {
    float a[3];
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &a[0], &a[1], &a[2]);
    duk_pop(ctx);
    duk_push_number(ctx, 0.299F * a[0] + 0.587F * a[1] + 0.114F * a[2]);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_linear(duk_context *ctx) {
    float a[4];
    duk_push_this(ctx);
    duk_unity_get4f(ctx, -1, &a[0], &a[1], &a[2], &a[3]);
    duk_pop(ctx);
    color_push_new(ctx, 
        unitycg_GammaToLinearSpaceExact(a[0]),
        unitycg_GammaToLinearSpaceExact(a[1]),
        unitycg_GammaToLinearSpaceExact(a[2]), 
        a[4]
    );
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_gamma(duk_context *ctx) {
    float a[4];
    duk_push_this(ctx);
    duk_unity_get4f(ctx, -1, &a[0], &a[1], &a[2], &a[3]);
    duk_pop(ctx);
    color_push_new(ctx, 
        unitycg_LinearToGammaSpaceExact(a[0]),
        unitycg_LinearToGammaSpaceExact(a[1]),
        unitycg_LinearToGammaSpaceExact(a[2]), 
        a[4]
    );
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_maxColorComponent(duk_context *ctx) {
    float a[3];
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &a[0], &a[1], &a[2]);
    duk_pop(ctx);
    duk_push_number(ctx, fmax(fmax(a[0], a[1]), a[2]));
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_getr(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 0);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_setr(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 0);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color_getg(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 1);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_setg(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 1);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color_getb(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_setb(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 2);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color_geta(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 3);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color_seta(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 3);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_constructor(duk_context *ctx) {
    duk_int_t r = (unsigned char)duk_get_int_default(ctx, 0, 0);
    duk_int_t g = (unsigned char)duk_get_int_default(ctx, 1, 0);
    duk_int_t b = (unsigned char)duk_get_int_default(ctx, 2, 0);
    duk_int_t a = (unsigned char)duk_get_int_default(ctx, 3, 255);
    duk_push_this(ctx);
    duk_unity_put4i(ctx, -1, r, g, b, a);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_getr(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 0);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_setr(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 0);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_getg(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 1);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_setg(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 1);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_getb(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_setb(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 2);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_geta(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 3);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_seta(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 3);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_static_Lerp(duk_context *ctx) {
    duk_int_t a[4];
    duk_int_t b[4];
    float t;
    duk_unity_get4i(ctx, 0, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_get4i(ctx, 1, &b[0], &b[1], &b[2], &b[3]);
    t = (float)duk_get_number_default(ctx, 2, 0);
    t = t > 1.0f ? 1.0f : (t < 0.0f ? 0.0f : t);
    color32_push_new(ctx, 
        (unsigned char)(a[0] + (b[0] - a[0]) * t), 
        (unsigned char)(a[1] + (b[1] - a[1]) * t), 
        (unsigned char)(a[2] + (b[2] - a[2]) * t), 
        (unsigned char)(a[3] + (b[3] - a[3]) * t));
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Color32_static_LerpUnclamped(duk_context *ctx) {
    duk_int_t a[4];
    duk_int_t b[4];
    float t;
    duk_unity_get4i(ctx, 0, &a[0], &a[1], &a[2], &a[3]);
    duk_unity_get4i(ctx, 1, &b[0], &b[1], &b[2], &b[3]);
    t = (float)duk_get_number_default(ctx, 2, 0);
    color32_push_new(ctx, 
        (unsigned char)(a[0] + (b[0] - a[0]) * t), 
        (unsigned char)(a[1] + (b[1] - a[1]) * t), 
        (unsigned char)(a[2] + (b[2] - a[2]) * t), 
        (unsigned char)(a[3] + (b[3] - a[3]) * t));
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_constructor(duk_context *ctx) {
    float x = (float)duk_get_number_default(ctx, 0, 0.0);
    float y = (float)duk_get_number_default(ctx, 1, 0.0);
    float z = (float)duk_get_number_default(ctx, 2, 0.0);
    float w = (float)duk_get_number_default(ctx, 3, 0.0);
    duk_push_this(ctx);
    duk_unity_put4f(ctx, -1, x, y, z, w);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_getx(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 0);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_setx(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 0);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_gety(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 1);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_sety(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 1);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_getz(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_setz(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 2);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_getw(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 3);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_setw(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 3);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_Clone(duk_context *ctx) {
    float x, y, z, w;
    duk_push_this(ctx);
    duk_unity_get4f(ctx, -1, &x, &y, &z, &w);
    duk_pop(ctx);
    quaternion_push_new(ctx, x, y, z, w);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Quaternion_Set(duk_context *ctx) {
    float x = (float)duk_get_number_default(ctx, 0, 0.0);
    float y = (float)duk_get_number_default(ctx, 1, 0.0);
    float z = (float)duk_get_number_default(ctx, 2, 0.0);
    float w = (float)duk_get_number_default(ctx, 3, 0.0);
    duk_push_this(ctx);
    duk_unity_put4f(ctx, -1, x, y, z, w);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_constructor(duk_context *ctx) {
    float x = (float)duk_get_number_default(ctx, 0, 0.0);
    float y = (float)duk_get_number_default(ctx, 1, 0.0);
    duk_push_this(ctx);
    duk_unity_put2f(ctx, -1, x, y);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_Clone(duk_context *ctx) {
    float x, y;
    duk_push_this(ctx);
    duk_unity_get2f(ctx, -1, &x, &y);
    duk_pop(ctx);
    vec2_push_new(ctx, x, y);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_Set(duk_context *ctx) {
    float x, y;
    x = (float)duk_get_number_default(ctx, 0, 0);
    y = (float)duk_get_number_default(ctx, 1, 0);
    duk_push_this(ctx);
    duk_unity_put2f(ctx, -1, x, y);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_Scale(duk_context *ctx) {
    float lhsx, lhsy;
    float rhsx, rhsy;
    duk_unity_get2f(ctx, 0, &lhsx, &lhsy);
    duk_push_this(ctx);
    duk_unity_get2f(ctx, -1, &rhsx, &rhsy);
    duk_unity_put2f(ctx, -1, rhsx * lhsx, rhsy * lhsy);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_Normalize(duk_context *ctx) {
    float rhsx, rhsy;
    duk_push_this(ctx);
    duk_unity_get2f(ctx, -1, &rhsx, &rhsy);
    float mag = 1.0f / sqrtf(rhsx * rhsx + rhsy * rhsy);
    duk_unity_put2f(ctx, -1, rhsx / mag, rhsy / mag);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Normalize(duk_context *ctx) {
    float rhsx, rhsy;
    duk_unity_get2f(ctx, 0, &rhsx, &rhsy);
    float mag = 1.0f / sqrtf(rhsx * rhsx + rhsy * rhsy);
    vec2_push_new(ctx, rhsx / mag, rhsy / mag);
    return 1;
}

// static Lerp(a: UnityEngine.Vector2, b: UnityEngine.Vector2, t: number): UnityEngine.Vector2
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Lerp(duk_context *ctx) {
    float a[2];
    float b[2];
    float t;
    duk_unity_get2f(ctx, 0, &a[0], &a[1]);
    duk_unity_get2f(ctx, 1, &b[0], &b[1]);
    t = (float)duk_get_number_default(ctx, 2, 0);
    t = t > 1.0f ? 1.0f : (t < 0.0f ? 0.0f : t);
    vec2_push_new(ctx, a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t);
    return 1;
}

// static LerpUnclamped(a: UnityEngine.Vector2, b: UnityEngine.Vector2, t: number): UnityEngine.Vector2
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_LerpUnclamped(duk_context *ctx) {
    float a[2];
    float b[2];
    float t;
    duk_unity_get2f(ctx, 0, &a[0], &a[1]);
    duk_unity_get2f(ctx, 1, &b[0], &b[1]);
    t = (float)duk_get_number_default(ctx, 2, 0);
    vec2_push_new(ctx, a[0] + (b[0] - a[0]) * t, a[1] + (b[1] - a[1]) * t);
    return 1;
}

// static MoveTowards(current: UnityEngine.Vector2, target: UnityEngine.Vector2, maxDistanceDelta: number): UnityEngine.Vector2
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_MoveTowards(duk_context *ctx) {
    float current[2];
    float target[2];
    float maxDistanceDelta;
    duk_unity_get2f(ctx, 0, &current[0], &current[1]);
    duk_unity_get2f(ctx, 1, &target[0], &target[1]);
    maxDistanceDelta = (float)duk_get_number_default(ctx, 2, 0);

    float res[2];
    vec2_move_towards(current, target, maxDistanceDelta, res);
    vec2_push_new(ctx, res[0], res[1]);
    return 1;
}

// static Scale(a: UnityEngine.Vector2, b: UnityEngine.Vector2): UnityEngine.Vector2
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Scale(duk_context *ctx) {
    float a[2];
    float b[2];
    duk_unity_get2f(ctx, 0, &a[0], &a[1]);
    duk_unity_get2f(ctx, 1, &b[0], &b[1]);
    vec2_push_new(ctx, a[0] * b[0], a[1] * b[1]);
    return 1;
}

// static Reflect(inDirection: UnityEngine.Vector2, inNormal: UnityEngine.Vector2): UnityEngine.Vector2
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Reflect(duk_context *ctx) {
    float inDirection[2];
    float inNormal[2];
    duk_unity_get2f(ctx, 0, &inDirection[0], &inDirection[1]);
    duk_unity_get2f(ctx, 1, &inNormal[0], &inNormal[1]);
    float dot2 = vec2_dot(inNormal, inDirection) * -2.0F;
    vec2_push_new(ctx, 
        dot2 * inNormal[0] + inDirection[0], 
        dot2 * inNormal[1] + inDirection[1]
    );
    return 1;
}

// static Perpendicular(inDirection: UnityEngine.Vector2): UnityEngine.Vector2
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Perpendicular(duk_context *ctx) {
    float inDirection[2];
    duk_unity_get2f(ctx, 0, &inDirection[0], &inDirection[1]);
    vec2_push_new(ctx, 
        -inDirection[1], 
        inDirection[0]
    );
    return 1;
}

// static Dot(lhs: UnityEngine.Vector2, rhs: UnityEngine.Vector2): number
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Dot(duk_context *ctx) {
    float lhs[2];
    float rhs[2];
    duk_unity_get2f(ctx, 0, &lhs[0], &lhs[1]);
    duk_unity_get2f(ctx, 1, &rhs[0], &rhs[1]);
    vec2_push_new(ctx, 
        lhs[0] * rhs[0], 
        lhs[1] * rhs[1]
    );
    return 1;
}

// static Angle(from: UnityEngine.Vector2, to: UnityEngine.Vector2): number
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Angle(duk_context *ctx) {
    float from[2];
    float to[2];
    duk_unity_get2f(ctx, 0, &from[0], &from[1]);
    duk_unity_get2f(ctx, 1, &to[0], &to[1]);
    duk_push_number(ctx, vec2_angle(from, to));
    return 1;
}

// static SignedAngle(from: UnityEngine.Vector2, to: UnityEngine.Vector2): number
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_SignedAngle(duk_context *ctx) {
    float from[2];
    float to[2];
    duk_unity_get2f(ctx, 0, &from[0], &from[1]);
    duk_unity_get2f(ctx, 1, &to[0], &to[1]);
    float angle = vec2_angle(from, to);
    if (from[0] * to[1] - from[1] * to[0] >= 0.0F) {
        duk_push_number(ctx, angle);
    } else {
        duk_push_number(ctx, -angle);
    }
    return 1;
}

// static Distance(a: UnityEngine.Vector2, b: UnityEngine.Vector2): number
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Distance(duk_context *ctx) {
    float a[2];
    float b[2];
    duk_unity_get2f(ctx, 0, &a[0], &a[1]);
    duk_unity_get2f(ctx, 1, &b[0], &b[1]);
    a[0] -= b[0];
    a[1] -= b[1];
    duk_push_number(ctx, vec2_magnitude(a));
    return 1;
}

// static ClampMagnitude(vector: UnityEngine.Vector2, maxLength: number): UnityEngine.Vector2
DUK_LOCAL duk_ret_t duk_unity_Vector2_static_ClampMagnitude(duk_context *ctx) {
    float vector[2];
    float maxLength;
    duk_unity_get2f(ctx, 0, &vector[0], &vector[1]);
    maxLength = (float)duk_get_number_default(ctx, 1, 0);
    float sqrMag = vector[0] * vector[0] + vector[1] * vector[1];
    if (sqrMag > maxLength * maxLength) {
        float mag = sqrtf(sqrMag);
        if (mag > UNITY_VECTOR3_kEpsilon) {
            vec2_push_new(ctx, 
                vector[0] * maxLength / mag, 
                vector[1] * maxLength / mag
            );
        } else {
            vec2_push_new(ctx, 
                0, 
                0
            );
        }
    } else {
        vec2_push_new(ctx, 
            vector[0], 
            vector[1]
        );
    }
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Min(duk_context *ctx) {
    float lhsx, lhsy;
    float rhsx, rhsy;
    duk_unity_get2f(ctx, 0, &lhsx, &lhsy);
    duk_unity_get2f(ctx, 1, &rhsx, &rhsy);
    vec2_push_new(ctx, lhsx > rhsx ? rhsx : lhsx, lhsy > rhsy ? rhsy : lhsy);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Max(duk_context *ctx) {
    float lhsx, lhsy;
    float rhsx, rhsy;
    duk_unity_get2f(ctx, 0, &lhsx, &lhsy);
    duk_unity_get2f(ctx, 1, &rhsx, &rhsy);
    vec2_push_new(ctx, lhsx < rhsx ? rhsx : lhsx, lhsy < rhsy ? rhsy : lhsy);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_normalized(duk_context *ctx) {
    float x, y;
    float mag;
    duk_push_this(ctx);
    duk_unity_get2f(ctx, -1, &x, &y);
    duk_pop(ctx);
    mag = sqrtf(x * x + y * y);
    if (mag > UNITY_VECTOR3_kEpsilon) {
        float rmag = 1.0f / mag;
        vec2_push_new(ctx, x * rmag, y * rmag);
        return 1;
    }
    vec2_push_new(ctx, 0, 0);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_ToString(duk_context *ctx) {
    float rhsx, rhsy;
    duk_push_this(ctx);
    duk_unity_get2f(ctx, -1, &rhsx, &rhsy);
    duk_pop(ctx);
    duk_push_sprintf(ctx, "%f, %f", rhsx, rhsy);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Add(duk_context *ctx) {
    float lhsx, lhsy;
    float rhsx, rhsy;
    duk_unity_get2f(ctx, 0, &lhsx, &lhsy);
    duk_unity_get2f(ctx, 1, &rhsx, &rhsy);
    vec2_push_new(ctx, lhsx + rhsx, lhsy + rhsy);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Sub(duk_context *ctx) {
    float lhsx, lhsy;
    float rhsx, rhsy;
    duk_unity_get2f(ctx, 0, &lhsx, &lhsy);
    duk_unity_get2f(ctx, 1, &rhsx, &rhsy);
    vec2_push_new(ctx, lhsx - rhsx, lhsy - rhsy);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Neg(duk_context *ctx) {
    float lhsx, lhsy;
    duk_unity_get2f(ctx, 0, &lhsx, &lhsy);
    vec2_push_new(ctx, -lhsx, -lhsy);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Mul(duk_context *ctx) {
    float lhsx, lhsy;
    float f;
    if (duk_is_number(ctx, 0)) {
        f = (float)duk_get_number_default(ctx, 0, 0.0);
        duk_unity_get2f(ctx, 1, &lhsx, &lhsy);
    } else {
        duk_unity_get2f(ctx, 0, &lhsx, &lhsy);
        f = (float)duk_get_number_default(ctx, 1, 0.0);
    }
    vec2_push_new(ctx, lhsx * f, lhsy * f);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_Inverse(duk_context *ctx) {
    float rhsx, rhsy;
    duk_push_this(ctx);
    duk_unity_get2f(ctx, -1, &rhsx, &rhsy);
    duk_pop(ctx);
    float x = 1.0f / rhsx;
    float y = 1.0f / rhsy;
    vec2_push_new(ctx, x, y);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Equals(duk_context *ctx) {
    float lhsx, lhsy;
    float rhsx, rhsy;
    duk_unity_get2f(ctx, 0, &lhsx, &lhsy);
    duk_unity_get2f(ctx, 1, &rhsx, &rhsy);
    float x = lhsx - rhsx;
    float y = lhsy - rhsy;
    duk_push_boolean(ctx, (x * x + y * y) < UNITY_VECTOR3_kEpsilon * UNITY_VECTOR3_kEpsilon);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_Equals(duk_context *ctx) {
    float lhsx, lhsy;
    duk_unity_get2f(ctx, 0, &lhsx, &lhsy);
    float rhsx, rhsy;
    duk_push_this(ctx);
    duk_unity_get2f(ctx, -1, &rhsx, &rhsy);
    duk_pop(ctx);
    float x = lhsx - rhsx;
    float y = lhsy - rhsy;
    duk_push_boolean(ctx, (x * x + y * y) < UNITY_VECTOR3_kEpsilon * UNITY_VECTOR3_kEpsilon);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_static_Div(duk_context *ctx) {
    float lhsx, lhsy;
    float f;
    if (duk_is_number(ctx, 0)) {
        f = 1.0f / (float)duk_get_number_default(ctx, 0, 0.0);
        duk_unity_get2f(ctx, 1, &lhsx, &lhsy);
    } else {
        duk_unity_get2f(ctx, 0, &lhsx, &lhsy);
        f = 1.0f / (float)duk_get_number_default(ctx, 1, 0.0);
    }
    vec2_push_new(ctx, lhsx * f, lhsy * f);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_magnitude(duk_context *ctx) {
    float x, y;
    duk_push_this(ctx);
    duk_unity_get2f(ctx, -1, &x, &y);
    duk_pop(ctx);
    duk_push_number(ctx, sqrtf(x * x + y * y));
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_sqrMagnitude(duk_context *ctx) {
    float x, y;
    duk_push_this(ctx);
    duk_unity_get2f(ctx, -1, &x, &y);
    duk_pop(ctx);
    duk_push_number(ctx, x * x + y * y);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_getx(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 0);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_setx(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 0);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_gety(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 1);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector2_sety(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 1);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_constructor(duk_context *ctx) {
    float x = (float)duk_get_number_default(ctx, 0, 0.0);
    float y = (float)duk_get_number_default(ctx, 1, 0.0);
    float z = (float)duk_get_number_default(ctx, 2, 0.0);
    duk_push_this(ctx);
    duk_unity_put3f(ctx, -1, x, y, z);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_Clone(duk_context *ctx) {
    float x, y, z;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &x, &y, &z);
    duk_pop(ctx);
    vec3_push_new(ctx, x, y, z);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_ToString(duk_context *ctx) {
    float rhsx, rhsy, rhsz;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &rhsx, &rhsy, &rhsz);
    duk_pop(ctx);
    duk_push_sprintf(ctx, "%f, %f, %f", rhsx, rhsy, rhsz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_Lerp(duk_context *ctx) {
    float ax, ay, az;
    float bx, by, bz;
    float cx, cy, cz;
    float t;
    duk_unity_get3f(ctx, 0, &ax, &ay, &az);
    duk_unity_get3f(ctx, 1, &bx, &by, &bz);
    t = (float)duk_get_number_default(ctx, 2, 0.0);
    t = t > 1.0f ? 1.0f : (t < 0.0f ? 0.0f : t);
    cx = ax + (bx - ax) * t;
    cy = ay + (by - ay) * t;
    cz = az + (bz - az) * t;
    vec3_push_new(ctx, cx, cy, cz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_LerpUnclamped(duk_context *ctx) {
    float ax, ay, az;
    float bx, by, bz;
    float cx, cy, cz;
    float t;
    duk_unity_get3f(ctx, 0, &ax, &ay, &az);
    duk_unity_get3f(ctx, 1, &bx, &by, &bz);
    t = (float)duk_get_number_default(ctx, 2, 0.0);
    cx = ax + (bx - ax) * t;
    cy = ay + (by - ay) * t;
    cz = az + (bz - az) * t;
    vec3_push_new(ctx, cx, cy, cz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_MoveTowards(duk_context *ctx) {
    float lhs[3];
    float rhs[3];
    float res[3];
    float maxDistanceDelta;
    duk_unity_get3f(ctx, 0, &lhs[0], &lhs[1], &lhs[2]);
    duk_unity_get3f(ctx, 1, &rhs[0], &rhs[1], &rhs[2]);
    maxDistanceDelta = (float)duk_get_number_default(ctx, 2, 0.0);
    vec3_move_towards(lhs, rhs, maxDistanceDelta, res);
    vec3_push_new(ctx, res[0], res[1], res[2]);
    return 1;
}

// static RotateTowards(current: UnityEngine.Vector3, target: UnityEngine.Vector3, maxRadiansDelta: number, maxMagnitudeDelta: number): UnityEngine.Vector3
DUK_LOCAL duk_ret_t duk_unity_Vector3_static_RotateTowards(duk_context *ctx) {
    float lhs[3];
    float rhs[3];
    float res[3];
    float maxRadiansDelta;
    float maxMagnitudeDelta;

    duk_unity_get3f(ctx, 0, &lhs[0], &lhs[1], &lhs[2]);
    duk_unity_get3f(ctx, 1, &rhs[0], &rhs[1], &rhs[2]);
    maxRadiansDelta = (float)duk_get_number_default(ctx, 2, 0.0);
    maxMagnitudeDelta = (float)duk_get_number_default(ctx, 3, 0.0);

    float lhsMag = vec3_magnitude(lhs);
    float rhsMag = vec3_magnitude(rhs);
    if (lhsMag > UNITY_VECTOR3_kEpsilon && rhsMag > UNITY_VECTOR3_kEpsilon) {
        float lhsN[3] = { lhs[0] / lhsMag, lhs[1] / lhsMag, lhs[2] / lhsMag };
        float rhsN[3] = { rhs[0] / rhsMag, rhs[1] / rhsMag, rhs[2] / rhsMag };
        float dot = vec3_dot(lhsN, rhsN);
        if (dot > 1 - UNITY_VECTOR3_kEpsilon) {
            vec3_move_towards(lhs, rhs, maxMagnitudeDelta, res);
            vec3_push_new(ctx, res[0], res[1], res[2]);
            return 1;
        }
        if (dot < -1 + UNITY_VECTOR3_kEpsilon) {
            float axis[3];
            if (fabs(lhsN[2]) > UNITY_VECTOR3_k1OverSqrt2) {
                float k = 1.0f / sqrtf(lhsN[1] * lhsN[1] + lhsN[2] * lhsN[2]);
                axis[0] = 0;
                axis[1] = -lhsN[2] * k;
                axis[2] = lhsN[1] * k;
            } else {
                float k = 1.0f / sqrtf(lhsN[0] * lhsN[0] + lhsN[1] * lhsN[1]);
                axis[0] = -lhsN[1] * k;
                axis[1] = lhsN[0] * k;
                axis[2] = 0;
            }
            float m[9];
            m3x3_set_axis_angle(m, axis, maxRadiansDelta);
            m3x3_multiply_vec3(m, lhsN, res);
            float c = float_clamped(lhsMag, rhsMag, maxMagnitudeDelta);
            vec3_push_new(ctx, res[0] * c, res[1] * c, res[2] * c);
            return 1;
        }
        float angle = acosf(dot);
        float naxis[3];
        vec3_cross(lhsN, rhsN, naxis);
        vec3_normalize(naxis);
        float nm[9];
        m3x3_set_axis_angle(nm, naxis, fminf(maxRadiansDelta, angle));
        m3x3_multiply_vec3(nm, lhsN, res);
        float c = float_clamped(lhsMag, rhsMag, maxMagnitudeDelta);
        vec3_push_new(ctx, res[0] * c, res[1] * c, res[2] * c);
        return 1;
    } 
    vec3_move_towards(lhs, rhs, maxMagnitudeDelta, res);
    vec3_push_new(ctx, res[0], res[1], res[2]);
    return 1;
}

/*
public static Vector3 SmoothDamp(
    0 Vector3 current, 
    1 Vector3 target,
    2 ref Vector3 currentVelocity,
    3 float smoothTime, 
    4 float maxSpeed,
    5 float deltaTime)
*/
DUK_LOCAL duk_ret_t duk_unity_Vector3_static_smoothDamp(duk_context *ctx) {
    float current_x, current_y, current_z;
    duk_unity_get3f(ctx, 0, &current_x, &current_y, &current_z);
    float target_x, target_y, target_z;
    duk_unity_get3f(ctx, 1, &target_x, &target_y, &target_z);
    float currentVelocity_x, currentVelocity_y, currentVelocity_z;
    duk_unity_get3f(ctx, 2, &currentVelocity_x, &currentVelocity_y, &currentVelocity_z);
    float smoothTime = (float)duk_get_number_default(ctx, 3, 0.0001);
    if (smoothTime > 0.0001f) {
        smoothTime = 0.0001f;
    }
    float maxSpeed = (float)duk_get_number_default(ctx, 4, INFINITY);
    float deltaTime = (float)duk_get_number_default(ctx, 5, 0.01667);

    float omega = 2.0f / smoothTime;
    float x = omega * deltaTime;
    float exp_ = 1.0f / (1.0f + x + 0.48f * x * x + 0.235f * x * x * x);
    float change_x = current_x - target_x;
    float change_y = current_y - target_y;
    float change_z = current_z - target_z;
    float originalTo_x = target_x;
    float originalTo_y = target_y;
    float originalTo_z = target_z;
    float maxChange = maxSpeed * smoothTime;
    float change_sqrmag = change_x * change_x + change_y * change_y + change_z * change_z;
    if (change_sqrmag > maxChange * maxChange) {
        float change_mag = sqrtf(change_sqrmag);
        float change_f = maxChange / change_mag;
        change_x *= change_f;
        change_y *= change_f;
        change_z *= change_f;
    }
    target_x = current_x - change_x;
    target_y = current_y - change_y;
    target_z = current_z - change_z;
    float temp_f = omega * change_x * deltaTime;
    float temp_x = currentVelocity_x * deltaTime + temp_f;
    float temp_y = currentVelocity_y * deltaTime + temp_f;
    float temp_z = currentVelocity_z * deltaTime + temp_f;
    currentVelocity_x = (currentVelocity_x - omega * temp_x) * exp_;
    currentVelocity_y = (currentVelocity_y - omega * temp_y) * exp_;
    currentVelocity_z = (currentVelocity_z - omega * temp_z) * exp_;
    float output_x = target_x + (change_x + temp_x) * exp_;
    float output_y = target_y + (change_y + temp_y) * exp_;
    float output_z = target_z + (change_z + temp_z) * exp_;
    
    float dpa_x = originalTo_x - current_x;
    float dpa_y = originalTo_y - current_y;
    float dpa_z = originalTo_z - current_z;
    float dpb_x = output_x - originalTo_x;
    float dpb_y = output_y - originalTo_y;
    float dpb_z = output_z - originalTo_z;
    if (dpa_x * dpb_x + dpa_y * dpb_y + dpa_z * dpb_z > 0) {
        output_x = originalTo_x;
        output_y = originalTo_y;
        output_z = originalTo_z;
        currentVelocity_x = (output_x - originalTo_x) / deltaTime;
        currentVelocity_y = (output_y - originalTo_y) / deltaTime;
        currentVelocity_z = (output_z - originalTo_z) / deltaTime;
    }
    duk_push_number(ctx, currentVelocity_x);
    duk_put_prop_index(ctx, 2, 0);
    duk_push_number(ctx, currentVelocity_y);
    duk_put_prop_index(ctx, 2, 1);
    duk_push_number(ctx, currentVelocity_z);
    duk_put_prop_index(ctx, 2, 2);
    vec3_push_new(ctx, output_x, output_y, output_z);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_Add(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    vec3_push_new(ctx, lhsx + rhsx, lhsy + rhsy, lhsz + rhsz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_Sub(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    vec3_push_new(ctx, lhsx - rhsx, lhsy - rhsy, lhsz - rhsz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_Neg(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    vec3_push_new(ctx, -lhsx, -lhsy, -lhsz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_Mul(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float f;
    if (duk_is_number(ctx, 0)) {
        f = (float)duk_get_number_default(ctx, 0, 0.0);
        duk_unity_get3f(ctx, 1, &lhsx, &lhsy, &lhsz);
    } else {
        duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
        f = (float)duk_get_number_default(ctx, 1, 0.0);
    }
    vec3_push_new(ctx, lhsx * f, lhsy * f, lhsz * f);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_Inverse(duk_context *ctx) {
    float rhsx, rhsy, rhsz;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &rhsx, &rhsy, &rhsz);
    duk_pop(ctx);
    float x = 1.0f / rhsx;
    float y = 1.0f / rhsy;
    float z = 1.0f / rhsz;
    vec3_push_new(ctx, x, y, z);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_Equals(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    float x = lhsx - rhsx;
    float y = lhsy - rhsy;
    float z = lhsz - rhsz;
    duk_push_boolean(ctx, (x * x + y * y + z * z) < UNITY_VECTOR3_kEpsilon * UNITY_VECTOR3_kEpsilon);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_Equals(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    float rhsx, rhsy, rhsz;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &rhsx, &rhsy, &rhsz);
    duk_pop(ctx);
    float x = lhsx - rhsx;
    float y = lhsy - rhsy;
    float z = lhsz - rhsz;
    duk_push_boolean(ctx, (x * x + y * y + z * z) < UNITY_VECTOR3_kEpsilon * UNITY_VECTOR3_kEpsilon);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_Div(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float f;
    if (duk_is_number(ctx, 0)) {
        f = 1.0f / (float)duk_get_number_default(ctx, 0, 0.0);
        duk_unity_get3f(ctx, 1, &lhsx, &lhsy, &lhsz);
    } else {
        duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
        f = 1.0f / (float)duk_get_number_default(ctx, 1, 0.0);
    }
    vec3_push_new(ctx, lhsx * f, lhsy * f, lhsz * f);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_Set(duk_context *ctx) {
    float x, y, z;
    x = (float)duk_get_number_default(ctx, 0, 0);
    y = (float)duk_get_number_default(ctx, 1, 0);
    z = (float)duk_get_number_default(ctx, 2, 0);
    duk_push_this(ctx);
    duk_unity_put3f(ctx, -1, x, y, z);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_scale(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &rhsx, &rhsy, &rhsz);
    duk_unity_put3f(ctx, -1, lhsx * rhsx, lhsy * rhsy, lhsz * rhsz);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_scale(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    vec3_push_new(ctx, lhsx * rhsx, lhsy * rhsy, lhsz * rhsz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_cross(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    vec3_push_new(ctx, lhsy * rhsz - lhsz * rhsy, lhsz * rhsx - lhsx * rhsz, lhsx * rhsy - lhsy * rhsx);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_OrthoNormalize(duk_context *ctx) {
    float u[3] = {0};
    float v[3] = {0};
    duk_unity_get3f(ctx, 0, &u[0], &u[1], &u[2]);
    duk_unity_get3f(ctx, 1, &v[0], &v[1], &v[2]);
    float mag = vec3_magnitude(u);
    if (mag > UNITY_VECTOR3_kEpsilon) {
        u[0] /= mag;
        u[1] /= mag;
        u[2] /= mag;
    } else {
        u[0] = 1;
        u[1] = u[2] = 0;
    }
    duk_unity_put3f(ctx, 0, u[0], u[1], u[2]);
    float dot0 = vec3_dot(u, v);
    v[0] -= u[0] * dot0;
    v[1] -= u[1] * dot0;
    v[2] -= u[2] * dot0;
    mag = vec3_magnitude(v);
    if (mag > UNITY_VECTOR3_kEpsilon) {
        v[0] /= mag;
        v[1] /= mag;
        v[2] /= mag;
    } else {
        v[0] = v[1] = v[2] = 0;
    }
    duk_unity_put3f(ctx, 1, v[0], v[1], v[2]);

    if (duk_get_top(ctx) > 2) {
        float w[3] = {0};
        duk_unity_get3f(ctx, 2, &w[0], &w[1], &w[2]);
        float dot1 = vec3_dot(v, w);
        dot0 = vec3_dot(u, w);
        w[0] -= u[0] * dot0 + v[0] * dot1;
        w[1] -= u[1] * dot0 + v[1] * dot1;
        w[2] -= u[2] * dot0 + v[2] * dot1;
        mag = vec3_magnitude(w);
        if (mag > UNITY_VECTOR3_kEpsilon) {
            w[0] /= mag;
            w[1] /= mag;
            w[2] /= mag;
        } else {
            vec3_cross(u, v, w);
        }
        duk_unity_put3f(ctx, 2, w[0], w[1], w[2]);
    }
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_Slerp(duk_context *ctx) {
    float lhs[3] = {0};
    float rhs[3] = {0};
    float t;
    duk_unity_get3f(ctx, 0, &lhs[0], &lhs[1], &lhs[2]);
    duk_unity_get3f(ctx, 1, &rhs[0], &rhs[1], &rhs[2]);
    t = (float)duk_get_number_default(ctx, 2, 0);
    // t = t > 1.0f ? 1.0f : (t < 0.0f ? 0.0f : t);
    float lhsMag = sqrtf(lhs[0] * lhs[0] + lhs[1] * lhs[1] + lhs[2] * lhs[2]);
    float rhsMag = sqrtf(rhs[0] * rhs[0] + rhs[1] * rhs[1] + rhs[2] * rhs[2]);
    if (lhsMag < UNITY_VECTOR3_kEpsilon || rhsMag < UNITY_VECTOR3_kEpsilon) {
        vec3_push_new(ctx, lhs[0] + (rhs[0] - lhs[0]) * t, lhs[1] + (rhs[1] - lhs[1]) * t, lhs[2] + (rhs[2] - lhs[2]) * t);
        return 1;
    }
    float lerpedMag = lhsMag + (rhsMag - lhsMag) * t;
    float dot = (lhs[0] * rhs[0] + lhs[1] * rhs[1] + lhs[2] * rhs[2]) / (lhsMag * rhsMag);
    if (dot > 1.0f - UNITY_VECTOR3_kEpsilon) {
        vec3_push_new(ctx, lhs[0] + (rhs[0] - lhs[0]) * t, lhs[1] + (rhs[1] - lhs[1]) * t, lhs[2] + (rhs[2] - lhs[2]) * t);
        return 1;
    } else if (dot < -1.0f + UNITY_VECTOR3_kEpsilon) {
        float lhsNorm[3] = {0};
        lhsNorm[0] = lhs[0] / lhsMag;
        lhsNorm[1] = lhs[1] / lhsMag;
        lhsNorm[2] = lhs[2] / lhsMag;
        float axis[3];
        if (fabsf(lhsNorm[2]) > UNITY_VECTOR3_k1OverSqrt2) {
            float k = 1.0f / sqrtf(lhsNorm[1] * lhsNorm[1] + lhsNorm[2] * lhsNorm[2]);
            axis[0] = 0;
            axis[1] = -lhsNorm[2] * k;
            axis[2] = lhsNorm[1] * k;
        } else {
            float k = 1.0f / sqrtf(lhsNorm[0] * lhsNorm[0] + lhsNorm[1] * lhsNorm[1]);
            axis[0] = -lhsNorm[1] * k;
            axis[1] = lhsNorm[0] * k;
            axis[2] = 0;
        }
        float m[9] = {0};
        m3x3_set_axis_angle(m, axis, UNITY_MATH_PI * t);
        float slerped[3] = {0};
        m3x3_multiply_vec3(m, lhsNorm, slerped);
        vec3_multiply(slerped, lerpedMag);
        vec3_push_new(ctx, slerped[0], slerped[1], slerped[2]);
        return 1;
    } else {
        float axis[3] = {0};
        vec3_cross(lhs, rhs, axis);
        vec3_normalize(axis);
        float lhsNorm[3] = {0};
        lhsNorm[0] = lhs[0] / lhsMag;
        lhsNorm[1] = lhs[1] / lhsMag;
        lhsNorm[2] = lhs[2] / lhsMag;
		float angle = acosf(dot) * t;
		
        float m[9] = {0};
        m3x3_set_axis_angle(m, axis, angle);
        float slerped[3] = {0};
        m3x3_multiply_vec3(m, lhsNorm, slerped);
		vec3_multiply(slerped, lerpedMag);
        vec3_push_new(ctx, slerped[0], slerped[1], slerped[2]);
        return 1;
    }
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_reflect(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    float dot2 = 2.0f * (lhsx * rhsx + lhsy * rhsy + lhsz * rhsz);
    vec3_push_new(ctx, dot2 * rhsx + lhsx, dot2 * rhsy + lhsy, dot2 * rhsz + lhsz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_normalize(duk_context *ctx) {
    float x, y, z;
    float mag;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &x, &y, &z);
    mag = sqrtf(x * x + y * y + z * z);
    if (mag > UNITY_VECTOR3_kEpsilon) {
        float rmag = 1.0f / mag;
        duk_unity_put3f(ctx, -1, x * rmag, y * rmag, z * rmag);
    } else {
        duk_unity_put3f(ctx, -1, 0, 0, 0);
    }
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_Normalize(duk_context *ctx) {
    float x, y, z;
    float mag;
    duk_unity_get3f(ctx, 0, &x, &y, &z);
    mag = sqrtf(x * x + y * y + z * z);
    if (mag > UNITY_VECTOR3_kEpsilon) {
        float rmag = 1.0f / mag;
        vec3_push_new(ctx, x * rmag, y * rmag, z * rmag);
    } else {
        vec3_push_new(ctx, 0, 0, 0);
    }
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_normalized(duk_context *ctx) {
    float x, y, z;
    float mag;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &x, &y, &z);
    duk_pop(ctx);
    mag = sqrtf(x * x + y * y + z * z);
    if (mag > UNITY_VECTOR3_kEpsilon) {
        float rmag = 1.0f / mag;
        vec3_push_new(ctx, x * rmag, y * rmag, z * rmag);
        return 1;
    }
    vec3_push_new(ctx, 0, 0, 0);
    return 1;
}

/*
public static float Dot(Vector3 lhs, Vector3 rhs)
*/
DUK_LOCAL duk_ret_t duk_unity_Vector3_static_dot(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    duk_push_number(ctx, lhsx * rhsx + lhsy * rhsy + lhsz * rhsz);
    return 1;
}

/*
public static Vector3 Project(Vector3 vector, Vector3 onNormal)
*/
DUK_LOCAL duk_ret_t duk_unity_Vector3_static_project(duk_context *ctx) {
    float nx, ny, nz;
    duk_unity_get3f(ctx, 1, &nx, &ny, &nz);
    float sqrMag = nx * nx + ny * ny + nz * nz;
    if (sqrMag < UNITY_SINGLE_Epsilon) {
        vec3_push_new(ctx, 0, 0, 0);
        return 1;
    }
    float vx, vy, vz;
    duk_unity_get3f(ctx, 0, &vx, &vy, &vz);
    float dotsq = (vx * nx + vy * ny + vz * nz) / sqrMag;
    vec3_push_new(ctx, nx * dotsq, ny * dotsq, nz * dotsq);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_projectOnPlane(duk_context *ctx) {
    float nx, ny, nz;
    float vx, vy, vz;
    duk_unity_get3f(ctx, 0, &vx, &vy, &vz);
    duk_unity_get3f(ctx, 1, &nx, &ny, &nz);
    float sqrMag = nx * nx + ny * ny + nz * nz;
    if (sqrMag < UNITY_SINGLE_Epsilon) {
        vec3_push_new(ctx, 
            nx, 
            ny, 
            nz);
        return 1;
    }
    float dotsq = (vx * nx + vy * ny + vz * nz) / sqrMag;
    vec3_push_new(ctx, 
        vx - nx * dotsq, 
        vy - ny * dotsq, 
        vz - nz * dotsq);
    return 1;
}

/*
public static float Angle(Vector3 from, Vector3 to)
*/
DUK_LOCAL duk_ret_t duk_unity_Vector3_static_angle(duk_context *ctx) {
    float from_x, from_y, from_z;
    float to_x, to_y, to_z;
    duk_unity_get3f(ctx, 0, &from_x, &from_y, &from_z);
    duk_unity_get3f(ctx, 1, &to_x, &to_y, &to_z);
    float denominator = sqrtf((from_x * from_x + from_y * from_y + from_z * from_z) * (to_x * to_x + to_y * to_y + to_z * to_z));
    if (denominator < UNITY_VECTOR3_kEpsilonNormalSqrt) {
        duk_push_number(ctx, 0);
        return 1;
    }
    float dot = (from_x * to_x + from_y * to_y + from_z * to_z) / denominator;
    if (dot > 1) {
        dot = 1;
    } else if (dot < -1) {
        dot = -1;
    }
    float ret = acosf(dot) * UNITY_RAD2DEG;
    duk_push_number(ctx, ret);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_signedAngle(duk_context *ctx) {
    float from_x, from_y, from_z;
    float to_x, to_y, to_z;
    duk_unity_get3f(ctx, 0, &from_x, &from_y, &from_z);
    duk_unity_get3f(ctx, 1, &to_x, &to_y, &to_z);
    float denominator = sqrtf((from_x * from_x + from_y * from_y + from_z * from_z) * (to_x * to_x + to_y * to_y + to_z * to_z));
    if (denominator < UNITY_VECTOR3_kEpsilonNormalSqrt) {
        duk_push_number(ctx, 0);
        return 1;
    }
    float axis_x, axis_y, axis_z;
    duk_unity_get3f(ctx, 2, &axis_x, &axis_y, &axis_z);
    float dot = (from_x * to_x + from_y * to_y + from_z * to_z) / denominator;
    if (dot > 1) {
        dot = 1;
    } else if (dot < -1) {
        dot = -1;
    }
    float ret = acosf(dot) * UNITY_RAD2DEG;
    float cross_x = from_y * to_z - from_z * to_y;
    float cross_y = from_z * to_x - from_x * to_z;
    float cross_z = from_x * to_y - from_y * to_x;
    float dot_axis = axis_x * cross_x + axis_y * cross_y + axis_z * cross_z;
    duk_push_number(ctx, dot_axis > 0.0f ? ret : -ret);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_distance(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    float x = lhsx - rhsx;
    float y = lhsy - rhsy;
    float z = lhsz - rhsz;
    duk_push_number(ctx, sqrtf(x * x + y * y + z * z));
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_clampMagnitude(duk_context *ctx) {
    float x, y, z;
    float maxLength;
    duk_unity_get3f(ctx, 0, &x, &y, &z);
    maxLength = (float)duk_get_number_default(ctx, 1, 0);
    float sqrMag = x * x + y * y + z * z;
    if (sqrMag > maxLength * maxLength) {
        float rx = maxLength / sqrMag;
        vec3_push_new(ctx, x * rx, y * rx, z * rx);
        return 1;
    }
    vec3_push_new(ctx, x, y, z);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_magnitude(duk_context *ctx) {
    float x, y, z;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &x, &y, &z);
    duk_pop(ctx);
    duk_push_number(ctx, sqrtf(x * x + y * y + z * z));
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_sqrMagnitude(duk_context *ctx) {
    float x, y, z;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &x, &y, &z);
    duk_pop(ctx);
    duk_push_number(ctx, x * x + y * y + z * z);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_min(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    vec3_push_new(ctx, lhsx > rhsx ? rhsx : lhsx, lhsy > rhsy ? rhsy : lhsy, lhsz > rhsz ? rhsz : lhsz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_static_max(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    vec3_push_new(ctx, lhsx < rhsx ? rhsx : lhsx, lhsy < rhsy ? rhsy : lhsy, lhsz < rhsz ? rhsz : lhsz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_getx(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 0);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_setx(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 0);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_gety(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 1);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_sety(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 1);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_getz(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_Vector3_setz(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 2);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL void duk_unity_Vector3_add_const(duk_context *ctx, duk_idx_t idx, const char *key, float x, float y, float z) {
    idx = duk_normalize_index(ctx, idx);
    vec3_push_new(ctx, x, y, z);
    duk_put_prop_string(ctx, idx, key);
}

DUK_EXTERNAL void duk_unity_push_vector2(duk_context *ctx, float x, float y) {
    vec2_push_new(ctx, x, y);
}

DUK_EXTERNAL void duk_unity_push_vector2i(duk_context *ctx, duk_int_t x, duk_int_t y) {
    vec2i_push_new(ctx, x, y);
}

DUK_EXTERNAL void duk_unity_push_vector3(duk_context *ctx, float x, float y, float z) {
    vec3_push_new(ctx, x, y, z);
}

DUK_EXTERNAL void duk_unity_push_vector3i(duk_context *ctx, duk_int_t x, duk_int_t y, duk_int_t z) {
    vec3i_push_new(ctx, x, y, z);
}

DUK_EXTERNAL void duk_unity_push_vector4(duk_context *ctx, float x, float y, float z, float w) {
    vec4_push_new(ctx, x, y, z, w);
}

DUK_EXTERNAL void duk_unity_push_quaternion(duk_context *ctx, float x, float y, float z, float w) {
    quaternion_push_new(ctx, x, y, z, w);
}

DUK_EXTERNAL void duk_unity_push_color(duk_context *ctx, float r, float g, float b, float a) {
    color_push_new(ctx, r, g, b, a);
}

DUK_EXTERNAL void duk_unity_push_color32(duk_context *ctx, unsigned char r, unsigned char g, unsigned char b, unsigned char a) {
    color32_push_new(ctx, r, g, b, a);
}

DUK_LOCAL void duk_unity_Color_add_const(duk_context *ctx, duk_idx_t idx, const char *key, float r, float g, float b, float a) {
    idx = duk_normalize_index(ctx, idx);
    color_push_new(ctx, r, g, b, a);
    duk_put_prop_string(ctx, idx, key);
}

DUK_INTERNAL void duk_unity_valuetypes_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");
    
    {
        duk_unity_begin_class(ctx, "Vector2", DUK_UNITY_BUILTINS_VECTOR2, duk_unity_Vector2_constructor, NULL);

        duk_unity_add_member(ctx, "Add", duk_unity_Vector2_static_Add, -2);
        duk_unity_add_member(ctx, "Sub", duk_unity_Vector2_static_Sub, -2);
        duk_unity_add_member(ctx, "Neg", duk_unity_Vector2_static_Neg, -2);
        duk_unity_add_member(ctx, "Mul", duk_unity_Vector2_static_Mul, -2);
        duk_unity_add_member(ctx, "Div", duk_unity_Vector2_static_Div, -2);
        duk_unity_add_member(ctx, "Inverse", duk_unity_Vector2_Inverse, -1);
        duk_unity_add_member(ctx, "Equals", duk_unity_Vector2_static_Equals, -2);
        duk_unity_add_member(ctx, "Equals", duk_unity_Vector2_Equals, -1);
        duk_unity_add_member(ctx, "ToString", duk_unity_Vector2_ToString, -1);

        duk_unity_add_member(ctx, "Clone", duk_unity_Vector2_Clone, -1);
        duk_unity_add_member(ctx, "Set", duk_unity_Vector2_Set, -1);
        duk_unity_add_member(ctx, "Scale", duk_unity_Vector2_Scale, -1);
        duk_unity_add_member(ctx, "Normalize", duk_unity_Vector2_Normalize, -1);
        duk_unity_add_member(ctx, "Normalize", duk_unity_Vector2_static_Normalize, -2);
        duk_unity_add_member(ctx, "Lerp", duk_unity_Vector2_static_Lerp, -2);
        duk_unity_add_member(ctx, "LerpUnclamped", duk_unity_Vector2_static_LerpUnclamped, -2);
        duk_unity_add_member(ctx, "MoveTowards", duk_unity_Vector2_static_MoveTowards, -2);
        duk_unity_add_member(ctx, "Scale", duk_unity_Vector2_static_Scale, -2);
        duk_unity_add_member(ctx, "Reflect", duk_unity_Vector2_static_Reflect, -2);
        duk_unity_add_member(ctx, "Perpendicular", duk_unity_Vector2_static_Perpendicular, -2);
        duk_unity_add_member(ctx, "Dot", duk_unity_Vector2_static_Dot, -2);
        duk_unity_add_member(ctx, "Angle", duk_unity_Vector2_static_Angle, -2);
        duk_unity_add_member(ctx, "SignedAngle", duk_unity_Vector2_static_SignedAngle, -2);
        duk_unity_add_member(ctx, "Distance", duk_unity_Vector2_static_Distance, -2);
        duk_unity_add_member(ctx, "ClampMagnitude", duk_unity_Vector2_static_ClampMagnitude, -2);
        duk_unity_add_member(ctx, "Min", duk_unity_Vector2_static_Min, -2);
        duk_unity_add_member(ctx, "Max", duk_unity_Vector2_static_Max, -2);
        // static SmoothDamp(current: UnityEngine.Vector2, target: UnityEngine.Vector2, currentVelocity: DuktapeJS.Ref<UnityEngine.Vector2>, smoothTime: number, maxSpeed: number, deltaTime: number): UnityEngine.Vector2
        // static SmoothDamp(current: UnityEngine.Vector2, target: UnityEngine.Vector2, currentVelocity: DuktapeJS.Ref<UnityEngine.Vector2>, smoothTime: number, maxSpeed: number): UnityEngine.Vector2
        // static SmoothDamp(current: UnityEngine.Vector2, target: UnityEngine.Vector2, currentVelocity: DuktapeJS.Ref<UnityEngine.Vector2>, smoothTime: number): UnityEngine.Vector2

        duk_unity_add_property(ctx, "normalized", duk_unity_Vector2_normalized, NULL, -1);
        duk_unity_add_property(ctx, "magnitude", duk_unity_Vector2_magnitude, NULL, -1);
        duk_unity_add_property(ctx, "sqrMagnitude", duk_unity_Vector2_sqrMagnitude, NULL, -1);

        duk_unity_Vector2_add_const(ctx, -2, "zero", 0.0F, 0.0F);
        duk_unity_Vector2_add_const(ctx, -2, "one", 1.0F, 1.0F);
        duk_unity_Vector2_add_const(ctx, -2, "up", 0.0F, 1.0F);
        duk_unity_Vector2_add_const(ctx, -2, "down", 0.0F, -1.0F);
        duk_unity_Vector2_add_const(ctx, -2, "left", -1.0F, 0.0F);
        duk_unity_Vector2_add_const(ctx, -2, "right", 1.0F, 0.0F);
        // duk_unity_Vector2_add_const(ctx, -2, "positiveInfinity", 1.0F / 0.0F, 1.0F / 0.0F);
        // duk_unity_Vector2_add_const(ctx, -2, "negativeInfinity", -1.0F / 0.0F, -1.0F / 0.0F);

        duk_unity_add_property(ctx, "x", duk_unity_Vector2_getx, duk_unity_Vector2_setx, -1);
        duk_unity_add_property(ctx, "y", duk_unity_Vector2_gety, duk_unity_Vector2_sety, -1);

        duk_unity_add_const_number(ctx, -2, "kEpsilon", 1e-05);
        duk_unity_add_const_number(ctx, -2, "kEpsilonNormalSqrt", 1e-15);

        duk_unity_end_class(ctx);
    }
    {
        duk_unity_begin_class(ctx, "Vector2Int", DUK_UNITY_BUILTINS_VECTOR2I, duk_unity_Vector2Int_constructor, NULL);
        // $GetValue(index: number): number
        // $SetValue(index: number, value: number): void
        // Set(x: number, y: number): void
        // Scale(scale: UnityEngine.Vector2Int): void
        // Clamp(min: UnityEngine.Vector2Int, max: UnityEngine.Vector2Int): void
        // Equals(other: System.Object): boolean
        // Equals(other: UnityEngine.Vector2Int): boolean
        // GetHashCode(): number
        // ToString(): string
        // static Distance(a: UnityEngine.Vector2Int, b: UnityEngine.Vector2Int): number
        // static Min(lhs: UnityEngine.Vector2Int, rhs: UnityEngine.Vector2Int): UnityEngine.Vector2Int
        // static Max(lhs: UnityEngine.Vector2Int, rhs: UnityEngine.Vector2Int): UnityEngine.Vector2Int
        // static Scale(a: UnityEngine.Vector2Int, b: UnityEngine.Vector2Int): UnityEngine.Vector2Int
        // static FloorToInt(v: UnityEngine.Vector2): UnityEngine.Vector2Int
        // static CeilToInt(v: UnityEngine.Vector2): UnityEngine.Vector2Int
        // static RoundToInt(v: UnityEngine.Vector2): UnityEngine.Vector2Int
        // x: number
        // y: number
        // readonly magnitude: number
        // readonly sqrMagnitude: number
        // readonly zero: UnityEngine.Vector2Int
        // readonly one: UnityEngine.Vector2Int
        // readonly up: UnityEngine.Vector2Int
        // readonly down: UnityEngine.Vector2Int
        // readonly left: UnityEngine.Vector2Int
        // readonly right: UnityEngine.Vector2Int
        duk_unity_end_class(ctx);
    }
    {
        duk_unity_begin_class(ctx, "Vector3", DUK_UNITY_BUILTINS_VECTOR3, duk_unity_Vector3_constructor, NULL);

        duk_unity_add_member(ctx, "Add", duk_unity_Vector3_static_Add, -2);
        duk_unity_add_member(ctx, "Sub", duk_unity_Vector3_static_Sub, -2);
        duk_unity_add_member(ctx, "Neg", duk_unity_Vector3_static_Neg, -2);
        duk_unity_add_member(ctx, "Mul", duk_unity_Vector3_static_Mul, -2);
        duk_unity_add_member(ctx, "Div", duk_unity_Vector3_static_Div, -2);
        duk_unity_add_member(ctx, "Inverse", duk_unity_Vector3_Inverse, -1);
        duk_unity_add_member(ctx, "Equals", duk_unity_Vector3_static_Equals, -2);
        duk_unity_add_member(ctx, "Equals", duk_unity_Vector3_Equals, -1);
        duk_unity_add_member(ctx, "ToString", duk_unity_Vector3_ToString, -1);

        duk_unity_add_member(ctx, "Clone", duk_unity_Vector3_Clone, -1);
        duk_unity_add_member(ctx, "Set", duk_unity_Vector3_Set, -1);
        duk_unity_add_member(ctx, "Lerp", duk_unity_Vector3_static_Lerp, -1);
        duk_unity_add_member(ctx, "LerpUnclamped", duk_unity_Vector3_static_LerpUnclamped, -2);
        duk_unity_add_member(ctx, "MoveTowards", duk_unity_Vector3_static_MoveTowards, -2);
        duk_unity_add_member(ctx, "RotateTowards", duk_unity_Vector3_static_RotateTowards, -2);
        duk_unity_add_member(ctx, "SmoothDamp", duk_unity_Vector3_static_smoothDamp, -2);
        duk_unity_add_member(ctx, "Scale", duk_unity_Vector3_scale, -1);
        duk_unity_add_member(ctx, "Scale", duk_unity_Vector3_static_scale, -2);
        duk_unity_add_member(ctx, "Cross", duk_unity_Vector3_static_cross, -2);
        duk_unity_add_member(ctx, "Slerp", duk_unity_Vector3_static_Slerp, -2);
        duk_unity_add_member(ctx, "OrthoNormalize", duk_unity_Vector3_static_OrthoNormalize, -2);
        duk_unity_add_member(ctx, "Reflect", duk_unity_Vector3_static_reflect, -2);
        duk_unity_add_member(ctx, "Normalize", duk_unity_Vector3_normalize, -1);
        duk_unity_add_member(ctx, "Normalize", duk_unity_Vector3_static_Normalize, -2);
        duk_unity_add_member(ctx, "Dot", duk_unity_Vector3_static_dot, -2);
        duk_unity_add_member(ctx, "Project", duk_unity_Vector3_static_project, -2);
        duk_unity_add_member(ctx, "ProjectOnPlane", duk_unity_Vector3_static_projectOnPlane, -2);
        duk_unity_add_member(ctx, "Angle", duk_unity_Vector3_static_angle, -2);
        duk_unity_add_member(ctx, "SignedAngle", duk_unity_Vector3_static_signedAngle, -2);
        duk_unity_add_member(ctx, "Distance", duk_unity_Vector3_static_distance, -2);
        duk_unity_add_member(ctx, "ClampMagnitude", duk_unity_Vector3_static_clampMagnitude, -2);
        duk_unity_add_member(ctx, "Min", duk_unity_Vector3_static_min, -2);
        duk_unity_add_member(ctx, "Max", duk_unity_Vector3_static_max, -2);

        duk_unity_add_property(ctx, "normalized", duk_unity_Vector3_normalized, NULL, -1);
        duk_unity_add_property(ctx, "magnitude", duk_unity_Vector3_magnitude, NULL, -1);
        duk_unity_add_property(ctx, "sqrMagnitude", duk_unity_Vector3_sqrMagnitude, NULL, -1);
        duk_unity_add_property(ctx, "x", duk_unity_Vector3_getx, duk_unity_Vector3_setx, -1);
        duk_unity_add_property(ctx, "y", duk_unity_Vector3_gety, duk_unity_Vector3_sety, -1);
        duk_unity_add_property(ctx, "z", duk_unity_Vector3_getz, duk_unity_Vector3_setz, -1);

        duk_unity_Vector3_add_const(ctx, -2, "zero", 0.0, 0.0, 0.0);
        duk_unity_Vector3_add_const(ctx, -2, "one", 1.0, 1.0, 1.0);
        duk_unity_Vector3_add_const(ctx, -2, "forward", 0.0, 0.0, 1.0);
        duk_unity_Vector3_add_const(ctx, -2, "back", 0.0, 0.0, -1.0);
        duk_unity_Vector3_add_const(ctx, -2, "up", 0.0, 1.0, 0.0);
        duk_unity_Vector3_add_const(ctx, -2, "down", 0.0, -1.0, 0.0);
        duk_unity_Vector3_add_const(ctx, -2, "left", -1.0, 0.0, 0.0);
        duk_unity_Vector3_add_const(ctx, -2, "right", 1.0, 0.0, 0.0);

        // duk_unity_Vector3_add_const(ctx, -2, "positiveInfinity", 1.0F / 0.0F, 1.0F / 0.0F, 1.0F / 0.0F);
        // duk_unity_Vector3_add_const(ctx, -2, "negativeInfinity", -1.0F / 0.0F, -1.0F / 0.0F, -1.0F / 0.0F);

        duk_unity_add_const_number(ctx, -2, "kEpsilon", 1e-05);
        duk_unity_add_const_number(ctx, -2, "kEpsilonNormalSqrt", 1e-15);
        duk_unity_end_class(ctx);
    }
    {
        duk_unity_begin_class(ctx, "Vector3Int", DUK_UNITY_BUILTINS_VECTOR3I, duk_unity_Vector3Int_constructor, NULL);
        // $GetValue(index: number): number
        // $SetValue(index: number, value: number): void
        // Set(x: number, y: number, z: number): void
        // Scale(scale: UnityEngine.Vector3Int): void
        // Clamp(min: UnityEngine.Vector3Int, max: UnityEngine.Vector3Int): void
        // Equals(other: System.Object): boolean
        // Equals(other: UnityEngine.Vector3Int): boolean
        // GetHashCode(): number
        // ToString(format: string): string
        // ToString(): string
        // static Distance(a: UnityEngine.Vector3Int, b: UnityEngine.Vector3Int): number
        // static Min(lhs: UnityEngine.Vector3Int, rhs: UnityEngine.Vector3Int): UnityEngine.Vector3Int
        // static Max(lhs: UnityEngine.Vector3Int, rhs: UnityEngine.Vector3Int): UnityEngine.Vector3Int
        // static Scale(a: UnityEngine.Vector3Int, b: UnityEngine.Vector3Int): UnityEngine.Vector3Int
        // static FloorToInt(v: UnityEngine.Vector3): UnityEngine.Vector3Int
        // static CeilToInt(v: UnityEngine.Vector3): UnityEngine.Vector3Int
        // static RoundToInt(v: UnityEngine.Vector3): UnityEngine.Vector3Int
        // x: number
        // y: number
        // z: number
        // readonly magnitude: number
        // readonly sqrMagnitude: number
        // readonly zero: UnityEngine.Vector3Int
        // readonly one: UnityEngine.Vector3Int
        // readonly up: UnityEngine.Vector3Int
        // readonly down: UnityEngine.Vector3Int
        // readonly left: UnityEngine.Vector3Int
        // readonly right: UnityEngine.Vector3Int
        duk_unity_end_class(ctx);
    }
    {
        duk_unity_begin_class(ctx, "Vector4", DUK_UNITY_BUILTINS_VECTOR4, duk_unity_Vector4_constructor, NULL);
        // $GetValue(index: number): number
        // $SetValue(index: number, value: number): void
        // Set(newX: number, newY: number, newZ: number, newW: number): void
        // Scale(scale: UnityEngine.Vector4): void
        // GetHashCode(): number
        // Equals(other: System.Object): boolean
        // Equals(other: UnityEngine.Vector4): boolean
        // Normalize(): void
        // ToString(format: string): string
        // ToString(): string
        // SqrMagnitude(): number
        // static Lerp(a: UnityEngine.Vector4, b: UnityEngine.Vector4, t: number): UnityEngine.Vector4
        // static LerpUnclamped(a: UnityEngine.Vector4, b: UnityEngine.Vector4, t: number): UnityEngine.Vector4
        // static MoveTowards(current: UnityEngine.Vector4, target: UnityEngine.Vector4, maxDistanceDelta: number): UnityEngine.Vector4
        // static Scale(a: UnityEngine.Vector4, b: UnityEngine.Vector4): UnityEngine.Vector4
        // static Normalize(a: UnityEngine.Vector4): UnityEngine.Vector4
        // static Dot(a: UnityEngine.Vector4, b: UnityEngine.Vector4): number
        // static Project(a: UnityEngine.Vector4, b: UnityEngine.Vector4): UnityEngine.Vector4
        // static Distance(a: UnityEngine.Vector4, b: UnityEngine.Vector4): number
        // static Magnitude(a: UnityEngine.Vector4): number
        // static Min(lhs: UnityEngine.Vector4, rhs: UnityEngine.Vector4): UnityEngine.Vector4
        // static Max(lhs: UnityEngine.Vector4, rhs: UnityEngine.Vector4): UnityEngine.Vector4
        // static SqrMagnitude(a: UnityEngine.Vector4): number
        // readonly normalized: UnityEngine.Vector4
        // readonly magnitude: number
        // readonly sqrMagnitude: number
        // readonly zero: UnityEngine.Vector4
        // readonly one: UnityEngine.Vector4
        // readonly positiveInfinity: UnityEngine.Vector4
        // readonly negativeInfinity: UnityEngine.Vector4
        duk_unity_add_const_number(ctx, -2, "kEpsilon", 1e-05);
        duk_unity_add_property(ctx, "x", duk_unity_Vector4_getx, duk_unity_Vector4_setx, -1);
        duk_unity_add_property(ctx, "y", duk_unity_Vector4_gety, duk_unity_Vector4_sety, -1);
        duk_unity_add_property(ctx, "z", duk_unity_Vector4_getz, duk_unity_Vector4_setz, -1);
        duk_unity_add_property(ctx, "w", duk_unity_Vector4_getw, duk_unity_Vector4_setw, -1);
        duk_unity_end_class(ctx);
    }
    {
        duk_unity_begin_class(ctx, "Quaternion", DUK_UNITY_BUILTINS_QUATERNION, duk_unity_Quaternion_constructor, NULL);
        duk_unity_add_member(ctx, "Clone", duk_unity_Quaternion_Clone, -1);
        duk_unity_add_member(ctx, "Set", duk_unity_Quaternion_Set, -1);
        // SetLookRotation(view: UnityEngine.Vector3, up: UnityEngine.Vector3): void
        // SetLookRotation(view: UnityEngine.Vector3): void
        // ToAngleAxis(angle: DuktapeJS.Out<number>, axis: DuktapeJS.Out<UnityEngine.Vector3>): void
        // SetFromToRotation(fromDirection: UnityEngine.Vector3, toDirection: UnityEngine.Vector3): void
        // Normalize(): void
        // GetHashCode(): number
        // Equals(other: System.Object): boolean
        // Equals(other: UnityEngine.Quaternion): boolean
        // ToString(format: string): string
        // ToString(): string
        // static FromToRotation(fromDirection: UnityEngine.Vector3, toDirection: UnityEngine.Vector3): UnityEngine.Quaternion
        // static Inverse(rotation: UnityEngine.Quaternion): UnityEngine.Quaternion
        // static Slerp(a: UnityEngine.Quaternion, b: UnityEngine.Quaternion, t: number): UnityEngine.Quaternion
        // static SlerpUnclamped(a: UnityEngine.Quaternion, b: UnityEngine.Quaternion, t: number): UnityEngine.Quaternion
        // static Lerp(a: UnityEngine.Quaternion, b: UnityEngine.Quaternion, t: number): UnityEngine.Quaternion
        // static LerpUnclamped(a: UnityEngine.Quaternion, b: UnityEngine.Quaternion, t: number): UnityEngine.Quaternion
        // static AngleAxis(angle: number, axis: UnityEngine.Vector3): UnityEngine.Quaternion
        // static LookRotation(forward: UnityEngine.Vector3, upwards: UnityEngine.Vector3): UnityEngine.Quaternion
        // static LookRotation(forward: UnityEngine.Vector3): UnityEngine.Quaternion
        // static Dot(a: UnityEngine.Quaternion, b: UnityEngine.Quaternion): number
        // static Angle(a: UnityEngine.Quaternion, b: UnityEngine.Quaternion): number
        // static Euler(x: number, y: number, z: number): UnityEngine.Quaternion
        // static Euler(euler: UnityEngine.Vector3): UnityEngine.Quaternion
        // static RotateTowards(from: UnityEngine.Quaternion, to: UnityEngine.Quaternion, maxDegreesDelta: number): UnityEngine.Quaternion
        // static Normalize(q: UnityEngine.Quaternion): UnityEngine.Quaternion
        // readonly identity: UnityEngine.Quaternion
        // eulerAngles: UnityEngine.Vector3
        // readonly normalized: UnityEngine.Quaternion
        duk_unity_add_property(ctx, "x", duk_unity_Quaternion_getx, duk_unity_Quaternion_setx, -1);
        duk_unity_add_property(ctx, "y", duk_unity_Quaternion_gety, duk_unity_Quaternion_sety, -1);
        duk_unity_add_property(ctx, "z", duk_unity_Quaternion_getz, duk_unity_Quaternion_setz, -1);
        duk_unity_add_property(ctx, "w", duk_unity_Quaternion_getw, duk_unity_Quaternion_setw, -1);
        duk_unity_add_const_number(ctx, -2, "kEpsilon", 1e-05);

        duk_unity_end_class(ctx);
    }
    {
        duk_unity_begin_class(ctx, "Color", DUK_UNITY_BUILTINS_COLOR, duk_unity_Color_constructor, NULL);
        duk_unity_add_member(ctx, "Add", duk_unity_Color_static_Add, -2);
        duk_unity_add_member(ctx, "Sub", duk_unity_Color_static_Sub, -2);
        duk_unity_add_member(ctx, "Neg", duk_unity_Color_static_Neg, -2);
        duk_unity_add_member(ctx, "Mul", duk_unity_Color_static_Mul, -2);
        duk_unity_add_member(ctx, "Div", duk_unity_Color_static_Div, -2);
        duk_unity_add_member(ctx, "Inverse", duk_unity_Color_Inverse, -1);
        duk_unity_add_member(ctx, "Equals", duk_unity_Color_static_Equals, -2);
        duk_unity_add_member(ctx, "Equals", duk_unity_Color_Equals, -1);
        duk_unity_add_member(ctx, "ToString", duk_unity_Color_ToString, -1);

        duk_unity_add_member(ctx, "Lerp", duk_unity_Color_static_Lerp, -1);
        duk_unity_add_member(ctx, "LerpUnclamped", duk_unity_Color_static_LerpUnclamped, -2);
        
        duk_unity_Color_add_const(ctx, -2, "red", 1.0F, 0.0F, 0.0F, 1.0F); 
        duk_unity_Color_add_const(ctx, -2, "green", 0.0F, 1.0F, 0.0F, 1.0F); 
        duk_unity_Color_add_const(ctx, -2, "blue", 0.0F, 0.0F, 1.0F, 1.0F); 
        duk_unity_Color_add_const(ctx, -2, "white", 1.0F, 1.0F, 1.0F, 1.0F); 
        duk_unity_Color_add_const(ctx, -2, "black", 0.0F, 0.0F, 0.0F, 1.0F); 
        duk_unity_Color_add_const(ctx, -2, "yellow", 1.0F, 235.0F / 255.0F, 4.0F / 255.0F, 1.0F); 
        duk_unity_Color_add_const(ctx, -2, "cyan", 0.0F, 1.0F, 1.0F, 1.0F); 
        duk_unity_Color_add_const(ctx, -2, "magenta", 1.0F, 0.0F, 1.0F, 1.0F); 
        duk_unity_Color_add_const(ctx, -2, "gray", 0.5F, 0.5F, 0.5F, 1.0F); 
        duk_unity_Color_add_const(ctx, -2, "grey", 0.5F, 0.5F, 0.5F, 1.0F); 
        duk_unity_Color_add_const(ctx, -2, "clear", 0.0F, 0.0F, 0.0F, 0.0F); 
        duk_unity_add_property(ctx, "grayscale", duk_unity_Color_grayscale, NULL, -1);
        // static RGBToHSV(rgbColor: UnityEngine.Color, H: DuktapeJS.Out<number>, S: DuktapeJS.Out<number>, V: DuktapeJS.Out<number>): void
        // static HSVToRGB(H: number, S: number, V: number, hdr: boolean): UnityEngine.Color
        // static HSVToRGB(H: number, S: number, V: number): UnityEngine.Color
        duk_unity_add_property(ctx, "linear", duk_unity_Color_linear, NULL, -1);
        duk_unity_add_property(ctx, "gamma", duk_unity_Color_gamma, NULL, -1);
        duk_unity_add_property(ctx, "maxColorComponent", duk_unity_Color_maxColorComponent, NULL, -1);
        duk_unity_add_property(ctx, "r", duk_unity_Color_getr, duk_unity_Color_setr, -1);
        duk_unity_add_property(ctx, "g", duk_unity_Color_getg, duk_unity_Color_setg, -1);
        duk_unity_add_property(ctx, "b", duk_unity_Color_getb, duk_unity_Color_setb, -1);
        duk_unity_add_property(ctx, "a", duk_unity_Color_geta, duk_unity_Color_seta, -1);
        
        duk_unity_end_class(ctx);
    }
    {
        duk_unity_begin_class(ctx, "Color32", DUK_UNITY_BUILTINS_COLOR32, duk_unity_Color32_constructor, NULL);
        duk_unity_add_member(ctx, "Lerp", duk_unity_Color32_static_Lerp, -1);
        duk_unity_add_member(ctx, "LerpUnclamped", duk_unity_Color32_static_LerpUnclamped, -2);
        duk_unity_add_property(ctx, "r", duk_unity_Color32_getr, duk_unity_Color32_setr, -1);
        duk_unity_add_property(ctx, "g", duk_unity_Color32_getg, duk_unity_Color32_setg, -1);
        duk_unity_add_property(ctx, "b", duk_unity_Color32_getb, duk_unity_Color32_setb, -1);
        duk_unity_add_property(ctx, "a", duk_unity_Color32_geta, duk_unity_Color32_seta, -1);
        duk_unity_end_class(ctx);
    }
    {
        duk_unity_begin_class(ctx, "Matrix4x4", DUK_UNITY_BUILTINS_MATRIX44, duk_unity_Matrix4x4_constructor, NULL);
        // $GetValue(row: number, column: number): number
        // $GetValue(index: number): number
        // $SetValue(row: number, column: number, value: number): void
        // $SetValue(index: number, value: number): void
        // ValidTRS(): boolean
        // SetTRS(pos: UnityEngine.Vector3, q: UnityEngine.Quaternion, s: UnityEngine.Vector3): void
        // GetHashCode(): number
        // Equals(other: System.Object): boolean
        // Equals(other: UnityEngine.Matrix4x4): boolean
        // GetColumn(index: number): UnityEngine.Vector4
        // GetRow(index: number): UnityEngine.Vector4
        // SetColumn(index: number, column: UnityEngine.Vector4): void
        // SetRow(index: number, row: UnityEngine.Vector4): void
        // MultiplyPoint(point: UnityEngine.Vector3): UnityEngine.Vector3
        // MultiplyPoint3x4(point: UnityEngine.Vector3): UnityEngine.Vector3
        // MultiplyVector(vector: UnityEngine.Vector3): UnityEngine.Vector3
        // TransformPlane(plane: UnityEngine.Plane): UnityEngine.Plane
        // ToString(format: string): string
        // ToString(): string
        // static Determinant(m: UnityEngine.Matrix4x4): number
        // static TRS(pos: UnityEngine.Vector3, q: UnityEngine.Quaternion, s: UnityEngine.Vector3): UnityEngine.Matrix4x4
        // static Inverse(m: UnityEngine.Matrix4x4): UnityEngine.Matrix4x4
        // static Transpose(m: UnityEngine.Matrix4x4): UnityEngine.Matrix4x4
        // static Ortho(left: number, right: number, bottom: number, top: number, zNear: number, zFar: number): UnityEngine.Matrix4x4
        // static Perspective(fov: number, aspect: number, zNear: number, zFar: number): UnityEngine.Matrix4x4
        // static LookAt(from: UnityEngine.Vector3, to: UnityEngine.Vector3, up: UnityEngine.Vector3): UnityEngine.Matrix4x4
        // static Frustum(left: number, right: number, bottom: number, top: number, zNear: number, zFar: number): UnityEngine.Matrix4x4
        // static Frustum(fp: UnityEngine.FrustumPlanes): UnityEngine.Matrix4x4
        // static Scale(vector: UnityEngine.Vector3): UnityEngine.Matrix4x4
        // static Translate(vector: UnityEngine.Vector3): UnityEngine.Matrix4x4
        // static Rotate(q: UnityEngine.Quaternion): UnityEngine.Matrix4x4
        // readonly rotation: UnityEngine.Quaternion
        // readonly lossyScale: UnityEngine.Vector3
        // readonly isIdentity: boolean
        // readonly determinant: number
        // readonly decomposeProjection: UnityEngine.FrustumPlanes
        // readonly inverse: UnityEngine.Matrix4x4
        // readonly transpose: UnityEngine.Matrix4x4
        duk_unity_Matrix4x4_add_const(ctx, -2, "zero", 0, 0, 0, 0);
        float c0[4] = { 1.0F, 0.0F, 0.0F, 0.0F };
        float c1[4] = { 0.0F, 1.0F, 0.0F, 0.0F };
        float c2[4] = { 0.0F, 0.0F, 1.0F, 0.0F };
        float c3[4] = { 0.0F, 0.0F, 0.0F, 1.0F };
        duk_unity_Matrix4x4_add_const(ctx, -2, "identity", c0, c1, c2, c3);
        duk_unity_add_property(ctx, "m00", duk_unity_Matrix4x4_get_m00, duk_unity_Matrix4x4_set_m00, -1); 
        duk_unity_add_property(ctx, "m10", duk_unity_Matrix4x4_get_m10, duk_unity_Matrix4x4_set_m10, -1); 
        duk_unity_add_property(ctx, "m20", duk_unity_Matrix4x4_get_m20, duk_unity_Matrix4x4_set_m20, -1); 
        duk_unity_add_property(ctx, "m30", duk_unity_Matrix4x4_get_m30, duk_unity_Matrix4x4_set_m30, -1); 
        duk_unity_add_property(ctx, "m01", duk_unity_Matrix4x4_get_m01, duk_unity_Matrix4x4_set_m01, -1); 
        duk_unity_add_property(ctx, "m11", duk_unity_Matrix4x4_get_m11, duk_unity_Matrix4x4_set_m11, -1); 
        duk_unity_add_property(ctx, "m21", duk_unity_Matrix4x4_get_m21, duk_unity_Matrix4x4_set_m21, -1); 
        duk_unity_add_property(ctx, "m31", duk_unity_Matrix4x4_get_m31, duk_unity_Matrix4x4_set_m31, -1); 
        duk_unity_add_property(ctx, "m02", duk_unity_Matrix4x4_get_m02, duk_unity_Matrix4x4_set_m02, -1); 
        duk_unity_add_property(ctx, "m12", duk_unity_Matrix4x4_get_m12, duk_unity_Matrix4x4_set_m12, -1); 
        duk_unity_add_property(ctx, "m22", duk_unity_Matrix4x4_get_m22, duk_unity_Matrix4x4_set_m22, -1); 
        duk_unity_add_property(ctx, "m32", duk_unity_Matrix4x4_get_m32, duk_unity_Matrix4x4_set_m32, -1); 
        duk_unity_add_property(ctx, "m03", duk_unity_Matrix4x4_get_m03, duk_unity_Matrix4x4_set_m03, -1); 
        duk_unity_add_property(ctx, "m13", duk_unity_Matrix4x4_get_m13, duk_unity_Matrix4x4_set_m13, -1); 
        duk_unity_add_property(ctx, "m23", duk_unity_Matrix4x4_get_m23, duk_unity_Matrix4x4_set_m23, -1); 
        duk_unity_add_property(ctx, "m33", duk_unity_Matrix4x4_get_m33, duk_unity_Matrix4x4_set_m33, -1); 
        duk_unity_end_class(ctx);
    }
    duk_pop_2(ctx); // pop DuktapeJS and global
}
