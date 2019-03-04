
mkdir build
pushd build

REM mkdir librws
REM pushd librws
REM rd /s /q x86
REM mkdir x86
REM pushd x86
REM cmake -G "Visual Studio 15 2017" ..\..\..\librws
REM popd
REM cmake --build x86 --config Release
REM xcopy /Y .\x86\Release\librws_static.lib .\
REM popd 

mkdir duktape
pushd duktape
rd /s /q x86
mkdir x86
pushd x86
cmake -G "Visual Studio 15 2017" ..\..\..
popd
cmake --build x86 --config Release
xcopy /Y .\x86\Release\duktape.dll ..\..\unity\Assets\Duktape\Plugins\x86\
popd

popd
