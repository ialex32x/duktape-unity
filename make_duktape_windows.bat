@echo off

SET BASE_PATH=%~dp0
SET BUILD_PATH=%~dp0build\duktape
echo %BUILD_PATH%
REM cmake -G "Visual Studio 15 2017 Win64" ..\..\..

mkdir %BUILD_PATH% 2>nul
cd %BUILD_PATH%

rd /s /q x64_debug 2>nul
echo building x64_debug
mkdir x64_debug
cd x64_debug
cmake -DDUKTAPE_SRC_CAT=debug -G "Visual Studio 16 2019" -A x64 ..\..\..
cd %BUILD_PATH%
cmake --build x64_debug --config Debug
mkdir ..\..\prebuilt\debug\Plugins\x64\ 2>nul
xcopy /Y .\x64_debug\Debug\duktape.dll ..\..\prebuilt\debug\Plugins\x64\

rd /s /q x64_release 2>nul
echo building x64_release
mkdir x64_release
cd x64_release
cmake -DDUKTAPE_SRC_CAT=release -G "Visual Studio 16 2019" -A x64 ..\..\..
cd %BUILD_PATH%
cmake --build x64_release --config Release
mkdir ..\..\prebuilt\release\Plugins\x64\ 2>nul
xcopy /Y .\x64_release\Release\duktape.dll ..\..\prebuilt\release\Plugins\x64\

rd /s /q x86_debug 2>nul
echo building x86_debug
mkdir x86_debug
cd x86_debug
cmake -DDUKTAPE_SRC_CAT=debug -G "Visual Studio 16 2019" -A Win32 ..\..\..
cd %BUILD_PATH%
cmake --build x86_debug --config Debug
mkdir ..\..\prebuilt\debug\Plugins\x86\ 2>nul
xcopy /Y .\x86_debug\Debug\duktape.dll ..\..\prebuilt\debug\Plugins\x86\

rd /s /q x86_release 2>nul
echo building x86_release
mkdir x86_release
cd x86_release
cmake -DDUKTAPE_SRC_CAT=release -G "Visual Studio 16 2019" -A Win32 ..\..\..
cd %BUILD_PATH%
cmake --build x86_release --config Release
mkdir ..\..\prebuilt\release\Plugins\x86\ 2>nul
xcopy /Y .\x86_release\Release\duktape.dll ..\..\prebuilt\release\Plugins\x86\

cd %BASE_PATH%
