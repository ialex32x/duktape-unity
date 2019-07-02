#!/usr/bin/env sh

if [ -z "$ANDROID_NDK" ]; then
    export ANDROID_NDK=~/android-ndk-r15c
fi

echo building v7a
rm -rf build/Android_v7a
mkdir -p build/Android_v7a
cd build/Android_v7a
cmake -DANDROID_ABI=armeabi-v7a -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9  ../..
cd ..
cmake --build Android_v7a --config Release
cd ..
mkdir -p ./unity/Assets/Duktape/Plugins/Android/libs/armeabi-v7a/
cp ./build/Android_v7a/libduktape.so ./unity/Assets/Duktape/Plugins/Android/libs/armeabi-v7a/


echo building v8a
rm -rf build/Android_v8a
mkdir -p build/Android_v8a
cd build/Android_v8a
cmake -DANDROID_ABI=arm64-v8a -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=arm-linux-androideabi-clang -DANDROID_NATIVE_API_LEVEL=android-9  ../..
cd ..
cmake --build Android_v8a --config Release
cd ..
mkdir -p ./unity/Assets/Duktape/Plugins/Android/libs/arm64-v8a/
cp ./build/Android_v8a/libduktape.so ./unity/Assets/Duktape/Plugins/Android/libs/arm64-v8a/


echo building x86
rm -rf build/Android_x86
mkdir -p build/Android_x86
cd build/Android_x86
cmake -DANDROID_ABI=x86 -DCMAKE_TOOLCHAIN_FILE=$ANDROID_NDK/build/cmake/android.toolchain.cmake -DANDROID_TOOLCHAIN_NAME=x86-clang -DANDROID_NATIVE_API_LEVEL=android-9  ../..
cd ..
cmake --build Android_x86 --config Release
cd ..
mkdir -p ./unity/Assets/Duktape/Plugins/Android/libs/x86/
cp ./build/Android_x86/libduktape.so ./unity/Assets/Duktape/Plugins/Android/libs/x86/

