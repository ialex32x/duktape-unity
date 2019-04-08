cmake_minimum_required(VERSION 3.7.2)
project(duktape_scratch)

set(TOP_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
set(DUKTAPE_SRC_DIR ${TOP_DIR}/duktape-2.3.0/src-scratch)
set(DUKTAPE_SCRATCH_SRC_DIR ${TOP_DIR}/scratch/src)

set(LIBWEBSOCKETS_DIR ${TOP_DIR}/thirdparty/libwebsockets)
set(MBEDTLS_DIR ${TOP_DIR}/thirdparty/mbedtls)

include_directories(${DUKTAPE_SRC_DIR})
aux_source_directory(${DUKTAPE_SRC_DIR} DUKTAPE_SRCS)

aux_source_directory(${DUKTAPE_SCRATCH_SRC_DIR} DUKTAPE_SCRATCH_SRCS)

include_directories(${MBEDTLS_DIR}/include)
aux_source_directory(${MBEDTLS_DIR}/library MBEDTLS_SRCS)

include_directories(${LIBWEBSOCKETS_DIR})
include_directories(${LIBWEBSOCKETS_DIR}/win32helpers)
include_directories(${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/include)
include_directories(${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/include/internal)
include_directories(${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/include/openssl)
include_directories(${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/include/platform)

set(LIBWEBSOCKETS_SRCS
    ${LIBWEBSOCKETS_DIR}/core/alloc.c
    ${LIBWEBSOCKETS_DIR}/core/context.c
    ${LIBWEBSOCKETS_DIR}/core/libwebsockets.c
    ${LIBWEBSOCKETS_DIR}/core/output.c
    ${LIBWEBSOCKETS_DIR}/core/pollfd.c
    ${LIBWEBSOCKETS_DIR}/core/service.c

    ${LIBWEBSOCKETS_DIR}/event-libs/poll/poll.c

    ${LIBWEBSOCKETS_DIR}/misc/base64-decode.c
    ${LIBWEBSOCKETS_DIR}/misc/lejp.c
    ${LIBWEBSOCKETS_DIR}/misc/sha-1.c

    ${LIBWEBSOCKETS_DIR}/roles/h1/ops-h1.c
    ${LIBWEBSOCKETS_DIR}/roles/http/header.c
    ${LIBWEBSOCKETS_DIR}/roles/http/client/client.c
    ${LIBWEBSOCKETS_DIR}/roles/http/client/client-handshake.c
    ${LIBWEBSOCKETS_DIR}/roles/http/server/fops-zip.c
    ${LIBWEBSOCKETS_DIR}/roles/http/server/lejp-conf.c
    ${LIBWEBSOCKETS_DIR}/roles/http/server/parsers.c
    ${LIBWEBSOCKETS_DIR}/roles/http/server/server.c
    ${LIBWEBSOCKETS_DIR}/roles/listen/ops-listen.c
    ${LIBWEBSOCKETS_DIR}/roles/pipe/ops-pipe.c
    ${LIBWEBSOCKETS_DIR}/roles/raw/ops-raw.c

    ${LIBWEBSOCKETS_DIR}/roles/ws/client-ws.c
    ${LIBWEBSOCKETS_DIR}/roles/ws/client-parser-ws.c
    ${LIBWEBSOCKETS_DIR}/roles/ws/ops-ws.c
    ${LIBWEBSOCKETS_DIR}/roles/ws/server-ws.c

    ${LIBWEBSOCKETS_DIR}/tls/tls.c
    ${LIBWEBSOCKETS_DIR}/tls/tls-client.c
    ${LIBWEBSOCKETS_DIR}/tls/tls-server.c

    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_cert.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_pkey.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_stack.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_methods.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_lib.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/library/ssl_x509.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/platform/ssl_port.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/wrapper/platform/ssl_pm.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/lws-genhash.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/mbedtls-client.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/lws-genrsa.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/ssl.c
    ${LIBWEBSOCKETS_DIR}/tls/mbedtls/mbedtls-server.c
)

IF (WIN32)
    MESSAGE("WIN32")
    list(APPEND LIBWEBSOCKETS_SRCS
        ${LIBWEBSOCKETS_DIR}/plat/lws-plat-win.c
        ${LIBWEBSOCKETS_DIR}/win32helpers/getopt.c
        ${LIBWEBSOCKETS_DIR}/win32helpers/getopt_long.c
        ${LIBWEBSOCKETS_DIR}/win32helpers/gettimeofday.c
    )
ELSE ()
    MESSAGE("UNIX")
    list(APPEND LIBWEBSOCKETS_SRCS
        ${LIBWEBSOCKETS_DIR}/plat/lws-plat-unix.c
    )
ENDIF ()

IF (ANDROID)
    MESSAGE("ANDROID")
    list(APPEND LIBWEBSOCKETS_SRCS
        ${LIBWEBSOCKETS_DIR}/misc/getifaddrs.c
    )
ENDIF ()

message("Source: ${DUKTAPE_SCRATCH_SRC_DIR}")
add_executable(duktape_scratch ${DUKTAPE_SCRATCH_SRCS} ${DUKTAPE_SRCS} ${MBEDTLS_SRCS} ${LIBWEBSOCKETS_SRCS})

IF(CMAKE_CL_64)
    #link_directories(${TOP_DIR}/build/duktape/x64/Release)
    #find_library(DUKTAPE_LIBRARY NAMES duktape.lib PATH ${TOP_DIR}/build/duktape/x64/Release)
    #target_link_libraries(duktape_scratch ${TOP_DIR}/build/duktape/x64/Release/duktape.lib)
ELSE()
    #link_directories(${TOP_DIR}/build/duktape/x86/Release)
    #find_library(DUKTAPE_LIBRARY NAMES duktape.lib PATH ${TOP_DIR}/build/duktape/x86/Release)
    #target_link_libraries(duktape_scratch ${TOP_DIR}/build/duktape/x86/Release/duktape.lib)
ENDIF()

#target_link_libraries(duktape_scratch rws_static)
#target_link_libraries(duktape_scratch ${DUKTAPE_LIBRARY})


IF (WIN32)
    target_link_libraries(duktape_scratch bcrypt)
ENDIF ()

