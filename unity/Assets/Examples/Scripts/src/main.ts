/// <reference path="./ut/component_system.ts" />

console.log("hello, javascript! (no stacktrace)");
// enable js stacktrace in print (= console.log)
enableStacktrace(true);
console.log("hello, javascript! again!! (with stacktrace)");

addSearchPath("Assets/Examples/Scripts/libs");

dofile("protobuf-library.js");
dofile("test.pb.js");

import UObject = UnityEngine.Object;
import GameObject = UnityEngine.GameObject;
import Transform = UnityEngine.Transform;
import Vector3 = UnityEngine.Vector3;
import Quaternion = UnityEngine.Quaternion;
import Time = UnityEngine.Time;

sample();
circle();

new ut.ComponentSystem();
