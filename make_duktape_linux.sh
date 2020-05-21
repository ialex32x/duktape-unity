#!/usr/bin/env sh

rm -rf build/linux_debug
mkdir -p build/linux_debug
cd build/linux_debug
cmake -DDUKTAPE_SRC_CAT=debug ../..
cd ..
cmake --build linux_debug --config Debug
cd ..
mkdir -p ./prebuilt/debug/Plugins/duktape.bundle/Contents/MacOS/
cp ./build/linux_debug/Debug/duktape.bundle/Contents/MacOS/duktape ./prebuilt/debug/Plugins/duktape.bundle/Contents/MacOS/

rm -rf build/linux_release
mkdir -p build/linux_release
cd build/linux_release
cmake -DDUKTAPE_SRC_CAT=release ../..
cd ..
cmake --build linux_release --config Release
cd ..
mkdir -p ./prebuilt/release/Plugins/duktape.bundle/Contents/MacOS/
cp ./build/linux_release/Release/duktape.bundle/Contents/MacOS/duktape ./prebuilt/release/Plugins/duktape.bundle/Contents/MacOS/

