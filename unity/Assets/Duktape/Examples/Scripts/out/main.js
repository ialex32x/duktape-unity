"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var sample_1 = require("./sample");
var circle_1 = require("./circle");
var fmath_1 = require("./fmath");
var component_system_1 = require("./ut/component_system");
var promise_test_1 = require("./promise_test");
setTimeout(function () {
    try {
        var i = UnityEngine.GameObject.Find("/Canvas/Button").GetComponent(UnityEngine.UI.Image);
        i.sprite = null;
    }
    catch (err) {
        console.error(err);
    }
}, 100);
if (!window["__reloading"]) {
    console.log("hello, javascript! (no stacktrace)", DuktapeJS.DUK_VERSION);
    // enable js stacktrace in print (= console.log)
    enableStacktrace(true);
    console.log("hello, javascript! again!! (with stacktrace)");
    addSearchPath("Assets/Duktape/Examples/Scripts/libs");
    window["Promise"] = require("bluebird.core.js");
    dofile("protobuf-library.js");
    dofile("test.pb.js");
    sample_1.sampleTests();
    circle_1.circle();
    fmath_1.fmathTest();
    promise_test_1.promiseTest();
    new component_system_1.ComponentSystem();
}
// window["OnBeforeSourceReload"] = function () {
//     console.log("before source reload");
//     window["__reloading"] = true;
// }
// window["OnAfterSourceReload"] = function () {
//     console.log("after source reload !!!");
// }
//# sourceMappingURL=main.js.map