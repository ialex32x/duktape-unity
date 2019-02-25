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
console.log(ss.field_a);
var sc = new SampleClass("testcase of SampleClass");
sc.SetEnum(SampleEnum.b);
console.log(sc.name + ".sum = " + sc.Sum([1, 2, 3, 4, 5]));
console.log("" + sc.sampleEnum);
//# sourceMappingURL=main.js.map