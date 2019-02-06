# prerequisities
pip install PyYAML

cd duktape-2.3.0
python .\tools\configure.py --source-directory src-input --output-directory src-custom --config-metadata config --dll -DDUK_USE_FATAL_HANDLER
cd ..
mkdir build
mkdir x86
cd x86
cmake ..
cd ..
mkdir x64
cd x64
cmake -A 64 ..
cd ..

