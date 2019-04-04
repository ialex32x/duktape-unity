//
#include "duk_internal.h"

#include <time.h>
#include <stdio.h>
#include <limits.h>
#include <float.h>

#ifdef DUK_F_WINDOWS
#   include <windows.h>
// #include <winsock2.h>
// #include <ws2tcpip.h>
#else
#   include <sys/time.h>
#endif

/* min and max macros */
#ifndef MIN
#define MIN(x, y) ((x) < (y) ? x : y)
#endif
#ifndef MAX
#define MAX(x, y) ((x) > (y) ? x : y)
#endif

DUK_INTERNAL duk_bool_t duk_sock_open(duk_context *ctx) {
    // WSADATA wsaData;
    // WORD wVersionRequested = MAKEWORD(2, 0); 
    // int err = WSAStartup(wVersionRequested, &wsaData);
    // if (err != 0) {
    //     return 0;
    // }
    // if ((LOBYTE(wsaData.wVersion) != 2 || HIBYTE(wsaData.wVersion) != 0) &&
    //     (LOBYTE(wsaData.wVersion) != 1 || HIBYTE(wsaData.wVersion) != 1)) {
    //     WSACleanup();
    //     return 0; 
    // }
    return 1;
}

DUK_INTERNAL double timeout_gettime(void) {
#ifdef DUK_F_WINDOWS
    FILETIME ft;
    double t;
    GetSystemTimeAsFileTime(&ft);
    /* Windows file time (time since January 1, 1601 (UTC)) */
    t  = ft.dwLowDateTime/1.0e7 + ft.dwHighDateTime*(4294967296.0/1.0e7);
    /* convert to Unix Epoch time (time since January 1, 1970 (UTC)) */
    return (t - 11644473600.0);
#else
    struct timeval v;
    gettimeofday(&v, NULL);
    /* Unix Epoch time (time since January 1, 1970 (UTC)) */
    return v.tv_sec + v.tv_usec / 1.0e6;
#endif
}

/*-------------------------------------------------------------------------*\
* Determines how much time we have left for the next system call,
* if the previous call was successful 
* Input
*   tm: timeout control structure
* Returns
*   the number of ms left or -1 if there is no time limit
\*-------------------------------------------------------------------------*/
DUK_INTERNAL double timeout_get(p_timeout tm) {
    if (tm->block < 0.0 && tm->total < 0.0) {
        return -1;
    } else if (tm->block < 0.0) {
        double t = tm->total - timeout_gettime() + tm->start;
        return MAX(t, 0.0);
    } else if (tm->total < 0.0) {
        return tm->block;
    } else {
        double t = tm->total - timeout_gettime() + tm->start;
        return MIN(tm->block, MAX(t, 0.0));
    }
}

/*-------------------------------------------------------------------------*\
* Returns time since start of operation
* Input
*   tm: timeout control structure
* Returns
*   start field of structure
\*-------------------------------------------------------------------------*/
double timeout_getstart(p_timeout tm) {
    return tm->start;
}

/*-------------------------------------------------------------------------*\
* Determines how much time we have left for the next system call,
* if the previous call was a failure
* Input
*   tm: timeout control structure
* Returns
*   the number of ms left or -1 if there is no time limit
\*-------------------------------------------------------------------------*/
double timeout_getretry(p_timeout tm) {
    if (tm->block < 0.0 && tm->total < 0.0) {
        return -1;
    } else if (tm->block < 0.0) {
        double t = tm->total - timeout_gettime() + tm->start;
        return MAX(t, 0.0);
    } else if (tm->total < 0.0) {
        double t = tm->block - timeout_gettime() + tm->start;
        return MAX(t, 0.0);
    } else {
        double t = tm->total - timeout_gettime() + tm->start;
        return MIN(tm->block, MAX(t, 0.0));
    }
}

/*-------------------------------------------------------------------------*\
* Sets timeout values for IO operations
* Lua Input: base, time [, mode]
*   time: time out value in seconds
*   mode: "b" for block timeout, "t" for total timeout. (default: b)
\*-------------------------------------------------------------------------*/
int timeout_meth_settimeout(duk_context *ctx, p_timeout tm) {
    double t = duk_get_number_default(ctx, 0, -1);
    const char *mode = duk_get_string_default(L, 1, "b");
    switch (*mode) {
        case 'b':
            tm->block = t; 
            break;
        case 'r': case 't':
            tm->total = t;
            break;
        default:
        return duk_generic_error(ctx, "invalid timeout mode");
    }
    duk_push_number(ctx, 1);
    return 1;
}

/*-------------------------------------------------------------------------*\
* Marks the operation start time in structure 
* Input
*   tm: timeout control structure
\*-------------------------------------------------------------------------*/
p_timeout timeout_markstart(p_timeout tm) {
    tm->start = timeout_gettime();
    return tm;
}

DUK_LOCAL duk_ret_t duk_timeout_gettime(duk_context *ctx) {
    duk_push_number(ctx, timeout_gettime());
    return 1;
}

DUK_LOCAL duk_ret_t duk_timeout_sleep(duk_context *ctx) {
    duk_double_t n = duk_require_number(ctx, 0);
#ifdef DUK_F_WINDOWS
    if (n < 0.0) n = 0.0;
    if (n < DBL_MAX/1000.0) n *= 1000.0;
    if (n > INT_MAX) n = INT_MAX;
    Sleep((int)n);
#else
    struct timespec t, r;
    if (n < 0.0) n = 0.0;
    if (n > INT_MAX) n = INT_MAX;
    t.tv_sec = (int) n;
    n -= t.tv_sec;
    t.tv_nsec = (int) (n * 1000000000);
    if (t.tv_nsec >= 1000000000) t.tv_nsec = 999999999;
    while (nanosleep(&t, &r) != 0) {
        t.tv_sec = r.tv_sec;
        t.tv_nsec = r.tv_nsec;
    }
#endif
    return 0;
}

DUK_INTERNAL duk_bool_t duk_timeout_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");

    duk_unity_add_member(ctx, "gettime", duk_timeout_gettime, -1);
    duk_unity_add_member(ctx, "sleep", duk_timeout_sleep, -1);

    duk_pop_2(ctx); // pop DuktapeJS and global    
    return 1;
}
