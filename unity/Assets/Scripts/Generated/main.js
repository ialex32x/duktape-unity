"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./mm/foo");
dofile("console-minimal.js");
dofile("protobuf-library.js");
dofile("test.pb.js");
// test protobuf
(function () {
    // let writer = protobuf.Writer.create()
    var msg = new protos.Ping();
    msg.payload = "hello, protobuf";
    msg.time = 123;
    var w = protos.Ping.encode(msg);
    var buf = w.finish();
    var dmsg = protos.Ping.decode(buf);
    console.log("msg.payload = " + dmsg.payload);
    console.log("msg.time = " + dmsg.time);
})();
(function () {
    var Vector3 = UnityEngine.Vector3;
    var start = Date.now();
    for (var i = 1; i < 200000; i++) {
        var v = new Vector3(i, i, i);
        v.Normalize();
    }
    console.log("vector3/js ", (Date.now() - start) / 1000);
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
    var q1 = new UnityEngine.Quaternion(1, 2, 3, 1);
    console.log("q: " + q1.x + ", " + q1.y + ", " + q1.z + " eulerAngles: " + q1.eulerAngles.ToString());
})();
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
var bridge = go.AddComponent(DuktapeJS.Bridge);
var MyBridge = /** @class */ (function () {
    function MyBridge() {
        this.hitInfo = {};
    }
    MyBridge.prototype.OnEnable = function () {
        console.log("bridge.OnEnable");
    };
    MyBridge.prototype.Start = function () {
        console.log("bridge.Start");
    };
    MyBridge.prototype.OnDisable = function () {
        console.log("bridge.OnDisable");
    };
    MyBridge.prototype.Update = function () {
        if (UnityEngine.Input.GetMouseButtonUp(0)) {
            if (UnityExtensions.RaycastMousePosition(this.hitInfo, 1000, 1)) {
                console.log("you clicked " + this.hitInfo.collider.name);
            }
            else {
                console.log("you clicked nothing");
            }
        }
    };
    MyBridge.prototype.OnDestroy = function () {
        console.log("bridge.OnDestroy");
    };
    return MyBridge;
}());
bridge.SetBridge(new MyBridge());
console.log("hello.name = ", hello.gameObject.name);
var go2 = new UnityEngine.GameObject("testing2_wait_destroy");
console.log("go2.activeSelf", go2.activeSelf);
console.log("go2.activeSelf", go2.activeSelf);
setTimeout(function () {
    go2.SetActive(false);
}, 3500);
setTimeout(function () {
    UnityEngine.Object.Destroy(go2);
}, 30000);
//# sourceMappingURL=main.js.map