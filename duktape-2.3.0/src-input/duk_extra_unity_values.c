#include "duk_internal.h"

/**
 * Vector3
 */
#define UNITY_VECTOR3_kEpsilon 0.00001
#define UNITY_SINGLE_Epsilon 1.401298E-45F
#define UNITY_VECTOR3_kEpsilonNormalSqrt 1e-15F

DUK_LOCAL duk_ret_t duk_unity_vector3_constructor(duk_context *ctx) {
    float x = (float)duk_get_number_default(ctx, 0, 0.0);
    float y = (float)duk_get_number_default(ctx, 1, 0.0);
    float z = (float)duk_get_number_default(ctx, 2, 0.0);
    duk_push_this(ctx);
    duk_unity_put3f(ctx, x, y, z);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_lerp(duk_context *ctx) {
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
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, cx);
    duk_push_number(ctx, cy);
    duk_push_number(ctx, cz);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_lerpUnclamped(duk_context *ctx) {
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
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, cx);
    duk_push_number(ctx, cy);
    duk_push_number(ctx, cz);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_moveTowards(duk_context *ctx) {
/*
public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
    Vector3 toVector = target - current;
    float dist = toVector.magnitude;
    if (dist <= maxDistanceDelta || dist < float.Epsilon)
        return target;
    return current + toVector / dist * maxDistanceDelta;
*/
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    float maxDistanceDelta;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    maxDistanceDelta = (float)duk_get_number_default(ctx, 2, 0.0);

    float tox, toy, toz;
    tox = rhsx - lhsx;
    toy = rhsy - lhsy;
    toz = rhsz - lhsz;
    float dist = (float)sqrt(tox * tox + toy * toy + toz * toz);
    if (dist < maxDistanceDelta || dist < UNITY_SINGLE_Epsilon) {
        duk_dup(ctx, 1);
        return 1;
    }
    float dd = 1 / dist * maxDistanceDelta;
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, lhsx + tox * dd);
    duk_push_number(ctx, lhsy + toy * dd);
    duk_push_number(ctx, lhsz + toz * dd);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_smoothDamp(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_Add(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, lhsx + rhsx);
    duk_push_number(ctx, lhsy + rhsy);
    duk_push_number(ctx, lhsz + rhsz);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_Sub(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, lhsx - rhsx);
    duk_push_number(ctx, lhsy - rhsy);
    duk_push_number(ctx, lhsz - rhsz);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_Neg(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, -lhsx);
    duk_push_number(ctx, -lhsy);
    duk_push_number(ctx, -lhsz);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_Mul(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float f;
    if (duk_is_number(ctx, 0)) {
        f = (float)duk_get_number_default(ctx, 0, 0.0);
        duk_unity_get3f(ctx, 1, &lhsx, &lhsy, &lhsz);
    } else {
        duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
        f = (float)duk_get_number_default(ctx, 1, 0.0);
    }
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, lhsx * f);
    duk_push_number(ctx, lhsy * f);
    duk_push_number(ctx, lhsz * f);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_Div(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float f;
    if (duk_is_number(ctx, 0)) {
        f = 1.0f / (float)duk_get_number_default(ctx, 0, 0.0);
        duk_unity_get3f(ctx, 1, &lhsx, &lhsy, &lhsz);
    } else {
        duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
        f = 1.0f / (float)duk_get_number_default(ctx, 1, 0.0);
    }
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, lhsx * f);
    duk_push_number(ctx, lhsy * f);
    duk_push_number(ctx, lhsz * f);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_set(duk_context *ctx) {
    float ax, ay, az;
    ax = (float)duk_get_number_default(ctx, 0, 0.0);
    ay = (float)duk_get_number_default(ctx, 1, 0.0);
    az = (float)duk_get_number_default(ctx, 2, 0.0);
    duk_push_this(ctx);
    duk_unity_put3f(ctx, ax, ay, az);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_scale(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &rhsx, &rhsy, &rhsz);
    duk_unity_put3f(ctx, lhsx * rhsx, lhsy * rhsy, lhsz * rhsz);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_scale(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, lhsx * rhsx);
    duk_push_number(ctx, lhsy * rhsy);
    duk_push_number(ctx, lhsz * rhsz);
    duk_new(ctx, 3);
    return 1;
}

/*
    lhs.y * rhs.z - lhs.z * rhs.y
    lhs.z * rhs.x - lhs.x * rhs.z
    lhs.x * rhs.y - lhs.y * rhs.x
*/
DUK_LOCAL duk_ret_t duk_unity_vector3_static_cross(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, lhsy * rhsz - lhsz * rhsy);
    duk_push_number(ctx, lhsz * rhsx - lhsx * rhsz);
    duk_push_number(ctx, lhsx * rhsy - lhsy * rhsx);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_reflect(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    float dot2 = 2.0f * (lhsx * rhsx + lhsy * rhsy + lhsz * rhsz);
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, dot2 * rhsx + lhsx);
    duk_push_number(ctx, dot2 * rhsy + lhsy);
    duk_push_number(ctx, dot2 * rhsz + lhsz);
    duk_new(ctx, 3);    
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_normalize(duk_context *ctx) {
    float x, y, z;
    float mag;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &x, &y, &z);
    mag = (float)sqrt(x * x + y * y + z * z);
    if (mag > UNITY_VECTOR3_kEpsilon) {
        float rmag = 1.0f / mag;
        duk_unity_put3f(ctx, x * rmag, y * rmag, z * rmag);
    } else {
        duk_unity_put3f(ctx, 0, 0, 0);
    }
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_normalized(duk_context *ctx) {
    float x, y, z;
    float mag;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &x, &y, &z);
    duk_pop(ctx);
    mag = (float)sqrt(x * x + y * y + z * z);
    if (mag > UNITY_VECTOR3_kEpsilon) {
        float rmag = 1.0f / mag;
        duk_builtins_reg_get(ctx, "Vector3");
        duk_push_number(ctx, x * rmag);
        duk_push_number(ctx, y * rmag);
        duk_push_number(ctx, z * rmag);
        duk_new(ctx, 3);
        return 1;
    }
    duk_builtins_reg_get(ctx, "Vector3");
    duk_get_prop_string(ctx, -1, "zero");
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_dot(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    duk_push_number(ctx, lhsx * rhsx + lhsy * rhsy + lhsz * rhsz);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_project(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_projectOnPlane(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_angle(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_signedAngle(duk_context *ctx) {
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_distance(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    float x = lhsx - rhsx;
    float y = lhsy - rhsy;
    float z = lhsz - rhsz;
    duk_push_number(ctx, sqrt(x * x + y * y + z * z));
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_static_clampMagnitude(duk_context *ctx) {
    float x, y, z;
    float maxLength;
    duk_unity_get3f(ctx, 0, &x, &y, &z);
    maxLength = (float)duk_get_number_default(ctx, 1, 0);
    float sqrMag = x * x + y * y + z * z;
    if (sqrMag > maxLength * maxLength) {
        float rx = maxLength / sqrMag;
        duk_builtins_reg_get(ctx, "Vector3");
        duk_push_number(ctx, x * rx);
        duk_push_number(ctx, y * rx);
        duk_push_number(ctx, z * rx);
        duk_new(ctx, 3);
        return 1;
    }
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, x);
    duk_push_number(ctx, y);
    duk_push_number(ctx, z);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_magnitude(duk_context *ctx) {
    float x, y, z;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &x, &y, &z);
    duk_push_number(ctx, sqrt(x * x + y * y + z * z));
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_sqrMagnitude(duk_context *ctx) {
    float x, y, z;
    duk_push_this(ctx);
    duk_unity_get3f(ctx, -1, &x, &y, &z);
    duk_push_number(ctx, x * x + y * y + z * z);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_min(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, lhsx > rhsx ? rhsx : lhsx);
    duk_push_number(ctx, lhsy > rhsy ? rhsy : lhsy);
    duk_push_number(ctx, lhsz > rhsz ? rhsz : lhsz);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_max(duk_context *ctx) {
    float lhsx, lhsy, lhsz;
    float rhsx, rhsy, rhsz;
    duk_unity_get3f(ctx, 0, &lhsx, &lhsy, &lhsz);
    duk_unity_get3f(ctx, 1, &rhsx, &rhsy, &rhsz);
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, lhsx < rhsx ? rhsx : lhsx);
    duk_push_number(ctx, lhsy < rhsy ? rhsy : lhsy);
    duk_push_number(ctx, lhsz < rhsz ? rhsz : lhsz);
    duk_new(ctx, 3);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_getx(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 0);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_setx(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 0);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_gety(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 1);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_sety(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 1);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_getz(duk_context *ctx) {
    duk_push_this(ctx);
    duk_get_prop_index(ctx, -1, 2);
    duk_remove(ctx, -2);
    return 1;
}

DUK_LOCAL duk_ret_t duk_unity_vector3_setz(duk_context *ctx) {
    duk_push_this(ctx);
    duk_dup(ctx, 0);
    duk_put_prop_index(ctx, -2, 2);
    duk_pop(ctx);
    return 0;
}

DUK_LOCAL void duk_unity_vector3_add_const(duk_context *ctx, duk_idx_t idx, const char *key, float x, float y, float z) {
    idx = duk_normalize_index(ctx, idx);
    duk_builtins_reg_get(ctx, "Vector3");
    duk_push_number(ctx, x);
    duk_push_number(ctx, y);
    duk_push_number(ctx, z);
    duk_new(ctx, 3);
    duk_put_prop_string(ctx, idx, key);
}

DUK_INTERNAL void duk_unity_vector3_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");
    duk_unity_begin_class(ctx, "Vector3", duk_unity_vector3_constructor, NULL);
    duk_unity_add_member(ctx, "Lerp", duk_unity_vector3_static_lerp, -1);
    duk_unity_add_member(ctx, "LerpUnclamped", duk_unity_vector3_static_lerpUnclamped, -2);
    duk_unity_add_member(ctx, "MoveTowards", duk_unity_vector3_static_moveTowards, -2);
    duk_unity_add_member(ctx, "SmoothDamp", duk_unity_vector3_static_smoothDamp, -2);
    duk_unity_add_member(ctx, "Add", duk_unity_vector3_static_Add, -2);
    duk_unity_add_member(ctx, "Sub", duk_unity_vector3_static_Sub, -2);
    duk_unity_add_member(ctx, "Neg", duk_unity_vector3_static_Neg, -2);
    duk_unity_add_member(ctx, "Mul", duk_unity_vector3_static_Mul, -2);
    duk_unity_add_member(ctx, "Div", duk_unity_vector3_static_Div, -2);
    duk_unity_add_member(ctx, "Scale", duk_unity_vector3_scale, -1);
    duk_unity_add_member(ctx, "Scale", duk_unity_vector3_static_scale, -2);
    duk_unity_add_member(ctx, "Cross", duk_unity_vector3_static_cross, -2);
    duk_unity_add_member(ctx, "Reflect", duk_unity_vector3_static_reflect, -2);
    duk_unity_add_member(ctx, "Normalize", duk_unity_vector3_normalize, -1);
    duk_unity_add_property(ctx, "normalized", duk_unity_vector3_normalized, NULL, -1);
    duk_unity_add_member(ctx, "Dot", duk_unity_vector3_static_dot, -2);
    duk_unity_add_member(ctx, "Project", duk_unity_vector3_static_project, -2);
    duk_unity_add_member(ctx, "ProjectOnPlane", duk_unity_vector3_static_projectOnPlane, -2);
    duk_unity_add_member(ctx, "Angle", duk_unity_vector3_static_angle, -2);
    duk_unity_add_member(ctx, "SignedAngle", duk_unity_vector3_static_signedAngle, -2);
    duk_unity_add_member(ctx, "Distance", duk_unity_vector3_static_distance, -2);
    duk_unity_add_member(ctx, "ClampMagnitude", duk_unity_vector3_static_clampMagnitude, -2);
    duk_unity_add_property(ctx, "magnitude", duk_unity_vector3_magnitude, NULL, -1);
    duk_unity_add_property(ctx, "sqrMagnitude", duk_unity_vector3_sqrMagnitude, NULL, -1);
    duk_unity_add_member(ctx, "Min", duk_unity_vector3_min, -1);
    duk_unity_add_member(ctx, "Max", duk_unity_vector3_max, -1);
    duk_unity_add_property(ctx, "x", duk_unity_vector3_getx, duk_unity_vector3_setx, -1);
    duk_unity_add_property(ctx, "y", duk_unity_vector3_gety, duk_unity_vector3_sety, -1);
    duk_unity_add_property(ctx, "z", duk_unity_vector3_getz, duk_unity_vector3_setz, -1);
    duk_unity_vector3_add_const(ctx, -2, "zero", 0.0, 0.0, 0.0);
    duk_unity_vector3_add_const(ctx, -2, "one", 1.0, 1.0, 1.0);
    duk_unity_vector3_add_const(ctx, -2, "forward", 0.0, 0.0, 1.0);
    duk_unity_vector3_add_const(ctx, -2, "back", 0.0, 0.0, -1.0);
    duk_unity_vector3_add_const(ctx, -2, "up", 0.0, 1.0, 0.0);
    duk_unity_vector3_add_const(ctx, -2, "down", 0.0, -1.0, 0.0);
    duk_unity_vector3_add_const(ctx, -2, "left", -1.0, 0.0, 0.0);
    duk_unity_vector3_add_const(ctx, -2, "right", 1.0, 0.0, 0.0);
    duk_unity_end_class(ctx);
    duk_pop_2(ctx); // pop DuktapeJS and global
}
