cmake_minimum_required(VERSION 3.7.2)
project(duktape_scratch)

set(TOP_DIR ${CMAKE_CURRENT_SOURCE_DIR}/..)
#set(LIBRWS_DIR ${TOP_DIR}/librws)
set(DUKTAPE_SRC_DIR ${TOP_DIR}/duktape-2.3.0/src-scratch)
set(DUKTAPE_SCRATCH_SRC_DIR ${TOP_DIR}/scratch/src)

set(LIBWEBSOCKETS_DIR ${TOP_DIR}/thirdparty/libwebsockets)
set(MBEDTLS_DIR ${TOP_DIR}/thirdparty/mbedtls)

include_directories(${DUKTAPE_SRC_DIR})
aux_source_directory(${DUKTAPE_SRC_DIR} DUKTAPE_SRCS)

aux_source_directory(${DUKTAPE_SCRATCH_SRC_DIR} DUKTAPE_SCRATCH_SRCS)

include_directories(${MBEDTLS_DIR}/include)
aux_source_directory(${MBEDTLS_DIR}/library MBEDTLS_SRCS)

message("Source: ${DUKTAPE_SCRATCH_SRC_DIR}")
add_executable(duktape_scratch ${DUKTAPE_SCRATCH_SRCS} ${DUKTAPE_SRCS} ${MBEDTLS_SRCS})

IF(CMAKE_CL_64)
    #link_directories(${TOP_DIR}/build/librws/x64/Release)
    #link_directories(${TOP_DIR}/build/duktape/x64/Release)
    #find_library(DUKTAPE_LIBRARY NAMES duktape.lib PATH ${TOP_DIR}/build/duktape/x64/Release)
    #target_link_libraries(duktape_scratch ${TOP_DIR}/build/duktape/x64/Release/duktape.lib)
ELSE()
    #link_directories(${TOP_DIR}/build/librws/x86/Release)
    #link_directories(${TOP_DIR}/build/duktape/x86/Release)
    #find_library(DUKTAPE_LIBRARY NAMES duktape.lib PATH ${TOP_DIR}/build/duktape/x86/Release)
    #target_link_libraries(duktape_scratch ${TOP_DIR}/build/duktape/x86/Release/duktape.lib)
ENDIF()

#target_link_libraries(duktape_scratch rws_static)
#target_link_libraries(duktape_scratch ${DUKTAPE_LIBRARY})
target_link_libraries(duktape_scratch bcrypt)

