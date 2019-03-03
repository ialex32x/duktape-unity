
mkdir build
pushd build

REM mkdir librws
REM pushd librws
REM rd /s /q x64
REM mkdir x64
REM pushd x64
REM cmake -G "Visual Studio 15 2017 Win64" ..\..\..\librws
REM popd
REM cmake --build x64 --config Release
REM xcopy /Y .\x64\Release\librws_static.lib .\
REM popd 

mkdir duktape
pushd duktape
rd /s /q x64
mkdir x64
pushd x64
cmake -G "Visual Studio 15 2017 Win64" ..\..\..
popd
cmake --build x64 --config Release
xcopy /Y .\x64\Release\duktape.dll ..\..\unity\Assets\Duktape\Plugins\x64\
popd

popd
