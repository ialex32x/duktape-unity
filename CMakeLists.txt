cmake_minimum_required(VERSION 2.6)
project(duktape)

set(TOP_DIR ${CMAKE_CURRENT_SOURCE_DIR}/)
set(DUKTAPE_DIR ${TOP_DIR}/duktape-2.3.0)
set(DUKTAPE_SRC_DIR ${DUKTAPE_DIR}/src-custom)

include_directories(${DUKTAPE_SRC_DIR})
aux_source_directory(${DUKTAPE_SRC_DIR} DUKTAPE_SRCS)

add_library(duktape SHARED ${DUKTAPE_SRCS})
