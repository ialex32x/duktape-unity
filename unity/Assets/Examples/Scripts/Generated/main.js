"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
require("./mm/foo");
addSearchPath("Assets/Examples/Scripts/libs");
dofile("protobuf-library.js");
dofile("test.pb.js");
// test protobuf
// (function () {
//     // let writer = protobuf.Writer.create()
//     let msg = new protos.Ping()
//     msg.payload = "hello, protobuf"
//     msg.time = 123
//     let w = protos.Ping.encode(msg)
//     let buf = w.finish()
//     // > go run examples\echoserver\src\main.go
//     // you need a simple echo server to run the code below
//     let ws = new DuktapeJS.WebSocket()
//     ws.connect("ws://127.0.0.1:8080/websocket")
//     ws.on("open", this, () => {
//         console.log("ws opened")
//         ws.send(buf)
//     })
//     ws.on("close", this, () => {
//         console.log("ws closed")
//     })
//     ws.on("data", this, data => {
//         let dmsg = protos.Ping.decode(data)
//         console.log(`msg.payload = ${dmsg.payload}`)
//         console.log(`msg.time = ${dmsg.time}`)
//     })
//     let go = new UnityEngine.GameObject("ws")
//     go.AddComponent(DuktapeJS.Bridge).SetBridge({
//         Update: () => {
//             ws.poll()
//         },
//         OnDestroy: () => {
//             console.log("ws close")
//             ws.close()
//         },
//     })
// })();
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
var go = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
go.name = "testing_cube";
var hello = go.AddComponent(SampleNamespace.Hello);
var bridge = go.AddComponent(DuktapeJS.Bridge);
var MyBridge = /** @class */ (function () {
    function MyBridge(gameObject) {
        this.hitInfo = {};
        this.rotx = 10;
        this.roty = 20;
        this.gameObject = gameObject;
    }
    MyBridge.prototype.OnEnable = function () {
        console.log("bridge.OnEnable");
    };
    MyBridge.prototype.Start = function () {
        console.log("bridge.Start");
        this.gameObject.transform.localPosition = new UnityEngine.Vector3(3, 0, 0);
    };
    MyBridge.prototype.OnDisable = function () {
        console.log("bridge.OnDisable");
    };
    MyBridge.prototype.Update = function () {
        this.gameObject.transform.localRotation = UnityEngine.Quaternion.Euler(this.rotx, this.roty, 0);
        this.rotx += UnityEngine.Time.deltaTime * 30;
        this.roty += UnityEngine.Time.deltaTime * 15;
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
bridge.SetBridge(new MyBridge(go));
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