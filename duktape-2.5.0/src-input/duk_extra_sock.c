//
#include "duk_internal.h"
#include "ikcp.h"

#ifdef DUK_F_WINDOWS
#define DUK_SOCK_CLOSE(fd) closesocket((fd))
#define WIN32_LEAN_AND_MEAN
#include <winsock2.h>
#include <ws2tcpip.h>
#include <Windows.h>
// https://docs.microsoft.com/en-us/windows/desktop/WinSock/windows-sockets-error-codes-2
#else

#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>
#include <netinet/in.h>
#include <arpa/inet.h>
#include <sys/select.h>

#define INVALID_SOCKET  -1
#define DUK_SOCK_CLOSE(fd) close((fd))
#endif

#define DUK_SOCK_ERROR int

enum duk_sock_state {
	EDUK_SOCKSTATE_CREATED = 0,
	EDUK_SOCKSTATE_CONNECTING = 1,
	EDUK_SOCKSTATE_CONNECTED = 2,
	EDUK_SOCKSTATE_CLOSED = 3,
};

enum duk_sock_error {
	EDUK_SOCKERR_AGAIN = 0,
	EDUK_SOCKERR_FAIL = -1,
	EDUK_SOCKERR_CLOSED = -2,		// 已断开时 send/recv
	EDUK_SOCKERR_DUPCONNECT = -3,	// 未断开时再次 connect
	EDUK_SOCKERR_UNSUPPORTED = -4,
	EDUK_SOCKERR_RESET = -5,
	EDUK_SOCKERR_DNS = -6,
};

enum duk_sock_type {
	EDUK_SOCKTYPE_TCP = 0,
	EDUK_SOCKTYPE_UDP = 1,
};

enum duk_sock_family {
	EDUK_SOCKFAMILY_IPV4 = 0,
	EDUK_SOCKFAMILY_IPV6 = 1,
};

struct duk_sock_t {
#ifdef DUK_F_WINDOWS
	SOCKET fd;
#else
	int fd;
#endif
	enum duk_sock_state state;
	enum duk_sock_type type;
	enum duk_sock_family family;
};

// family: AF_INET, AF_INET6
// type: SOCK_STREAM, SOCK_DGRAM
// protocol: IPPROTO_TCP, IPPROTO_UDP
DUK_LOCAL DUK_INLINE struct duk_sock_t* _duk_sock_create(duk_context *ctx, enum duk_sock_type type, enum duk_sock_family family) {
	int sock_type, sock_proto;
	int sock_af = family == EDUK_SOCKFAMILY_IPV4 ? AF_INET : AF_INET6;
	if (type == EDUK_SOCKTYPE_TCP) {
		sock_type = SOCK_STREAM;
		sock_proto = IPPROTO_TCP;
	}
	else {
		sock_type = SOCK_DGRAM;
		sock_proto = IPPROTO_UDP;
	}
#ifdef DUK_F_WINDOWS
	SOCKET fd = socket(sock_af, sock_type, sock_proto);
	if (fd == INVALID_SOCKET) {
		return NULL;
	}
#else 
	int fd = socket(sock_af, sock_type, sock_proto);
	if (fd < 0) {
		return NULL;
	}
#endif
	struct duk_sock_t* sock = (struct duk_sock_t*)duk_alloc(ctx, sizeof(struct duk_sock_t));
	if (!sock) {
		DUK_SOCK_CLOSE(fd);
		return NULL;
	}
	memset(sock, 0, sizeof(struct duk_sock_t));
	sock->fd = fd;
	sock->state = EDUK_SOCKSTATE_CREATED;
	sock->type = type;
	sock->family = family;
	return sock;
}

DUK_LOCAL DUK_INLINE void _duk_sock_close(struct duk_sock_t* sock) {
	if (sock) {
		if (sock->fd != INVALID_SOCKET) {
			DUK_SOCK_CLOSE(sock->fd);
			sock->fd = INVALID_SOCKET;
		}
		sock->state = EDUK_SOCKSTATE_CLOSED;
	}
}

DUK_LOCAL DUK_INLINE void _duk_sock_destroy(duk_context *ctx, struct duk_sock_t *sock) {
	_duk_sock_close(sock);
    duk_free(ctx, sock);
}

DUK_LOCAL DUK_INLINE void _duk_sock_setnonblocking(struct duk_sock_t* sock) {
#ifdef DUK_F_WINDOWS
	u_long argp = 1;
	ioctlsocket(sock->fd, FIONBIO, &argp);
#else 
	int flags = fcntl(sock->fd, F_GETFL, 0);
	flags |= O_NONBLOCK;
	fcntl(sock->fd, F_SETFL, flags);
#endif
}

DUK_LOCAL DUK_INLINE DUK_SOCK_ERROR _duk_sock_connect(struct duk_sock_t *sock, struct sockaddr *addr, int port) {
	if (sock->state != EDUK_SOCKSTATE_CREATED && sock->state != EDUK_SOCKSTATE_CLOSED) {
		return EDUK_SOCKERR_DUPCONNECT;
	}
	int res = -1;
	switch (addr->sa_family) {
	case AF_INET: {
		struct sockaddr_in sa;
		memcpy(&sa, addr, sizeof(struct sockaddr_in));
		sa.sin_port = htons(port);
		sock->state = EDUK_SOCKSTATE_CONNECTING;
		res = connect(sock->fd, (const struct sockaddr *)&sa, sizeof(struct sockaddr_in));
		break;
	}
	case AF_INET6: {
		struct sockaddr_in6 sa;
		memcpy(&sa, addr, sizeof(struct sockaddr_in6));
		sa.sin6_port = htons(port);
		res = connect(sock->fd, (const struct sockaddr *)&sa, sizeof(struct sockaddr_in6));
		break;
	}
	default: return EDUK_SOCKERR_UNSUPPORTED;
	}
	if (res < 0) {
#ifdef DUK_F_WINDOWS
		int err = WSAGetLastError();
		if (err != WSAEWOULDBLOCK && err != WSAEINPROGRESS) {
			_duk_sock_close(sock);
			return EDUK_SOCKERR_FAIL;
		}
#else
		int err = errno;
		while ((err = errno) == EINTR);
		if (err != EINPROGRESS && err != EAGAIN) {
			_duk_sock_close(sock);
			return EDUK_SOCKERR_FAIL;
		}
#endif
		sock->state = EDUK_SOCKSTATE_CONNECTING;
	}
	else {
		sock->state = EDUK_SOCKSTATE_CONNECTED;
	}
	return EDUK_SOCKERR_AGAIN;
}

DUK_LOCAL DUK_INLINE DUK_SOCK_ERROR _duk_sock_connect_host(struct duk_sock_t* sock, const char* host, int port) {
	struct addrinfo hints;
	struct addrinfo* resolved = NULL, * iter = NULL;
	struct sockaddr* result = NULL;
	memset(&hints, 0, sizeof(hints));

	hints.ai_socktype = sock->type == EDUK_SOCKTYPE_TCP ? SOCK_STREAM : SOCK_DGRAM;
	hints.ai_protocol = sock->type == EDUK_SOCKTYPE_TCP ? IPPROTO_TCP : IPPROTO_UDP;
	hints.ai_family = sock->family == EDUK_SOCKFAMILY_IPV4 ? AF_INET : AF_INET6;
	hints.ai_flags = 0;
	int res = getaddrinfo(host, NULL, &hints, &resolved);
	if (!res) {
		for (iter = resolved; iter; iter = iter->ai_next) {
			result = iter->ai_addr;
			break;
		}
		if (result) {
			int retval = _duk_sock_connect(sock, result, 1234);
			freeaddrinfo(resolved);
			return retval;
		}
	}
	freeaddrinfo(resolved);
	return EDUK_SOCKERR_DNS;
}

DUK_LOCAL DUK_INLINE DUK_SOCK_ERROR _duk_sock_connecting(struct duk_sock_t* sock) {
	fd_set wset;
	FD_ZERO(&wset);
	FD_SET(sock->fd, &wset);
	struct timeval tv;
	tv.tv_sec = 0;
	tv.tv_usec = 0;
	int res = select(sock->fd + 1, NULL, &wset, NULL, &tv);
	if (res < 0) {
		_duk_sock_close(sock);
		return EDUK_SOCKERR_FAIL;
	}
	if (res == 0) {
		// timeout
		return EDUK_SOCKERR_AGAIN;
	}
	if (FD_ISSET(sock->fd, &wset)) {
		int error = 0;
		socklen_t len = sizeof(error);
		if (getsockopt(sock->fd, SOL_SOCKET, SO_ERROR, &error, &len) < 0) {
			_duk_sock_close(sock);
			return EDUK_SOCKERR_FAIL;
		}
		if (error != 0) {
			_duk_sock_close(sock);
			return EDUK_SOCKERR_FAIL;
		}
		else {
			sock->state = EDUK_SOCKSTATE_CONNECTED;
		}
	}
	return EDUK_SOCKERR_AGAIN;
}

DUK_LOCAL DUK_INLINE DUK_SOCK_ERROR _duk_sock_recv(struct duk_sock_t *sock, char *buf, int buf_size, int *recv_size) {
	*recv_size = 0;
	if (sock->state == EDUK_SOCKSTATE_CLOSED || sock->state == EDUK_SOCKSTATE_CREATED) {
		return EDUK_SOCKERR_CLOSED;
	}
	if (sock->state == EDUK_SOCKSTATE_CONNECTING) {
		return _duk_sock_connecting(sock);
	}
#ifdef DUK_F_WINDOWS
	int prev = 0;
	for (;;) {
		int res = recv(sock->fd, buf, buf_size, 0);
		if (res > 0) {
			*recv_size = res;
			return EDUK_SOCKERR_AGAIN;
		}
		if (res < 0) {
			int err = WSAGetLastError();
			if (err != WSAEWOULDBLOCK) {
				if (err != WSAECONNRESET || prev == WSAECONNRESET) {
					_duk_sock_close(sock);
					return EDUK_SOCKERR_FAIL;
				}
				prev = err;
				continue;
			}
			return EDUK_SOCKERR_AGAIN;
		}
		_duk_sock_close(sock);
		return EDUK_SOCKERR_RESET;
	}
#else
	int res = recv(sock->fd, buf, buf_size, 0);
	if (res > 0) {
		*recv_size = res;
		return EDUK_SOCKERR_AGAIN;
	}
	if (res < 0) {
		int err = errno;
		if (err != EAGAIN && err != EINTR && err != EWOULDBLOCK) {
			_duk_sock_close(sock);
			return EDUK_SOCKERR_RESET;
		}
		return EDUK_SOCKERR_AGAIN;
	}
	_duk_sock_close(sock);
	return EDUK_SOCKERR_RESET;
#endif
}

DUK_LOCAL DUK_INLINE DUK_SOCK_ERROR _duk_sock_send(struct duk_sock_t *sock, const char *buf, int buf_size, int *sent_size) {
	*sent_size = 0;
	if (sock->state == EDUK_SOCKSTATE_CLOSED || sock->state == EDUK_SOCKSTATE_CREATED) {
		return EDUK_SOCKERR_CLOSED;
	}
	if (sock->state == EDUK_SOCKSTATE_CONNECTING) {
		return _duk_sock_connecting(sock);
	}
	int res = send(sock->fd, buf, buf_size, 0);
	if (res >= 0) {
		*sent_size = res;
		return EDUK_SOCKERR_AGAIN;
	}
#ifdef DUK_F_WINDOWS
	int err = WSAGetLastError();
	if (err != WSAEWOULDBLOCK) { //TODO: ! (errno == EINTR || errno == EWOULDBLOCK || errno == EAGAIN)
		_duk_sock_close(sock);
		return EDUK_SOCKERR_RESET;
	}
	return EDUK_SOCKERR_AGAIN;
#else
	int err = errno;
	if (err == EPIPE || (err != EPROTOTYPE && err != EINTR && err != EAGAIN)) {
		_duk_sock_close(sock);
		return EDUK_SOCKERR_RESET;
	}
	return EDUK_SOCKERR_AGAIN;
#endif
}

DUK_LOCAL duk_ret_t duk_sock_constructor(duk_context* ctx) {
	duk_int_t type = duk_require_int(ctx, 0);
	duk_int_t family = duk_require_int(ctx, 1);

	duk_push_this(ctx);
	struct duk_sock_t *sock = _duk_sock_create(ctx, type, family);
	duk_push_pointer(ctx, sock);
	duk_put_prop_literal(ctx, -2, DUK_HIDDEN_SYMBOL("_sock"));
	duk_pop(ctx); // pop this
	return 0;
}

DUK_LOCAL void duk_sock_finalizer(duk_context* ctx) {
	duk_get_prop_literal(ctx, 0, DUK_HIDDEN_SYMBOL("_sock"));
	struct duk_sock_t* sock = (struct duk_sock_t*)duk_to_pointer(ctx, -1);
	duk_pop(ctx); // pop sock
	duk_del_prop_literal(ctx, 0, DUK_HIDDEN_SYMBOL("_sock"));
	_duk_sock_destroy(ctx, sock);
}

DUK_LOCAL duk_ret_t duk_sock_connect(duk_context* ctx) {
	const char* host = duk_require_string(ctx, 0);
	duk_int_t port = duk_require_int(ctx, 1);
	duk_push_this(ctx);
	duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_sock"));
	struct duk_sock_t *sock = (struct duk_sock_t *)duk_to_pointer(ctx, -1);
	duk_pop_2(ctx); // pop sock and this
	if (!sock) {
		return duk_generic_error(ctx, "invalid socket");
	}
	int ret = _duk_sock_connect_host(sock, host, port);
	duk_push_int(ctx, ret);
	return 1;
}

DUK_LOCAL duk_ret_t duk_sock_close(duk_context* ctx) {
	duk_push_this(ctx);
	duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_sock"));
	struct duk_sock_t* sock = (struct duk_sock_t*)duk_to_pointer(ctx, -1);
	duk_pop_2(ctx); // pop sock and this 
	if (sock) {
		duk_del_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_sock"));
		_duk_sock_close(sock);
	}
	return 0;
}

DUK_LOCAL duk_ret_t duk_sock_setnonblocking(duk_context* ctx) {
	duk_push_this(ctx);
	duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_sock"));
	struct duk_sock_t* sock = (struct duk_sock_t*)duk_to_pointer(ctx, -1);
	duk_pop_2(ctx); // pop sock and this 
	if (sock) {
		_duk_sock_setnonblocking(sock);
	}
	return 0;
}

DUK_LOCAL duk_ret_t duk_sock_send(duk_context* ctx) {
	if (duk_is_string(ctx, 0)) {
		duk_size_t length;
		const char* buffer = duk_require_lstring(ctx, 0, &length);
		if (length == 0) {
			duk_push_int(ctx, 0);
			return 1;
		}
		duk_push_this(ctx);
		duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_sock"));
		struct duk_sock_t* sock = (struct duk_sock_t*)duk_to_pointer(ctx, -1);
		duk_pop_2(ctx); // pop sock and this 
		if (sock) {
			int sent_size = 0;
			int retval = _duk_sock_send(sock, buffer, length, &sent_size);
			if (retval < 0) {
				duk_push_int(ctx, retval);
			}
			else {
				duk_push_int(ctx, sent_size);
			}
		}
		else {
			duk_push_int(ctx, -1);
		}
		return 1;
	}
	else {
		duk_size_t size;
		char* buffer = duk_require_buffer_data(ctx, 0, &size);
		duk_int_t index = duk_require_int(ctx, 1);
		duk_int_t length = duk_require_int(ctx, 2);
		if (index + length > size) {
			return duk_generic_error(ctx, "buffer overflow");
		}
		if (length == 0) {
			duk_push_int(ctx, 0);
			return 1;
		}
		duk_push_this(ctx);
		duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_sock"));
		struct duk_sock_t* sock = (struct duk_sock_t*)duk_to_pointer(ctx, -1);
		duk_pop_2(ctx); // pop sock and this 
		if (sock) {
			int sent_size = 0;
			int retval = _duk_sock_send(sock, buffer + index, length, &sent_size);
			if (retval < 0) {
				duk_push_int(ctx, retval);
			}
			else {
				duk_push_int(ctx, sent_size);
			}
		}
		else {
			duk_push_int(ctx, -1);
		}
		return 1;
	}
}

DUK_LOCAL duk_ret_t duk_sock_recv(duk_context* ctx) {
	duk_size_t size;
	char* buffer = duk_require_buffer_data(ctx, 0, &size);
	duk_int_t index = duk_require_int(ctx, 1);
	duk_int_t length = duk_require_int(ctx, 2);
	if (index + length > size) {
		return duk_generic_error(ctx, "buffer overflow");
	}
	duk_push_this(ctx);
	duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_sock"));
	struct duk_sock_t* sock = (struct duk_sock_t*)duk_to_pointer(ctx, -1);
	duk_pop_2(ctx); // pop sock and this 
	if (sock) {
		int recv_size = 0;
		int retval = _duk_sock_recv(sock, buffer + index, length, &recv_size);
		if (retval < 0) {
			duk_push_int(ctx, retval);
		}
		else {
			duk_push_int(ctx, recv_size);
		}
	}
	else {
		duk_push_int(ctx, -1);
	}
	return 1;
}

struct duk_kcp_t {
	ikcpcb *kcp;
	char *buffer;
	int buffer_size;
};

DUK_LOCAL DUK_INLINE int _duk_kcp_output(const char *buf, int len, ikcpcb *kcp, void *user) {
	struct duk_sock_t *sock = (struct duk_sock_t *)user;
	if (sock) {
		_duk_sock_send(sock, buf, 0, len);
	}
	return 0;
}

DUK_LOCAL DUK_INLINE struct duk_kcp_t *_duk_kcp_create(duk_context *ctx, duk_uint_t conv, enum duk_sock_family family, int buffer_size) {
	struct duk_sock_t *sock = _duk_sock_create(ctx, EDUK_SOCKTYPE_UDP, family);
	if (sock) {
		char *buffer = (char *)duk_alloc(ctx, buffer_size);
		if (buffer) {
			struct duk_kcp_t *kcp = (struct duk_kcp_t *)duk_alloc(ctx, sizeof(struct duk_kcp_t *));
			if (kcp) {
				ikcpcb *_kcp = ikcp_create(conv, sock);
				if (_kcp) {
					kcp->kcp = _kcp;
					kcp->buffer = buffer;
					kcp->buffer_size = buffer_size;
					_kcp->output = _duk_kcp_output;
					return kcp;
				} else {
					duk_free(ctx, kcp);
					_duk_sock_destroy(ctx, sock);
					duk_free(ctx, buffer);
				}
			} else {
				_duk_sock_destroy(ctx, sock);
				duk_free(ctx, buffer);
			}
		} else {
			_duk_sock_destroy(ctx, sock);
		}
	}
	return NULL;
}

DUK_LOCAL DUK_INLINE void _duk_kcp_destroy(duk_context *ctx, struct duk_kcp_t *kcp) {
	_duk_sock_destroy(ctx, (struct duk_sock_t *)(kcp->kcp->user));
	ikcp_release(kcp->kcp);
	duk_free(ctx, kcp->buffer);
	duk_free(ctx, kcp);
}

DUK_LOCAL duk_ret_t duk_kcp_constructor(duk_context *ctx) {
	if (duk_get_top(ctx) < 3) {
		return duk_generic_error(ctx, "error, need 3 args.");
	}
	duk_uint_t conv = duk_require_uint(ctx, 0);
	duk_int_t family = duk_require_int(ctx, 1);
	duk_uint_t buffer_size = duk_require_int(ctx, 2);
	
	duk_push_this(ctx);
	struct duk_kcp_t *kcp = _duk_kcp_create(ctx, conv, family, buffer_size);
	duk_push_pointer(ctx, kcp);
	duk_put_prop_literal(ctx, -2, DUK_HIDDEN_SYMBOL("_kcp"));
	duk_pop(ctx); // pop this
	return 0;
}

DUK_LOCAL void duk_kcp_finalizer(duk_context* ctx) {
	duk_get_prop_literal(ctx, 0, DUK_HIDDEN_SYMBOL("_kcp"));
	struct duk_kcp_t *kcp = (struct duk_kcp_t *)duk_to_pointer(ctx, -1);
	duk_pop(ctx); 
	duk_del_prop_literal(ctx, 0, DUK_HIDDEN_SYMBOL("_kcp"));
	_duk_kcp_destroy(ctx, kcp);
}

DUK_LOCAL duk_ret_t duk_kcp_wndsize(duk_context *ctx) {
	duk_int_t sndwnd = duk_require_int(ctx, 0);
	duk_int_t rcvwnd = duk_require_int(ctx, 1);

	duk_push_this(ctx);
	duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_kcp"));
	struct duk_kcp_t *kcp = (struct duk_kcp_t *)duk_to_pointer(ctx, -1);
	duk_pop_2(ctx); // pop kcp and this
	if (!kcp) {
		return duk_generic_error(ctx, "invalid kcp");
	}
	int retval = ikcp_wndsize(kcp->kcp, sndwnd, rcvwnd);
	duk_push_int(ctx, retval);
	return 1;
}

DUK_LOCAL duk_ret_t duk_kcp_nodelay(duk_context *ctx) {
	duk_int_t nodelay = duk_require_int(ctx, 0);
	duk_int_t interval = duk_require_int(ctx, 1);
	duk_int_t resend = duk_require_int(ctx, 2);
	duk_int_t nc = duk_require_int(ctx, 3);

	duk_push_this(ctx);
	duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_kcp"));
	struct duk_kcp_t *kcp = (struct duk_kcp_t *)duk_to_pointer(ctx, -1);
	duk_pop_2(ctx); // pop kcp and this
	if (!kcp) {
		return duk_generic_error(ctx, "invalid kcp");
	}
	int retval = ikcp_nodelay(kcp->kcp, nodelay, interval, resend, nc);
	duk_push_int(ctx, retval);
	return 1;
}

DUK_LOCAL duk_ret_t duk_kcp_update(duk_context *ctx) {
	duk_uint_t current = duk_require_uint(ctx, 0);

	duk_push_this(ctx);
	duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_kcp"));
	struct duk_kcp_t *kcp = (struct duk_kcp_t *)duk_to_pointer(ctx, -1);
	duk_pop_2(ctx); // pop kcp and this
	if (!kcp) {
		return duk_generic_error(ctx, "invalid kcp");
	}
	ikcp_update(kcp->kcp, current);
	return 0;
}

DUK_LOCAL duk_ret_t duk_kcp_send(duk_context *ctx) {
	if (duk_is_string(ctx, 0)) {
		duk_size_t length;
		const char* buffer = duk_require_lstring(ctx, 0, &length);
		if (buffer == NULL || length == 0) {
			return 0;
		}
		duk_push_this(ctx);
		duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_kcp"));
		struct duk_kcp_t *kcp = (struct duk_kcp_t *)duk_to_pointer(ctx, -1);
		duk_pop_2(ctx); // pop kcp and this
		if (!kcp) {
			return duk_generic_error(ctx, "invalid kcp");
		}
		ikcp_send(kcp->kcp, buffer, length);
		return 0;
	} else {
		duk_size_t length;
		char *buffer = duk_require_buffer_data(ctx, 0, &length);
		if (buffer == NULL || length == 0) {
			return 0;
		}

		duk_push_this(ctx);
		duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_kcp"));
		struct duk_kcp_t *kcp = (struct duk_kcp_t *)duk_to_pointer(ctx, -1);
		duk_pop_2(ctx); // pop kcp and this
		if (!kcp) {
			return duk_generic_error(ctx, "invalid kcp");
		}
		ikcp_send(kcp->kcp, buffer, length);
		return 0;
	}
}

DUK_LOCAL duk_ret_t duk_kcp_recv(duk_context *ctx) {
	duk_size_t size;
	char *buffer = duk_require_buffer_data(ctx, 0, &size);
	duk_int_t index = duk_require_int(ctx, 1);
	duk_int_t length = duk_require_int(ctx, 2);
	if (index + length > size) {
		return duk_generic_error(ctx, "buffer overflow");
	}

	duk_push_this(ctx);
	duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_kcp"));
	struct duk_kcp_t *kcp = (struct duk_kcp_t *)duk_to_pointer(ctx, -1);
	duk_pop_2(ctx); // pop kcp and this
	if (!kcp) {
		return duk_generic_error(ctx, "invalid kcp");
	}
	int recv_size = 0;
	int retval = _duk_sock_recv((struct duk_sock_t *)(kcp->kcp->user), kcp->buffer, kcp->buffer_size, &recv_size);
	if (retval <= 0) {
		duk_push_int(ctx, retval);
	} else {
		ikcp_input(kcp->kcp, kcp->buffer, recv_size);
		int kcp_ret = ikcp_recv(kcp->kcp, buffer + index, length);
		if (kcp_ret > 0) {
			duk_push_int(ctx, kcp_ret);
		} else {
			duk_push_int(ctx, 0);
		}
	}
	return 1;
}

DUK_LOCAL duk_ret_t duk_kcp_connect(duk_context* ctx) {
	const char* host = duk_require_string(ctx, 0);
	duk_int_t port = duk_require_int(ctx, 1);
	duk_push_this(ctx);
	duk_get_prop_literal(ctx, -1, DUK_HIDDEN_SYMBOL("_kcp"));
	struct duk_kcp_t *kcp = (struct duk_kcp_t *)duk_to_pointer(ctx, -1);
	duk_pop_2(ctx); // pop sock and this
	if (!kcp) {
		return duk_generic_error(ctx, "invalid kcp");
	}
	int ret = _duk_sock_connect_host((struct duk_sock_t *)(kcp->kcp->user), host, port);
	duk_push_int(ctx, ret);
	return 1;
}

DUK_INTERNAL duk_bool_t duk_sock_open(duk_context *ctx) {
    duk_push_global_object(ctx);
    duk_unity_get_prop_object(ctx, -1, "DuktapeJS");

    {
        duk_push_object(ctx);
        duk_push_int(ctx, EDUK_SOCKTYPE_TCP);
        duk_put_prop_literal(ctx, -2, "TCP");
        duk_push_int(ctx, EDUK_SOCKTYPE_UDP);
        duk_put_prop_literal(ctx, -2, "UDP");
        duk_put_prop_literal(ctx, -2, "SocketType");

        duk_push_object(ctx);
        duk_push_int(ctx, EDUK_SOCKFAMILY_IPV4);
        duk_put_prop_literal(ctx, -2, "IPV4");
        duk_push_int(ctx, EDUK_SOCKFAMILY_IPV6);
        duk_put_prop_literal(ctx, -2, "IPV6");
        duk_put_prop_literal(ctx, -2, "SocketFamily");

        duk_unity_begin_class(ctx, "Socket", DUK_UNITY_BUILTINS_SOCKET, duk_sock_constructor, duk_sock_finalizer);
        duk_push_c_function(ctx, duk_sock_connect, 2);
        duk_put_prop_literal(ctx, -2, "connect");
        // duk_push_c_function(ctx, duk_sock_connect_addr, 2);
        // duk_put_prop_literal(ctx, -2, "connect_addr");
        duk_push_c_function(ctx, duk_sock_close, 0);
        duk_put_prop_literal(ctx, -2, "close");
        duk_push_c_function(ctx, duk_sock_setnonblocking, 0);
        duk_put_prop_literal(ctx, -2, "setnonblocking");
        duk_push_c_function(ctx, duk_sock_send, DUK_VARARGS);
        duk_put_prop_literal(ctx, -2, "send");
        duk_push_c_function(ctx, duk_sock_recv, 3);
        duk_put_prop_literal(ctx, -2, "recv");
        duk_unity_end_class(ctx);

        duk_unity_begin_class(ctx, "Kcp", DUK_UNITY_BUILTINS_KCP, duk_kcp_constructor, duk_kcp_finalizer);
        duk_push_c_function(ctx, duk_kcp_connect, 2);
        duk_put_prop_literal(ctx, -2, "connect");
		duk_push_c_function(ctx, duk_kcp_wndsize, 2);
		duk_put_prop_literal(ctx, -2, "wndsize");
		duk_push_c_function(ctx, duk_kcp_nodelay, 4);
		duk_put_prop_literal(ctx, -2, "nodelay");
		duk_push_c_function(ctx, duk_kcp_update, 1);
		duk_put_prop_literal(ctx, -2, "update");
		duk_push_c_function(ctx, duk_kcp_send, 2);
		duk_put_prop_literal(ctx, -2, "send");
		duk_push_c_function(ctx, duk_kcp_recv, 3);
		duk_put_prop_literal(ctx, -2, "recv");
		// property: rx_minrto
		// property: fastresend
        duk_unity_end_class(ctx);
    }

    duk_pop_2(ctx); // pop DuktapeJS and global    
#ifdef DUK_F_WINDOWS
    WSADATA wsaData;
	WORD wVersionRequested = MAKEWORD(2, 2);
	int err = WSAStartup(wVersionRequested, &wsaData);
	if (err != 0) {
		// failed
		return 0;
	}
#endif
    return 1;
}