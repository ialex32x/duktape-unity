/// <reference path="./ut/component_system.ts" />

import UObject = UnityEngine.Object;
import GameObject = UnityEngine.GameObject;
import Transform = UnityEngine.Transform;
import Vector3 = UnityEngine.Vector3;
import Quaternion = UnityEngine.Quaternion;
import Time = UnityEngine.Time;

if (!window["__reloading"]) {
    console.log("hello, javascript! (no stacktrace)", DuktapeJS.DUK_VERSION);
    // enable js stacktrace in print (= console.log)
    enableStacktrace(true);
    console.log("hello, javascript! again!! (with stacktrace)");

    addSearchPath("Assets/Examples/Scripts/libs");

    window["Promise"] = require("bluebird.core.js");
    dofile("protobuf-library.js");
    dofile("test.pb.js");

    sample();
    circle();
    fmathtest();

    new Promise((resolve: (value: any) => void) => {
        console.log("promise.resolve", Time.realtimeSinceStartup);
        setTimeout(() => {
            resolve(123);
        }, 1000);
    }).then((value: any) => {
        console.log("promise.then", value, Time.realtimeSinceStartup);
    });
    console.log("timeout begin", Time.realtimeSinceStartup)
    setTimeout(() => {
        console.log("timeout 3s", Time.realtimeSinceStartup)
    }, 1000 * 3);

    console.log("interval begin", Time.realtimeSinceStartup)
    setInterval(() => {
        console.log("interval 15s", Time.realtimeSinceStartup)
    }, 1000 * 15);

    new ut.ComponentSystem();
}

window["OnBeforeSourceReload"] = function () {
    console.log("before source reload");
    window["__reloading"] = true;
}

window["OnAfterSourceReload"] = function () {
    console.log("after source reload !!!");
}
