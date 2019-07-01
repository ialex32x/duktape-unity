#!/usr/bin/env sh

rm -rf build/ios
mkdir -p build/ios
cd build/ios

cmake -DCMAKE_TOOLCHAIN_FILE=../../cmake/ios.toolchain.cmake -DPLATFORM=OS64 -GXcode ../..
cd ..
cmake --build ios --config Release
cd ..
mkdir -p ./unity/Assets/Duktape/Plugins/duktape.bundle/Contents/MacOS/
cp ./build/ios/Release-iphoneos/libduktape.a ./unity/Assets/Duktape/Plugins/iOS/

