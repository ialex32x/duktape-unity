"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./mm/foo");
(function () {
    var Vector3 = UnityEngine.Vector3;
    var start = Date.now();
    for (var i = 1; i < 200000; i++) {
        var v = new Vector3(i, i, i);
        v.Normalize();
    }
    console.log("test3/js ", (Date.now() - start) / 1000);
})();
(function () {
    console.log("### Vector3 (replaced)");
    var v1 = new UnityEngine.Vector3(1, 2, 3);
    console.log("v: " + v1.x + ", " + v1.y + ", " + v1.z + " (" + v1.magnitude + ")");
    console.log("v: " + v1[0] + ", " + v1[1] + ", " + v1[2]);
    var v2 = v1.normalized;
    console.log("v: " + v2.x + ", " + v2.y + ", " + v2.z + " (" + v2.magnitude + ")");
    v2.x += 10;
    console.log("v: " + v2.x + ", " + v2.y + ", " + v2.z + " (" + v2.magnitude + ")");
})();
// DuktapeJS.Behaviour = function () { }
// class MyBehaviour extends DuktapeJS.Behaviour {
//     Awake() {
//         console.log("Awake")
//     }
//     OnEnable() {
//         console.log("OnEnable")
//     }
//     OnDestroy() {
//         console.log("OnDestroy")
//     }
// }
// (function () {
//     let go = new UnityEngine.GameObject("Bridge")
//     go.AddComponent(MyBehaviour)
// })();
(function () {
    console.log("### Delegates begin");
    var d = new DuktapeJS.Delegate0();
    d.on(this, function () {
        console.log("delegate0");
    });
    d.dispatch();
    d.clear();
    d.dispatch();
    console.log("### Delegates end");
})();
// (function () {
//     console.log("### System.Array")
//     let nativeArray = System.Array.CreateInstance(System.Int32, 10)
//     let s = new SampleNamespace.SampleClass("test")
//     console.log(nativeArray)
//     console.log(nativeArray.ToString())
//     console.log(s.GetPositions(nativeArray))
//     s.TestDuktapeArray([1, 2, 3])
// })();
(function () {
    SampleNamespace.SampleClass.TestDelegate(function () {
        console.log(this, "TestDelegate");
    });
    var d = new DuktapeJS.Dispatcher();
    d.on("this", function () {
        console.log(this, "TestDelegate");
    });
    SampleNamespace.SampleClass.TestDelegate(d);
})();
console.log(UnityEngine.Mathf.PI);
// UnityEngine.Debug.Log("greeting")
var go = new UnityEngine.GameObject("testing");
var hello = go.AddComponent(SampleNamespace.Hello);
console.log("hello.name = ", hello.gameObject.name);
// let go2 = new UnityEngine.GameObject("testing2")
console.log("go.activeSelf", go.activeSelf);
console.log("go.activeSelf", go.activeSelf);
setTimeout(function () {
    go.SetActive(false);
}, 15000);
setTimeout(function () {
    UnityEngine.Object.Destroy(go);
}, 30000);
//
// let ss = new SampleStruct()
// let SampleClass = SampleNamespace.SampleClass
// ss.field_a = 12345
// console.log(`ss.field_a = ${ss.field_a}`)
// new SampleClass.SampleInnerClass().Foo()
// let scxx = new SampleClass("testcase of SampleClass:", "a1", "a2", "a3")
// scxx.SetEnum(SampleEnum.b)
// scxx.TestVector3([1, 2, 3])
// scxx.delegateFoo1 = new Delegate2()
// scxx.delegateFoo1.on(scxx, (a, b) => {
//     console.log("delegate callback from SampleClass", a, b)
// })
// scxx.TestDelegate1()
// console.log(`[JS] TestType1: ${scxx.TestType1(SampleClass)}`)
// console.log(`${scxx.name}.sum = ${scxx.Sum([1, 2, 3, 4, 5])}`)
// console.log(`sampleEnum = ${scxx.sampleEnum}`)
// let res1 = scxx.CheckingVA(1, 2, 3, 4, 5)
// let res2 = scxx.CheckingVA2(1, 2, 3, 4, 5)
// console.log(`res1 = ${res1}`)
// console.log(`res2 = ${res2}`)
// for (let p in DuktapeJS.Enum) {
//     console.log(p)
// }
// let timer1 = setInterval(() => {
//     console.log("interval tick 1")
// }, 1000)
// setInterval(() => {
//     console.log("interval tick 2")
// }, 2000)
// setTimeout((a, b) => {
//     console.log("timeout tick", a, b)
//     clearInterval(timer1)
// }, 3500, "额外参数1", 222)
// let scyy = new SampleClass("scyy")
// let v3 = new Vector3(1, 2, 3)
// scyy.TestVector3(v3)
// console.log(v3.x, v3.y, v3.z)
// var ev = new DuktapeJS.EventDispatcher()
// class JSO {
//     name: string
//     constructor(name: string) {
//         this.name = name
//     }
//     foo1(phase) {
//         console.log(phase, this.name, "foo1")
//     }
//     foo2(phase) {
//         console.log(phase, this.name, "foo2")
//     }
// }
// let jso1 = new JSO("A")
// let jso2 = new JSO("B")
// ev.on("test", jso1, jso1.foo1)
// ev.on("test", jso1, jso1.foo2)
// ev.on("test", jso2, jso2.foo1)
// ev.on("test", jso2, jso2.foo2)
// ev.dispatch("test", "FIRST")
// ev.off("test", jso1, jso1.foo1)
// console.log(ev.events["test"].handlers)
// ev.dispatch("test", "SECOND")
// ev.off("test", jso2)
// ev.dispatch("test", "THIRD")
//# sourceMappingURL=main.js.map