#!/usr/bin/env sh

# -DCMAKE_OSX_DEPLOYMENT_TARGET:STRING=10.13

rm -rf build/osx_debug
mkdir -p build/osx_debug
cd build/osx_debug
cmake -DDUKTAPE_SRC_CAT=debug -GXcode ../..
cd ..
cmake --build osx_debug --config Release
cd ..
mkdir -p ./prebuilt/debug/Plugins/duktape.bundle/Contents/MacOS/
cp ./build/osx_debug/Release/duktape.bundle/Contents/MacOS/duktape ./prebuilt/debug/Plugins/duktape.bundle/Contents/MacOS/

rm -rf build/osx_release
mkdir -p build/osx_release
cd build/osx_release
cmake -DDUKTAPE_SRC_CAT=release -GXcode ../..
cd ..
cmake --build osx_release --config Release
cd ..
mkdir -p ./prebuilt/release/Plugins/duktape.bundle/Contents/MacOS/
cp ./build/osx_release/Release/duktape.bundle/Contents/MacOS/duktape ./prebuilt/release/Plugins/duktape.bundle/Contents/MacOS/

