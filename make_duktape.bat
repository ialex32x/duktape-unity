
mkdir build
pushd build

rd /s /q x64
rd /s /q x86

mkdir x64
mkdir x86

pushd x64
cmake -G "Visual Studio 15 2017 Win64" ..\..
msbuild duktape.sln /p:Configuration=Release
xcopy /Y Release/duktape.dll ..\..\unity\Assets\Plugins\x64
popd

pushd x86
cmake -G "Visual Studio 15 2017" ..\..
msbuild duktape.sln /p:Configuration=Release
xcopy /Y Release/duktape.dll ..\..\unity\Assets\Plugins\x86
popd

popd
