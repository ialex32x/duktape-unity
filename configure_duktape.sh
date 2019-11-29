#!/usr/bin/env sh

cd duktape-2.4.0
python ./tools/configure.py --source-directory src-input --output-directory src-custom --config-metadata config --dll -DDUK_USE_FATAL_HANDLER -DDUK_USE_FUNC_FILENAME_PROPERTY
