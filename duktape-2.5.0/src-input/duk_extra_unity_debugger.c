#include "duk_internal.h"

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
#if defined(DUK_USE_DEBUGGER_SUPPORT)    
    duk_debugger_detach(ctx);
    duk_free(ctx, debugger);
#endif
}
