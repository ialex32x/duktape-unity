#if !defined(DUK_TRANS_SOCKET_H_INCLUDED)
#define DUK_TRANS_SOCKET_H_INCLUDED

#if defined(DUK_USE_DEBUGGER_SUPPORT)
DUK_INTERNAL_DECL void duk_trans_socket_init(void);
DUK_INTERNAL_DECL void duk_trans_socket_finish(void);
DUK_INTERNAL_DECL void duk_trans_socket_waitconn(void);
DUK_INTERNAL_DECL duk_size_t duk_trans_socket_read_cb(void *udata, char *buffer, duk_size_t length);
DUK_INTERNAL_DECL duk_size_t duk_trans_socket_write_cb(void *udata, const char *buffer, duk_size_t length);
DUK_INTERNAL_DECL duk_size_t duk_trans_socket_peek_cb(void *udata);
DUK_INTERNAL_DECL void duk_trans_socket_read_flush_cb(void *udata);
DUK_INTERNAL_DECL void duk_trans_socket_write_flush_cb(void *udata);
#endif

#endif  /* DUK_TRANS_SOCKET_H_INCLUDED */
