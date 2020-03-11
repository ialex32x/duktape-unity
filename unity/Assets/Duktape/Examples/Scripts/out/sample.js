"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var http_1 = require("./duktape/http");
var coroutine_1 = require("./duktape/coroutine");
var Time = UnityEngine.Time;
function sampleTests() {
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
        console.log("http requesting...");
        http_1.HttpRequest.GET("http://t.weather.sojson.com/api/weather/city/101030100", null, function (status, res) {
            console.warn("http response:", status, res);
            if (status) {
                var obj = JSON.parse(res);
                console.log("as object", obj.message);
            }
        });
    })();
    (function () {
        var Vector3 = UnityEngine.Vector3;
        var start;
        var DoNothing = SampleNamespace.SampleClass.DoNothing;
        start = Date.now();
        for (var i = 1; i < 1000000; i++) {
            DoNothing();
        }
        SampleNamespace.SampleClass.WriteLog("js/DoNothing: " + (Date.now() - start) / 1000);
        var DoNothing1 = SampleNamespace.SampleClass.DoNothing1;
        start = Date.now();
        for (var i = 1; i < 1000000; i++) {
            DoNothing1(i);
        }
        SampleNamespace.SampleClass.WriteLog("js/DoNothing1: " + (Date.now() - start) / 1000);
        var v1 = new Vector3(0, 0, 0);
        start = Date.now();
        for (var i = 1; i < 200000; i++) {
            v1.Set(i, i, i);
            v1.Normalize();
        }
        console.log("js/vector3/normailize", (Date.now() - start) / 1000);
        var v = Vector3.zero;
        var w = Vector3.one;
        start = Date.now();
        for (var i = 1; i < 200000; i++) {
            v.Scale(w);
        }
        console.log("js/vector3/scale", (Date.now() - start) / 1000);
        var sum = 0;
        start = Date.now();
        for (var i = 1; i < 20000000; i++) {
            sum += i;
        }
        console.log("js/number/add", (Date.now() - start) / 1000, sum);
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
        SampleNamespace.SampleClass.staticTestEvent.on(function () {
            console.log("sampleClass.staticTestEvent invoked!!!!");
        });
        SampleNamespace.SampleClass.DispatchStaticTestEvent();
        var sampleClass = new SampleNamespace.SampleClass("sampleclass.constructor");
        sampleClass.testEvent.on(function () {
            console.log("sampleClass.testEvent invoked!!!!");
        });
        sampleClass.DispatchTestEvent();
        sampleClass.delegateFoo4 = function (a, b) { return a + b; };
        sampleClass.TestDelegate4();
        console.log("trytrytrytry", sampleClass.delegateFoo4);
        var fn = function () {
            console.log(this, "TestDelegate");
        };
        SampleNamespace.SampleClass.TestDelegate(fn);
        SampleNamespace.SampleClass.TestDelegate(fn);
        var d = new DuktapeJS.Dispatcher();
        d.on("this", function () {
            console.log(this, "TestDelegate");
        });
        SampleNamespace.SampleClass.TestDelegate(d);
    })();
    console.log(UnityEngine.Mathf.PI);
    // UnityEngine.Debug.Log("greeting")
    (function () {
        var go = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
        go.name = "testing_cube";
        var hello = go.AddComponent(SampleNamespace.Hello);
        console.log("hello.name = ", hello.gameObject.name);
        console.log("DuktapeJS.Bridge = ", DuktapeJS.Bridge);
        var bridge = go.AddComponent(DuktapeJS.Bridge);
        var MyBridge = /** @class */ (function () {
            function MyBridge(gameObject) {
                this.hitInfo = {};
                this.rotx = 10;
                this.roty = 20;
                this.gameObject = gameObject;
                this.transform = gameObject.transform;
            }
            MyBridge.prototype.OnEnable = function () {
                console.log("bridge.OnEnable");
            };
            MyBridge.prototype.Start = function () {
                console.log("bridge.Start");
                this.transform.localPosition = new UnityEngine.Vector3(3, 0, 0);
            };
            MyBridge.prototype.OnDisable = function () {
                console.log("bridge.OnDisable");
            };
            MyBridge.prototype.Update = function () {
                this.transform.localRotation = UnityEngine.Quaternion.Euler(this.rotx, this.roty, 0);
                this.rotx += Time.deltaTime * 3.0;
                this.roty += Time.deltaTime * 1.5;
                if (UnityEngine.Input.GetMouseButtonUp(0) || UnityEngine.Input.GetKeyUp(UnityEngine.KeyCode.Space)) {
                    if (UnityExtensions.RaycastMousePosition(this.hitInfo, 1000, 1)) {
                        console.log("you clicked " + this.hitInfo.collider.name);
                    }
                }
            };
            MyBridge.prototype.OnDestroy = function () {
                console.log("bridge.OnDestroy");
            };
            return MyBridge;
        }());
        bridge.SetBridge(new MyBridge(go));
    })();
    (function () {
        var go2 = new UnityEngine.GameObject("testing2_wait_destroy");
        console.log("go2.activeSelf", go2.activeSelf);
        console.log("go2.activeSelf", go2.activeSelf);
        setTimeout(function () {
            go2.SetActive(false);
        }, 3500);
        setTimeout(function () {
            UnityEngine.Object.Destroy(go2);
        }, 30000);
        var time = 0;
        setInterval(function () {
            setTimeout(function () {
                time++;
                // setTimeout/setInterval gc test
            }, 50);
        }, 200);
        if (DuktapeJS.Socket) {
            var buffer = new Buffer(1024);
            var sock = new DuktapeJS.Socket(1, 0);
            var count = 0;
            sock.connect("localhost", 1234);
            console.log("buffer.length:", buffer.length);
            setInterval(function () {
                count++;
                sock.send("test" + count);
                var recv_size = sock.recv(buffer, 0, buffer.length);
                if (recv_size > 0) {
                    console.log("echo", buffer.toString("utf8", 0, recv_size));
                }
            }, 1000);
        }
        else {
            console.error("no DuktapeJS.Socket", DuktapeJS.SocketType, DuktapeJS.SocketFamily);
        }
    })();
    (function () {
        // console.log(UnityEngine.UI.Text)
        var textui = UnityEngine.GameObject.Find("/Canvas/Text").GetComponent(UnityEngine.UI.Text);
        if (textui) {
            textui.text = "hello, javascript";
        }
        var buttonui = UnityEngine.GameObject.Find("/Canvas/Button").GetComponent(UnityEngine.UI.Button);
        if (buttonui) {
            var delegate = new DuktapeJS.Delegate0();
            delegate.on(buttonui, function () {
                if (textui) {
                    textui.color = UnityEngine.Color.Lerp(UnityEngine.Color.black, UnityEngine.Color.green, UnityEngine.Random.value);
                }
                console.log("you clicked the button");
            });
            delegate.on(buttonui, function () {
                console.log("another listener", this == buttonui);
            });
            buttonui.onClick.AddListener(delegate);
        }
    })();
    (function () {
        console.log("[Buffer] tests");
        var buffer = SampleNamespace.SampleClass.GetBytes();
        console.log(buffer);
        var str = SampleNamespace.SampleClass.InputBytes(buffer);
        console.log(str);
        setInterval(function () {
            SampleNamespace.SampleClass.AnotherBytesTest(buffer);
        }, 5000);
    })();
    (function () {
        var co = new coroutine_1.Coroutine(function (x) {
            console.log("duktape thread, start:", x);
            for (var i = 1; i <= 5; ++i) {
                var r = coroutine_1.Coroutine.yield(i);
                console.log("duktape thread, yield:", r);
            }
            // Coroutine.break();
            return "all done!";
        });
        var c = 'A'.charCodeAt(0);
        while (co.next(String.fromCharCode(c++))) {
            console.log("duktape thread, next:", co.value);
        }
        console.log("duktape thread, done:", co.value);
    })();
    (function () {
        setTimeout(function () {
            var time_id = setInterval(function () {
                console.log("[setInterval@" + Time.frameCount + "] test zero interval timer1");
            }, 0);
            setTimeout(function () {
                clearInterval(time_id);
                console.log("[setTimeout] clear interval timer1 [id:" + time_id + "]");
            }, 0);
            console.log("[setInterval@" + Time.frameCount + "] before timer2 " + Time.realtimeSinceStartup);
            var timer2frames = 0;
            var time_id2 = setInterval(function () {
                timer2frames++;
                console.log("[setInterval@" + Time.frameCount + "] test zero interval timer2");
            }, 0);
            setTimeout(function () {
                clearInterval(time_id2);
                console.log("[setTimeout] clear interval timer2 [id:" + time_id2 + "] realtime:" + Time.realtimeSinceStartup + " frames:" + timer2frames);
            }, 150);
        }, 5000);
    })();
    // (function () {
    //     console.log("[error] tests");
    //     let u = null;
    //     console.log(u.value);
    // })();
}
exports.sampleTests = sampleTests;
//# sourceMappingURL=sample.js.map