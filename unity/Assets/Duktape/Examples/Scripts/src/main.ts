
import { sampleTests } from "./sample";
import { circle } from "./circle";
import { fmathTest } from "./fmath";
import { ComponentSystem } from "./ut/component_system";
import { promiseTest } from "./promise_test";

// setTimeout(() => {
//     try {
//         let i = UnityEngine.GameObject.Find("/Canvas/Button").GetComponent(UnityEngine.UI.Image);
//         i.sprite = <any> {};
//     } catch (err) {
//         // will crash here if you catch an js error and call csharp native methods
//         console.error(err);
//     }
// }, 100);

// if (!window["__reloading"]) {
//     console.log("hello, javascript! (no stacktrace)", DuktapeJS.DUK_VERSION);
//     // enable js stacktrace in print (= console.log)
//     enableStacktrace(true);
//     console.log("hello, javascript! again!! (with stacktrace)");

//     addSearchPath("Assets/Duktape/Examples/Scripts/libs");

//     window["Promise"] = require("bluebird.core.js");
//     dofile("protobuf-library.js");
//     dofile("test.pb.js");

//     sampleTests();
//     circle();
//     fmathTest();
//     promiseTest();

//     new ComponentSystem();
// }

setTimeout(function () {
    console.log("timeout here")
}, 50000);

console.log(UnityExtensions);