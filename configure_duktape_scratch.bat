pushd duktape-2.3.0
python .\tools\configure.py --source-directory src-input --output-directory src-scratch --config-metadata config -DDUK_USE_FATAL_HANDLER -DDUK_USE_FUNC_FILENAME_PROPERTY
popd
