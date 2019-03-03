
mkdir build
pushd build

mkdir librws
pushd librws
rd /s /q x64

mkdir x64

pushd x64
cmake -G "Visual Studio 15 2017 Win64" ..\..\..\librws
popd
cmake --build x64 --config Release
xcopy /Y .\x64\Release\librws_static.lib .\
popd 

mkdir duktape
pushd duktape

rd /s /q x64

mkdir x64

pushd x64
cmake -G "Visual Studio 15 2017 Win64" ..\..\..
popd
cmake --build x64 --config Release
xcopy /Y .\x64\Release\duktape.dll ..\..\unity\Assets\Duktape\Plugins\x64

popd

popd
