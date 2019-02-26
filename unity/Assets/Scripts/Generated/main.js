"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var foo_1 = require("./mm/foo");
var foo = new foo_1.Foo(12);
foo.greet();
// UnityEngine.Debug.Log("greeting")
var go = new UnityEngine.GameObject("testing");
var go2 = new UnityEngine.GameObject("testing2");
var cgo = go.Foo();
console.log("back ref test", cgo == go, go == go2, cgo == go2);
console.log("go.activeSelf", go.activeSelf);
go.SetActive(false);
console.log("go.activeSelf", go.activeSelf);
// UnityEngine.Object.Destroy(go)
var ss = new SampleStruct();
ss.field_a = 12345;
console.log("ss.field_a = " + ss.field_a);
var sc = new SampleClass("testcase of SampleClass:", "a1", "a2", "a3");
sc.SetEnum(SampleEnum.b);
console.log(sc.name + ".sum = " + sc.Sum([1, 2, 3, 4, 5]));
console.log("sampleEnum = " + sc.sampleEnum);
var res1 = sc.CheckingVA(1, 2, 3, 4, 5);
var res2 = sc.CheckingVA2(1, 2, 3, 4, 5);
console.log("res1 = " + res1);
console.log("res2 = " + res2);
for (var p in DuktapeJS.Enum) {
    console.log(p);
}
var timer1 = setInterval(function () {
    console.log("interval tick");
}, 1000);
setTimeout(function (a, b) {
    console.log("timeout tick", a, b);
    clearInterval(timer1);
}, 3500, "额外参数1", 222);
//# sourceMappingURL=main.js.map