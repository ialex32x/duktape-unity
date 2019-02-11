
pushd duktape-2.3.0
python .\tools\configure.py --source-directory src-input --output-directory src-custom --config-metadata config --dll -DDUK_USE_FATAL_HANDLER
popd
