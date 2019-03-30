//
#include "duk_internal.h"

#include <winsock2.h>
#include <ws2tcpip.h>

DUK_INTERNAL duk_bool_t duk_sock_open(duk_context *ctx) {
    WSADATA wsaData;
    WORD wVersionRequested = MAKEWORD(2, 0); 
    int err = WSAStartup(wVersionRequested, &wsaData);
    if (err != 0) {
        return 0;
    }
    if ((LOBYTE(wsaData.wVersion) != 2 || HIBYTE(wsaData.wVersion) != 0) &&
        (LOBYTE(wsaData.wVersion) != 1 || HIBYTE(wsaData.wVersion) != 1)) {
        WSACleanup();
        return 0; 
    }
    return 1;
}
