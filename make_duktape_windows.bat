
mkdir build
pushd build

rd /s /q x64
rd /s /q x86

mkdir x64
mkdir x86

pushd x64
cmake -G "Visual Studio 15 2017 Win64" ..\..
REM msbuild duktape.sln /p:Configuration=Release
popd
cmake --build x64 --config Release
xcopy /Y .\x64\Release\duktape.dll ..\unity\Assets\Duktape\Plugins\x64

pushd x86
cmake -G "Visual Studio 15 2017" ..\..
REM msbuild duktape.sln /p:Configuration=Release
popd
cmake --build x86 --config Release
xcopy /Y .\x86\Release\duktape.dll ..\unity\Assets\Duktape\Plugins\x86

popd
