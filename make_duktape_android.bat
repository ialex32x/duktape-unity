@echo off

REM ...
SET ANDROID_NDK=E:/android-ndk-r15c

mkdir build
pushd build

echo building v7a debug
rd /s /q Android_v7a_debug
mkdir Android_v7a_debug
pushd Android_v7a_debug
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=debug -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../..
popd
cmake --build Android_v7a_debug --config Release
mkdir ..\prebuilt\debug\Plugins\Android\libs\armeabi-v7a\
xcopy /Y .\Android_v7a_debug\libduktape.so ..\prebuilt\debug\Plugins\Android\libs\armeabi-v7a\

echo building v7a release
rd /s /q Android_v7a_release
mkdir Android_v7a_release
pushd Android_v7a_release
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=release -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../..
popd
cmake --build Android_v7a_release --config Release
mkdir ..\prebuilt\release\Plugins\Android\libs\armeabi-v7a\
xcopy /Y .\Android_v7a_release\libduktape.so ..\prebuilt\release\Plugins\Android\libs\armeabi-v7a\

echo building v8a debug
rd /s /q Android_v8a_debug
mkdir Android_v8a_debug
pushd Android_v8a_debug
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=debug -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../..
popd
cmake --build Android_v8a_debug --config Release
mkdir ..\prebuilt\debug\Plugins\Android\libs\arm64-v8a\
xcopy /Y .\Android_v8a_debug\libduktape.so ..\prebuilt\debug\Plugins\Android\libs\arm64-v8a\

echo building v8a release
rd /s /q Android_v8a_release
mkdir Android_v8a_release
pushd Android_v8a_release
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=release -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../..
popd
cmake --build Android_v8a_release --config Release
mkdir ..\prebuilt\release\Plugins\Android\libs\arm64-v8a\
xcopy /Y .\Android_v8a_release\libduktape.so ..\prebuilt\release\Plugins\Android\libs\arm64-v8a\

echo building x86 debug
rd /s /q Android_x86_debug
mkdir Android_x86_debug
pushd Android_x86_debug
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=debug -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=x86-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../..
popd
cmake --build Android_x86_debug --config Release
mkdir ..\prebuilt\debug\Plugins\Android\libs\x86\
xcopy /Y .\Android_x86_debug\libduktape.so ..\prebuilt\debug\Plugins\Android\libs\x86\

echo building x86 release
rd /s /q Android_x86_release
mkdir Android_x86_release
pushd Android_x86_release
cmake -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=release -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=%ANDROID_NDK%/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=x86-clang -DANDROID_NATIVE_API_LEVEL=android-9 -G "NMake Makefiles" ../..
popd
cmake --build Android_x86_release --config Release
mkdir ..\prebuilt\release\Plugins\Android\libs\x86\
xcopy /Y .\Android_x86_release\libduktape.so ..\prebuilt\release\Plugins\Android\libs\x86\

popd
