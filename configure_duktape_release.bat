
pushd duktape-2.4.0
python .\tools\configure.py --source-directory src-input --output-directory src-release --config-metadata config --dll -DDUK_USE_FATAL_HANDLER
popd
