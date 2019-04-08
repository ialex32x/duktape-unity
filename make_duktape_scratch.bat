REM pushd duktape-2.3.0
REM python .\tools\configure.py --source-directory src-input --output-directory src-scratch --config-metadata config -DDUK_USE_FATAL_HANDLER
REM popd

mkdir build
pushd build

mkdir duktape_scratch
pushd duktape_scratch 
rd /s /q x64
mkdir x64
pushd x64
cmake -G "Visual Studio 15 2017 Win64" ..\..\..\scratch
popd
cmake --build x64 --config Release
popd

popd
