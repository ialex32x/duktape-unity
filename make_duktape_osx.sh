#!/usr/bin/env bash

rm -rf build/osx
mkdir -p build/osx
cd build/osx

cmake -GXcode ..
cd ..
cmake --build osx --config Release
cd ..
mkdir -p ./unity/Assets/Duktape/Plugins/duktape.bundle/Contents/MacOS/
cp ./build/osx/Release/duktape.bundle/Contents/MacOS/duktape ./unity/Assets/Duktape/Plugins/duktape.bundle/Contents/MacOS/

