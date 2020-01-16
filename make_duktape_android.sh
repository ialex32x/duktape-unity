#!/usr/bin/env sh

if [ -z "$ANDROID_NDK" ]; then
    export ANDROID_NDK=~/android-ndk-r15c
fi

echo building v7a debug
rm -rf build/Android_v7a_debug
mkdir -p build/Android_v7a_debug
cd build/Android_v7a_debug
cmake -DDEBUG=NO -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=debug -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9  ../..
cd ..
cmake --build Android_v7a_debug --config Release
cd ..
mkdir -p ./prebuilt/debug/Plugins/Android/libs/armeabi-v7a/
cp ./build/Android_v7a_debug/libduktape.so ./prebuilt/debug/Plugins/Android/libs/armeabi-v7a/


echo building v7a release
rm -rf build/Android_v7a_release
mkdir -p build/Android_v7a_release
cd build/Android_v7a_release
cmake -DDEBUG=NO -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=release -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9  ../..
cd ..
cmake --build Android_v7a_release --config Release
cd ..
mkdir -p ./prebuilt/release/Plugins/Android/libs/armeabi-v7a/
cp ./build/Android_v7a_release/libduktape.so ./prebuilt/release/Plugins/Android/libs/armeabi-v7a/


echo building v8a debug
rm -rf build/Android_v8a_debug
mkdir -p build/Android_v8a_debug
cd build/Android_v8a_debug
cmake -DDEBUG=NO -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=debug -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9  ../..
cd ..
cmake --build Android_v8a_debug --config Release
cd ..
mkdir -p ./prebuilt/debug/Plugins/Android/libs/arm64-v8a/
cp ./build/Android_v8a_debug/libduktape.so ./prebuilt/debug/Plugins/Android/libs/arm64-v8a/


echo building v8a release
rm -rf build/Android_v8a_release
mkdir -p build/Android_v8a_release
cd build/Android_v8a_release
cmake -DDEBUG=NO -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=release -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9  ../..
cd ..
cmake --build Android_v8a_release --config Release
cd ..
mkdir -p ./prebuilt/release/Plugins/Android/libs/arm64-v8a/
cp ./build/Android_v8a_release/libduktape.so ./prebuilt/release/Plugins/Android/libs/arm64-v8a/


echo building x86 debug
rm -rf build/Android_x86_debug
mkdir -p build/Android_x86_debug
cd build/Android_x86_debug
cmake -DDEBUG=NO -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=debug -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=x86-clang -DANDROID_NATIVE_API_LEVEL=android-9  ../..
cd ..
cmake --build Android_x86_debug --config Release
cd ..
mkdir -p ./prebuilt/debug/Plugins/Android/libs/x86/
cp ./build/Android_x86_debug/libduktape.so ./prebuilt/debug/Plugins/Android/libs/x86/


echo building x86 release
rm -rf build/Android_x86_release
mkdir -p build/Android_x86_release
cd build/Android_x86_release
cmake -DDEBUG=NO -DCMAKE_BUILD_TYPE=RELEASE -DDUKTAPE_SRC_CAT=release -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=x86-clang -DANDROID_NATIVE_API_LEVEL=android-9  ../..
cd ..
cmake --build Android_x86_release --config Release
cd ..
mkdir -p ./prebuilt/release/Plugins/Android/libs/x86/
cp ./build/Android_x86_release/libduktape.so ./prebuilt/release/Plugins/Android/libs/x86/

