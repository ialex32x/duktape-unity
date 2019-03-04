pushd duktape-2.3.0
python .\tools\configure.py --source-directory src-input --output-directory src-scratch --config-metadata config -DDUK_USE_FATAL_HANDLER
popd

mkdir build
pushd build

REM mkdir librws
REM pushd librws
REM rd /s /q x64
REM mkdir x64
REM pushd x64
REM cmake -G "Visual Studio 15 2017 Win64" ..\..\..\librws
REM popd
REM cmake --build x64 --config Release
REM xcopy /Y .\x64\Release\librws_static.lib .\
REM popd 

REM mkdir duktape
REM pushd duktape
REM rd /s /q x64
REM mkdir x64
REM pushd x64
REM cmake -G "Visual Studio 15 2017 Win64" ..\..\..
REM popd
REM cmake --build x64 --config Release
REM popd

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
