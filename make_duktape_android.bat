@echo off

SET ANDROID_NDK=E:/android-ndk-r15c
SET BASE_PATH=%~dp0
SET BUILD_PATH=%~dp0build\duktape
echo %BUILD_PATH%

mkdir %BUILD_PATH% 2>nul
cd %BUILD_PATH%

rd /s /q %BUILD_PATH%\Android_v7a_debug 2>nul
echo building v7a debug
mkdir %BUILD_PATH%\Android_v7a_debug
cd %BUILD_PATH%\Android_v7a_debug
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=debug -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../../..
cd %BUILD_PATH%
cmake --build Android_v7a_debug --config Release
mkdir ..\..\prebuilt\debug\Plugins\Android\libs\armeabi-v7a\ 2>nul
xcopy /Y .\Android_v7a_debug\libduktape.so ..\..\prebuilt\debug\Plugins\Android\libs\armeabi-v7a\

rd /s /q %BUILD_PATH%\Android_v7a_release 2>nul
echo building v7a release
mkdir %BUILD_PATH%\Android_v7a_release
cd %BUILD_PATH%\Android_v7a_release
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=release -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../../..
cd %BUILD_PATH%
cmake --build Android_v7a_release --config Release
mkdir ..\..\prebuilt\release\Plugins\Android\libs\armeabi-v7a\ 2>nul
xcopy /Y .\Android_v7a_release\libduktape.so ..\..\prebuilt\release\Plugins\Android\libs\armeabi-v7a\

rd /s /q %BUILD_PATH%\Android_v8a_debug 2>nul
echo building v8a debug
mkdir %BUILD_PATH%\Android_v8a_debug
cd %BUILD_PATH%\Android_v8a_debug
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=debug -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../../..
cd %BUILD_PATH%
cmake --build Android_v8a_debug --config Release
mkdir ..\..\prebuilt\debug\Plugins\Android\libs\arm64-v8a\ 2>nul
xcopy /Y .\Android_v8a_debug\libduktape.so ..\..\prebuilt\debug\Plugins\Android\libs\arm64-v8a\

rd /s /q %BUILD_PATH%\Android_v8a_release 2>nul
echo building v8a release
mkdir %BUILD_PATH%\Android_v8a_release
cd %BUILD_PATH%\Android_v8a_release
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=release -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../../..
cd %BUILD_PATH%
cmake --build Android_v8a_release --config Release
mkdir ..\..\prebuilt\release\Plugins\Android\libs\arm64-v8a\ 2>nul
xcopy /Y .\Android_v8a_release\libduktape.so ..\..\prebuilt\release\Plugins\Android\libs\arm64-v8a\

rd /s /q %BUILD_PATH%\Android_x86_debug 2>nul
echo building x86 debug
mkdir %BUILD_PATH%\Android_x86_debug
cd %BUILD_PATH%\Android_x86_debug
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=debug -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=x86-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../../..
cd %BUILD_PATH%
cmake --build Android_x86_debug --config Release
mkdir ..\..\prebuilt\debug\Plugins\Android\libs\x86\ 2>nul
xcopy /Y .\Android_x86_debug\libduktape.so ..\..\prebuilt\debug\Plugins\Android\libs\x86\

rd /s /q %BUILD_PATH%\Android_x86_release 2>nul
echo building x86 release
mkdir %BUILD_PATH%\Android_x86_release
cd %BUILD_PATH%\Android_x86_release
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=release -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=x86-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../../..
cd %BUILD_PATH%
cmake --build Android_x86_release --config Release
mkdir ..\..\prebuilt\release\Plugins\Android\libs\x86\ 2>nul
xcopy /Y .\Android_x86_release\libduktape.so ..\..\prebuilt\release\Plugins\Android\libs\x86\

cd %BASE_PATH%
