cmake_minimum_required(VERSION 3.7.2)
project(duktape)

set(TOP_DIR ${CMAKE_CURRENT_SOURCE_DIR})
set(DUKTAPE_DIR ${TOP_DIR}/duktape-2.3.0)
set(DUKTAPE_SRC_DIR ${DUKTAPE_DIR}/src-custom)

set(LIBWEBSOCKETS_DIR ${TOP_DIR}/thirdparty/libwebsockets)
set(MBEDTLS_DIR ${TOP_DIR}/thirdparty/mbedtls)

include_directories(${DUKTAPE_SRC_DIR})
aux_source_directory(${DUKTAPE_SRC_DIR} DUKTAPE_SRCS)

include_directories(${MBEDTLS_DIR}/include)
aux_source_directory(${MBEDTLS_DIR}/library MBEDTLS_SRCS)

#IF(CMAKE_CL_64)
#    link_directories(${TOP_DIR}/build/librws/x64/Release)
#ELSE()
#    link_directories(${TOP_DIR}/build/librws/x86/Release)
#ENDIF()

if (APPLE) 
    if (IOS)
        add_library(duktape STATIC ${DUKTAPE_SRCS} ${MBEDTLS_SRCS})
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD)")
        set_property(TARGET duktape PROPERTY XCODE_ATTRIBUTE_IPHONEOS_DEPLOYMENT_TARGET "7.0")
    else ()
        add_library(duktape MODULE ${DUKTAPE_SRCS} ${MBEDTLS_SRCS})
        set(CMAKE_OSX_ARCHITECTURES "$(ARCHS_STANDARD_64_BIT)")
        set_target_properties(duktape PROPERTIES MACOSX_BUNDLE TRUE)
        set_target_properties(duktape PROPERTIES BUNDLE TRUE)
    endif ()
else ()
    add_library(duktape SHARED ${DUKTAPE_SRCS} ${MBEDTLS_SRCS})
endif ()

target_sources(duktape PRIVATE 
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

#target_link_libraries(duktape librws_static.lib)

#target_link_libraries(duktape_scratch bcrypt)
