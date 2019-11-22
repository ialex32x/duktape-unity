
mkdir build
pushd build

mkdir duktape
pushd duktape
rd /s /q x86
mkdir x86
pushd x86
REM cmake -G "Visual Studio 15 2017" ..\..\..
cmake -G "Visual Studio 16 2019" -A Win32 ..\..\..
popd
cmake --build x86 --config Release
xcopy /Y .\x86\Release\duktape.dll ..\..\unity\Assets\Duktape\Plugins\x86\
popd

popd
