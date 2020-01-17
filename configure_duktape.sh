#!/usr/bin/env sh

rm -rf build/src-debug
rm -rf build/src-release

cd duktape-2.5.0
python2 ./tools/configure.py --source-directory src-input --output-directory ../build/src-debug --config-metadata config --dll -DDUK_USE_FATAL_HANDLER -DDUK_USE_FUNC_FILENAME_PROPERTY -DDUK_USE_DEBUGGER_SUPPORT -DDUK_USE_INTERRUPT_COUNTER -DDUK_USE_DEBUGGER_INSPECT -DDUK_USE_DEBUGGER_FWD_LOGGING -DDUK_USE_DEBUGGER_FWD_PRINTALERT -DDUK_USE_DEBUGGER_THROW_NOTIFY -DDUK_USE_DEBUGGER_PAUSE_UNCAUGHT -DDUK_USE_DEBUGGER_DUMPHEAP
python2 ./tools/configure.py --source-directory src-input --output-directory ../build/src-release --config-metadata config --dll -DDUK_USE_FATAL_HANDLER
