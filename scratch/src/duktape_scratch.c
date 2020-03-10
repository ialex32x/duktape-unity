/*
 *  Very simple example program
 */

#include "duktape.h"
#include <stdio.h>
#include <stdlib.h>

#define WIN32_LEAN_AND_MEAN
#include <winsock2.h>
#include <ws2tcpip.h>
#include <Windows.h>
 // https://docs.microsoft.com/en-us/windows/desktop/WinSock/windows-sockets-error-codes-2

#define RUN_AS_MODULE
#define duk_memcmp memcmp
#define duk_memcpy memcpy

enum duk_sock_state {
	DSOCK_CLOSED = 0,
	DSOCK_CONNECTING = 1,
	DSOCK_CONNECTED = 2,
};

struct duk_sock_t {
#if WIN32
	SOCKET fd;
#else
	int fd;
#endif
	enum duk_sock_state state;
};

void duk_sock_setnonblocking(struct duk_sock_t *sock) {
#if WIN32
	u_long argp = 1;
	ioctlsocket(sock->fd, FIONBIO, &argp);
#else 
	int flags = fcntl(sock->fd, F_GETFL, 0);
	flags |= O_NONBLOCK;
	fcntl(sock->fd, F_SETFL, flags);
#endif
}

struct duk_sock_t *duk_sock_create(int af, int type, int protocol) {
#if WIN32
	SOCKET fd = socket(af, type, protocol);
	if (fd == INVALID_SOCKET) {
		return NULL;
	}
#else 
	int fd = socket(af, type, protocol);
	if (fd < 0) {
		return NULL;
	}
#endif
	struct duk_sock_t *sock = (struct duk_sock_t *)malloc(sizeof(struct duk_sock_t));
	memset(sock, 0, sizeof(struct duk_sock_t));
	sock->fd = fd;
	return sock;
}

int duk_sock_connect(struct duk_sock_t *sock, struct sockaddr *addr, int port) {
	int res = -1;
	switch (addr->sa_family) {
	case AF_INET: {
		struct sockaddr_in sa;
		memcpy(&sa, addr, sizeof(struct sockaddr_in));
		sa.sin_port = htons(port);
		sock->state = DSOCK_CONNECTING;
		res = connect(sock->fd, (const struct sockaddr *)&sa, sizeof(struct sockaddr_in));
		break;
	}
	case AF_INET6: {
		struct sockaddr_in6 sa;
		memcpy(&sa, addr, sizeof(struct sockaddr_in6));
		sa.sin6_port = htons(port);
		sock->state = DSOCK_CONNECTING;
		res = connect(sock->fd, (const struct sockaddr *)&sa, sizeof(struct sockaddr_in6));
		break;
	}
	default: return -1;
	}
	if (res < 0) {
		//TODO: errno == EINPROGRESS 
		int err = WSAGetLastError();
		if (err != WSAEWOULDBLOCK && err != WSAEINPROGRESS) {
			printf("connect failed %d (%d)\n", res, WSAGetLastError());
			return -1;
		}
		res = 0;
	}
	// int optval;
	// socklen_t optlen = sizeof(optval);
	// res = getsockopt(sock->fd, SOL_SOCKET, SO_ERROR, (char *)&optval, &optlen);
	// if (res < 0) {
	//     printf("getsockopt failed %d (%d)\n", res, WSAGetLastError());
	//     return -1;
	// }
	return res;
}

int duk_sock_close(struct duk_sock_t *sock) {
	closesocket(sock->fd);
	return 0;
}

int duk_sock_poll(struct duk_sock_t *sock, char *buf, int buf_size) {
	if (sock->state == DSOCK_CLOSED) {
		return -1;
	}
	if (sock->state == DSOCK_CONNECTING) {
		//TODO: select, check connect state
		//return 
	}
	else {
		//TODO: recv, if connected
	}
	int res = recv(sock->fd, buf, buf_size, 0);
	if (res < 0) {
		int err = WSAGetLastError();
		if (err != WSAEWOULDBLOCK) { //TODO: ! (errno == EINTR || errno == EWOULDBLOCK || errno == EAGAIN)
			sock->state = DSOCK_CLOSED;
			printf("recv failed %d (%d)\n", res, WSAGetLastError());
			return -1;
		}
		return 0;
	}
	if (res == 0) {
		sock->state = DSOCK_CLOSED;
		return -1;
	}
	sock->state = DSOCK_CONNECTED;
	return res;
}

int duk_sock_send(struct duk_sock_t *sock, const char *buf, int buf_size) {
	if (sock->state == DSOCK_CLOSED) {
		return -1;
	}
	if (sock->state == DSOCK_CONNECTING) {
		//TODO: return skip;
	}
	int res = send(sock->fd, buf, buf_size, 0);
	if (res < 0) {
		int err = WSAGetLastError();
		if (err != WSAEWOULDBLOCK) { //TODO: ! (errno == EINTR || errno == EWOULDBLOCK || errno == EAGAIN)
			sock->state = DSOCK_CLOSED;
			printf("send failed %d (%d)\n", res, WSAGetLastError());
			return -1;
		}
		return 0;
	}
	if (res == 0) {
		sock->state = DSOCK_CLOSED;
		return -1;
	}
	return res;
}

static duk_ret_t native_print(duk_context *ctx) {
	duk_push_string(ctx, " ");
	duk_insert(ctx, 0);
	duk_join(ctx, duk_get_top(ctx) - 1);
	printf("%s\n", duk_safe_to_string(ctx, -1));
	return 0;
}

int init() {
#if WIN32
	WSADATA wsaData;
	WORD wVersionRequested = MAKEWORD(2, 0);
	int err = WSAStartup(wVersionRequested, &wsaData);
	if (err != 0) return 0;
	if ((LOBYTE(wsaData.wVersion) != 2 || HIBYTE(wsaData.wVersion) != 0) &&
		(LOBYTE(wsaData.wVersion) != 1 || HIBYTE(wsaData.wVersion) != 1)) {
		WSACleanup();
		return 0;
	}
#endif
	return 1;
}

void deinit() {
#if WIN32
	WSACleanup();
#endif
}

int main(int argc, char *argv[]) {
	init();
	duk_context *ctx = duk_create_heap_default();

	(void) argc; (void) argv;  /* suppress warning */

	duk_unity_open(ctx);

	duk_push_c_function(ctx, native_print, DUK_VARARGS);
	duk_put_global_string(ctx, "print");

	// duk_example_attach_debugger(ctx);

	FILE *fp = fopen("scripts/main.js", "r");
	if (fp) {
		fseek(fp, 0, SEEK_END);
		long length = ftell(fp);
		fseek(fp, 0, SEEK_SET);
		char *buf = malloc(length + 1);
		memset(buf, 0, length + 1);
		fread(buf, length, 1, fp);
		fclose(fp);
		//printf("source(%d): %s\n", length, buf);
		duk_push_string(ctx, buf);

#if defined(RUN_AS_MODULE)
		duk_module_node_peval_main(ctx, "scripts/main.js");
#else
		duk_push_string(ctx, "scripts/main.js");
		duk_compile(ctx, 0);
		if (duk_pcall(ctx, 0) != 0) {
			duk_get_prop_string(ctx, -1, "stack");
			const char *err = duk_safe_to_string(ctx, -1);
			printf("peval error: %s\n", err);
			//printf("source: %s\n", buf);
		}
#endif
		free(buf);
		duk_pop(ctx);  // pop eval result 
	} else {
		printf("can not read file\n");
	}

	duk_destroy_heap(ctx);
	fflush(stdout);
	deinit();
	system("pause");
	return 0;
}
