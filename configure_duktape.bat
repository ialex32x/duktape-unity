
pushd duktape-2.4.0
REM python .\tools\configure.py --source-directory src-input --output-directory src-custom --config-metadata config --dll -DDUK_USE_FATAL_HANDLER -DDUK_USE_FUNC_FILENAME_PROPERTY
REM debugger support
python .\tools\configure.py --source-directory src-input --output-directory src-custom --config-metadata config --dll -DDUK_USE_FATAL_HANDLER -DDUK_USE_FUNC_FILENAME_PROPERTY -DDUK_USE_DEBUGGER_SUPPORT -DDUK_USE_INTERRUPT_COUNTER -DDUK_USE_DEBUGGER_INSPECT -DDUK_USE_DEBUGGER_FWD_LOGGING -DDUK_USE_DEBUGGER_FWD_PRINTALERT -DDUK_USE_DEBUGGER_THROW_NOTIFY -DDUK_USE_DEBUGGER_PAUSE_UNCAUGHT -DDUK_USE_DEBUGGER_DUMPHEAP
popd
