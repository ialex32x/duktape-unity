
import { sampleTests } from "./sample";
import { circle } from "./circle";
import { fmathTest } from "./fmath";
import { ComponentSystem } from "./ut/component_system";
import { promiseTest } from "./promise_test";

if (!window["__reloading"]) {
    console.log("hello, javascript! (no stacktrace)", DuktapeJS.DUK_VERSION);
    // enable js stacktrace in print (= console.log)
    enableStacktrace(true);
    console.log("hello, javascript! again!! (with stacktrace)");

    addSearchPath("Assets/Duktape/Examples/Scripts/libs");

    window["Promise"] = require("bluebird.core.js");
    dofile("protobuf-library.js");
    dofile("test.pb.js");

    sampleTests();
    circle();
    fmathTest();
    promiseTest();

    new ComponentSystem();
}

// window["OnBeforeSourceReload"] = function () {
//     console.log("before source reload");
//     window["__reloading"] = true;
// }

// window["OnAfterSourceReload"] = function () {
//     console.log("after source reload !!!");
// }