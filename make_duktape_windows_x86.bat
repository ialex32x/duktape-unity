
mkdir build
pushd build

mkdir librws
pushd librws
rd /s /q x86

mkdir x86

pushd x86
cmake -G "Visual Studio 15 2017" ..\..\..\librws
popd
cmake --build x86 --config Release
xcopy /Y .\x86\Release\librws_static.lib .\
popd 

mkdir duktape
pushd duktape

rd /s /q x86

mkdir x86

pushd x86
cmake -G "Visual Studio 15 2017" ..\..\..
popd
cmake --build x86 --config Release
xcopy /Y .\x86\Release\duktape.dll ..\..\unity\Assets\Duktape\Plugins\x86

popd

popd
