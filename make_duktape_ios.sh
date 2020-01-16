#!/usr/bin/env sh

rm -rf build/ios_debug
mkdir -p build/ios_debug
cd build/ios_debug
cmake -DDUKTAPE_SRC_CAT=debug -DCMAKE_TOOLCHAIN_FILE=../../cmake/ios.toolchain.cmake -DPLATFORM=OS64 -GXcode ../..
cd ..
cmake --build ios_debug --config Release
cd ..
mkdir -p ./prebuilt/debug/Plugins/iOS/
cp ./build/ios_debug/Release-iphoneos/libduktape.a ./prebuilt/debug/Plugins/iOS/


rm -rf build/ios_release
mkdir -p build/ios_release
cd build/ios_release
cmake -DDUKTAPE_SRC_CAT=release -DCMAKE_TOOLCHAIN_FILE=../../cmake/ios.toolchain.cmake -DPLATFORM=OS64 -GXcode ../..
cd ..
cmake --build ios_release --config Release
cd ..
mkdir -p ./prebuilt/release/Plugins/iOS/
cp ./build/ios_release/Release-iphoneos/libduktape.a ./prebuilt/release/Plugins/iOS/

