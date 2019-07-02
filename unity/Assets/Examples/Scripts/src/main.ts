/// <reference path="./ut/component_system.ts" />

import UObject = UnityEngine.Object;
import GameObject = UnityEngine.GameObject;
import Transform = UnityEngine.Transform;
import Vector3 = UnityEngine.Vector3;
import Quaternion = UnityEngine.Quaternion;
import Time = UnityEngine.Time;

if (!window["__reloading"]) {
    console.log("hello, javascript! (no stacktrace)");
    // enable js stacktrace in print (= console.log)
    enableStacktrace(true);
    console.log("hello, javascript! again!! (with stacktrace)");

    addSearchPath("Assets/Examples/Scripts/libs");

    dofile("protobuf-library.js");
    dofile("test.pb.js");

    sample();
    circle();

    new ut.ComponentSystem();
}

window["OnBeforeSourceReload"] = function () {
    console.log("before source reload")
    window["__reloading"] = true
}

window["OnAfterSourceReload"] = function () {
    console.log("after source reload !!!")
}
