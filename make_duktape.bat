
mkdir build
pushd build
mkdir x64
mkdir x86

pushd x64
cmake -G "Visual Studio 15 2017 Win64" ..\..
msbuild duktape.sln /p:Configuration=Release
popd

pushd x86
cmake -G "Visual Studio 15 2017" ..\..
msbuild duktape.sln /p:Configuration=Release
popd

popd