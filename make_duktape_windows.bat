@echo off

REM cmake -G "Visual Studio 15 2017 Win64" ..\..\..

mkdir build
pushd build
mkdir duktape
pushd duktape

REM x64 -------------
    rd /s /q x64_debug
    mkdir x64_debug
    pushd x64_debug
    cmake -DDUKTAPE_SRC_CAT=debug -G "Visual Studio 16 2019" -A x64 ..\..\..
    popd
    cmake --build x64_debug --config Release
    xcopy /Y .\x64_debug\Release\duktape.dll ..\..\prebuilt\debug\Plugins\x64\

    rd /s /q x64_release
    mkdir x64_release
    pushd x64_release
    cmake -DDUKTAPE_SRC_CAT=release -G "Visual Studio 16 2019" -A x64 ..\..\..
    popd
    cmake --build x64_release --config Release
    xcopy /Y .\x64_release\Release\duktape.dll ..\..\prebuilt\release\Plugins\x64\
REM x64 -------------

REM x86 -------------
    rd /s /q x86_debug
    mkdir x86_debug
    pushd x86_debug
    cmake -DDUKTAPE_SRC_CAT=debug -G "Visual Studio 16 2019" -A Win32 ..\..\..
    popd
    cmake --build x86_debug --config Release
    xcopy /Y .\x86_debug\Release\duktape.dll ..\..\prebuilt\debug\Plugins\x86\

    rd /s /q x86_release
    mkdir x86_release
    pushd x86_release
    cmake -DDUKTAPE_SRC_CAT=release -G "Visual Studio 16 2019" -A Win32 ..\..\..
    popd
    cmake --build x86_release --config Release
    xcopy /Y .\x86_release\Release\duktape.dll ..\..\prebuilt\release\Plugins\x86\
REM x86 -------------

popd
popd
