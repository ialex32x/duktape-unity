
mkdir build
pushd build
mkdir x64
mkdir x86

pushd x64
cmake -G "Visual Studio 15 2017 Win64" ..\..
popd

pushd x86
cmake -G "Visual Studio 15 2017" ..\..
popd

popd