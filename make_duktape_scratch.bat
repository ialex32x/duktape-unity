@echo off

mkdir build
pushd build

mkdir duktape_scratch
pushd duktape_scratch 
rd /s /q x64
mkdir x64
pushd x64
REM cmake -G "Visual Studio 15 2017 Win64" ..\..\..\scratch
cmake -G "Visual Studio 16 2019" -A x64 ..\..\..\scratch
popd
REM cmake --build x64 --config Release
popd

popd
