
rd /s /q build\src-scratch
mkdir build

pushd duktape-2.5.0
python .\tools\configure.py --source-directory src-input --output-directory ..\build\src-scratch --config-metadata config -DDUK_USE_FATAL_HANDLER -DDUK_USE_FUNC_FILENAME_PROPERTY -DDUK_USE_DEBUGGER_SUPPORT -DDUK_USE_INTERRUPT_COUNTER -DDUK_USE_DEBUGGER_INSPECT -DDUK_USE_DEBUGGER_FWD_LOGGING -DDUK_USE_DEBUGGER_FWD_PRINTALERT -DDUK_USE_DEBUGGER_THROW_NOTIFY -DDUK_USE_DEBUGGER_PAUSE_UNCAUGHT -DDUK_USE_DEBUGGER_DUMPHEAP
popd
