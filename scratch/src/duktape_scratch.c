/*
 *  Very simple example program
 */

#include "duktape.h"
#include <stdio.h>
#include <stdlib.h>

#define RUN_AS_MODULE
#define duk_memcmp memcmp
#define duk_memcpy memcpy


#ifdef WIN32
#define WIN32_LEAN_AND_MEAN
#include <winsock2.h>
#include <ws2tcpip.h>
#include <Windows.h>
// https://docs.microsoft.com/en-us/windows/desktop/WinSock/windows-sockets-error-codes-2
#else
#define INVALID_SOCKET  -1
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
#if WIN32
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
struct duk_sock_t* duk_sock_create(enum duk_sock_type type, enum duk_sock_family family) {
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
#if WIN32
	SOCKET fd = socket(sock_af, sock_type, sock_proto);
	if (fd == INVALID_SOCKET) {
		return NULL;
	}
#else 
	int fd = socket(af, type, protocol);
	if (fd < 0) {
		return NULL;
	}
#endif
	struct duk_sock_t* sock = (struct duk_sock_t*)malloc(sizeof(struct duk_sock_t));
	if (!sock) {
		close(fd);
		return NULL;
	}
	memset(sock, 0, sizeof(struct duk_sock_t));
	sock->fd = fd;
	sock->state = EDUK_SOCKSTATE_CREATED;
	sock->type = type;
	sock->family = family;
	return sock;
}

void duk_sock_setnonblocking(struct duk_sock_t* sock) {
#if WIN32
	u_long argp = 1;
	ioctlsocket(sock->fd, FIONBIO, &argp);
#else 
	int flags = fcntl(sock->fd, F_GETFL, 0);
	flags |= O_NONBLOCK;
	fcntl(sock->fd, F_SETFL, flags);
#endif
}

void duk_sock_close(struct duk_sock_t* sock) {
	if (sock) {
		if (sock->fd != INVALID_SOCKET) {
			close(sock->fd);
			sock->fd = INVALID_SOCKET;
		}
		sock->state = EDUK_SOCKSTATE_CLOSED;
	}
}

DUK_SOCK_ERROR duk_sock_connect(struct duk_sock_t *sock, struct sockaddr *addr, int port) {
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
#if WIN32
		int err = WSAGetLastError();
		if (err != WSAEWOULDBLOCK && err != WSAEINPROGRESS) {
			duk_sock_close(sock);
			return EDUK_SOCKERR_FAIL;
		}
#else
		int err = errno;
		while ((err = errno) == EINTR);
		if (err != EINPROGRESS && err != EAGAIN) {
			duk_sock_close(sock);
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

DUK_SOCK_ERROR duk_sock_connect_host(struct duk_sock_t* sock, const char* host, int port) {
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
			int retval = duk_sock_connect(sock, result, 1234);
			freeaddrinfo(resolved);
			return retval;
		}
	}
	freeaddrinfo(resolved);
	return EDUK_SOCKERR_DNS;
}

DUK_SOCK_ERROR duk_sock_connecting(struct duk_sock_t* sock) {
	FD_SET wset;
	FD_ZERO(&wset);
	FD_SET(sock->fd, &wset);
	struct timeval tv;
	tv.tv_sec = 0;
	tv.tv_usec = 0;
	int res = select(sock->fd + 1, NULL, &wset, NULL, &tv);
	if (res < 0) {
		duk_sock_close(sock);
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
			duk_sock_close(sock);
			return EDUK_SOCKERR_FAIL;
		}
		if (error != 0) {
			duk_sock_close(sock);
			return EDUK_SOCKERR_FAIL;
		}
		else {
			sock->state = EDUK_SOCKSTATE_CONNECTED;
		}
	}
	return EDUK_SOCKERR_AGAIN;
}

DUK_SOCK_ERROR duk_sock_recv(struct duk_sock_t *sock, char *buf, int buf_size, int *recv_size) {
	*recv_size = 0;
	if (sock->state == EDUK_SOCKSTATE_CLOSED || sock->state == EDUK_SOCKSTATE_CREATED) {
		return EDUK_SOCKERR_CLOSED;
	}
	if (sock->state == EDUK_SOCKSTATE_CONNECTING) {
		return duk_sock_connecting(sock);
	}
#if WIN32
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
					duk_sock_close(sock);
					return EDUK_SOCKERR_FAIL;
				}
				prev = err;
				continue;
			}
			return EDUK_SOCKERR_AGAIN;
		}
		duk_sock_close(sock);
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
			duk_sock_close(sock);
			return EDUK_SOCKERR_RESET;
		}
		return EDUK_SOCKERR_AGAIN;
	}
	duk_sock_close(sock);
	return EDUK_SOCKERR_RESET;
#endif
}

DUK_SOCK_ERROR duk_sock_send(struct duk_sock_t *sock, const char *buf, int buf_size, int *sent_size) {
	*sent_size = 0;
	if (sock->state == EDUK_SOCKSTATE_CLOSED || sock->state == EDUK_SOCKSTATE_CREATED) {
		return EDUK_SOCKERR_CLOSED;
	}
	if (sock->state == EDUK_SOCKSTATE_CONNECTING) {
		return duk_sock_connecting(sock);
	}
	int res = send(sock->fd, buf, buf_size, 0);
	if (res >= 0) {
		*sent_size = res;
		return EDUK_SOCKERR_AGAIN;
	}
#if WIN32
	int err = WSAGetLastError();
	if (err != WSAEWOULDBLOCK) { //TODO: ! (errno == EINTR || errno == EWOULDBLOCK || errno == EAGAIN)
		duk_sock_close(sock);
		return EDUK_SOCKERR_RESET;
	}
	return EDUK_SOCKERR_AGAIN;
#else
	int err = errno;
	if (err == EPIPE || (err != EPROTOTYPE && err != EINTR && err != EAGAIN)) {
		duk_sock_close(sock);
		return EDUK_SOCKERR_RESET;
	}
	return EDUK_SOCKERR_AGAIN;
#endif
}

static duk_ret_t native_print(duk_context *ctx) {
	duk_push_string(ctx, " ");
	duk_insert(ctx, 0);
	duk_join(ctx, duk_get_top(ctx) - 1);
	printf("%s\n", duk_safe_to_string(ctx, -1));
	return 0;
}

duk_ret_t duk_sock_constructor(duk_context* ctx) {
	duk_idx_t top = duk_get_top(ctx);
	duk_int_t type = duk_require_int(ctx, 0);
	duk_int_t family = duk_require_int(ctx, 1);

	duk_push_this(ctx);
	struct duk_sock_t *sock = duk_sock_create(type, family);
	duk_push_pointer(ctx, sock);
	duk_put_prop_literal(ctx, -2, DUK_HIDDEN_SYMBOL("_sock"));
	duk_pop(ctx); // pop this
	return 0;
}

//duk_ret_t 

void test_socket() {
	WSADATA wsaData;
	WORD wVersionRequested = MAKEWORD(2, 2);
	int err = WSAStartup(wVersionRequested, &wsaData);
	if (err != 0) {
		// failed
		return ;
	}
	// family: AF_INET, AF_INET6
	// type: SOCK_STREAM, SOCK_DGRAM
	// protocol: IPPROTO_TCP, IPPROTO_UDP
	struct duk_sock_t *sock = duk_sock_create(EDUK_SOCKTYPE_UDP, EDUK_SOCKFAMILY_IPV4);
	if (sock) {
		duk_sock_setnonblocking(sock);
		int retval = duk_sock_connect_host(sock, "localhost", 1234);
		if (retval >= 0) {
			char send_buf[] = "echo test";
			char recv_buf[1024];
			int sent_size = 0;
			int recv_size = 0;
			do {
				Sleep(1000);
				retval = duk_sock_send(sock, send_buf, sizeof(send_buf), &sent_size);
				if (retval < 0) {
					break;
				}
				retval = duk_sock_recv(sock, recv_buf, sizeof(recv_buf), &recv_size);
				if (retval < 0) {
					break;
				}
				if (recv_size > 0) {
					recv_buf[recv_size] = '\0';
					printf("%s\n", recv_buf);
				}
				else {
					printf("again\n");
				}
			} while (retval >= 0);
			duk_sock_close(sock);
			printf("close\n");
		}
	}
	WSACleanup();
}

int main(int argc, char *argv[]) {
	test_socket();

//	duk_context *ctx = duk_create_heap_default();
//
//	(void) argc; (void) argv;  /* suppress warning */
//
//	duk_unity_open(ctx);
//
//	duk_push_c_function(ctx, native_print, DUK_VARARGS);
//	duk_put_global_string(ctx, "print");
//
//	//duk_push_global_object(ctx);
//	//duk_push_object(ctx);
//	//duk_push_c_function(ctx, )
//
//	// duk_example_attach_debugger(ctx);
//
//	FILE *fp = fopen("scripts/main.js", "r");
//	if (fp) {
//		fseek(fp, 0, SEEK_END);
//		long length = ftell(fp);
//		fseek(fp, 0, SEEK_SET);
//		char *buf = malloc(length + 1);
//		memset(buf, 0, length + 1);
//		fread(buf, length, 1, fp);
//		fclose(fp);
//		//printf("source(%d): %s\n", length, buf);
//		duk_push_string(ctx, buf);
//
//#if defined(RUN_AS_MODULE)
//		duk_module_node_peval_main(ctx, "scripts/main.js");
//#else
//		duk_push_string(ctx, "scripts/main.js");
//		duk_compile(ctx, 0);
//		if (duk_pcall(ctx, 0) != 0) {
//			duk_get_prop_string(ctx, -1, "stack");
//			const char *err = duk_safe_to_string(ctx, -1);
//			printf("peval error: %s\n", err);
//			//printf("source: %s\n", buf);
//		}
//#endif
//		free(buf);
//		duk_pop(ctx);  // pop eval result 
//	} else {
//		printf("can not read file\n");
//	}
//
//	duk_destroy_heap(ctx);
//	fflush(stdout);
//	system("pause");
	return 0;
}
