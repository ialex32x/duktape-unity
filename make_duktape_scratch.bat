
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
