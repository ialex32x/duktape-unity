var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
var MyCircleBridge = /** @class */ (function () {
    function MyCircleBridge() {
        this.rot = 0;
    }
    MyCircleBridge.prototype.Awake = function () {
        console.log(this.gameObject);
        var cube = GameObject.Find("/abox");
        var root_cw = new GameObject("cube instances cw");
        this.root_cw = root_cw.transform;
        this.root_cw.localPosition = Vector3.zero;
        var root_ccw = new GameObject("cube instances ccw");
        this.root_ccw = root_ccw.transform;
        this.root_ccw.localPosition = Vector3.zero;
        var secs = 10;
        var up = new Vector3(0, 5, 0);
        // let copy = <GameObject>UObject.Instantiate(cube, cube.transform)
        // copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, 45))
        for (var i = 0; i < secs; i++) {
            var slice = i * 360 / secs;
            var copy = UObject.Instantiate(cube, this.root_cw);
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice));
        }
        for (var i = 0; i < secs; i++) {
            var slice = i * 360 / secs;
            var copy = UObject.Instantiate(cube, this.root_ccw);
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice));
        }
    };
    MyCircleBridge.prototype.Update = function () {
        this.rot += Time.deltaTime * 50;
        this.root_cw.localRotation = Quaternion.Euler(0, 0, this.rot);
        this.root_ccw.localRotation = Quaternion.Euler(0, 0, -this.rot);
    };
    return MyCircleBridge;
}());
function circle() {
    var bridge = UnityEngine.Camera.main.gameObject.AddComponent(DuktapeJS.Bridge);
    var target = new MyCircleBridge();
    target.gameObject = bridge.gameObject;
    target.transform = bridge.transform;
    bridge.SetBridge(target);
}
function fmathtest() {
    var f1 = FMath.from_int(2);
    var f2 = FMath.from_int(5000);
    for (var i = 0; i < 5; i++) {
        f2 = FMath.div(f2, f1);
        console.log(FMath.to_number(f2), FMath.to_number(FMath.sin(f2)));
    }
    var seed_start = 0;
    var seed_end = seed_start + 5;
    for (var seed = seed_start; seed < seed_end; seed++) {
        var r = new FMath.Random(seed);
        var c = 0;
        var times = 100;
        for (var i = 0; i < times; i++) {
            // let v = r.range(0, full) / full;
            var v = r.value;
            if (v > 0.5) {
                c++;
            }
        }
        console.log("random:", c * 100 / times, "% with seed:", seed);
    }
}
var ut;
(function (ut) {
    var ComponentSystem = /** @class */ (function () {
        function ComponentSystem() {
            console.log("ComponentSystem");
        }
        return ComponentSystem;
    }());
    ut.ComponentSystem = ComponentSystem;
})(ut || (ut = {}));
var ContentType = /** @class */ (function () {
    function ContentType() {
    }
    ContentType.FORM = "application/x-www-form-urlencoded";
    ContentType.JSON = "application/json";
    return ContentType;
}());
var HttpRequest = /** @class */ (function () {
    function HttpRequest(url, data) {
        this.baseUrl = "";
        this.onfinish = null;
        this._url = url;
        this._data = data;
        this._req = new DuktapeJS.HttpRequest();
    }
    Object.defineProperty(HttpRequest.prototype, "isCancelled", {
        get: function () {
            return this._cancel;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(HttpRequest.prototype, "isDone", {
        get: function () {
            return this._done;
        },
        enumerable: true,
        configurable: true
    });
    /**
     * 将json对象转为 key=val 形式的 query 参数
     */
    HttpRequest.QUERY = function (payload) {
        if (payload) {
            var str = '';
            for (var key in payload) {
                if (str.length > 0) {
                    str += '&';
                }
                str += key + "=" + encodeURIComponent(payload[key]);
            }
            return str;
        }
        return null;
    };
    /**
     * 拼接 url 与 query 参数
     */
    HttpRequest.URL = function (url, payload) {
        var query = this.QUERY(payload);
        if (query && query.length > 0) {
            return url + "?" + query;
        }
        return url;
    };
    // 以 GET 方式发送请求 （参数直接在url中携带）
    HttpRequest.GET = function (url, query, finish) {
        var req = new HttpRequest(this.URL(url, query), undefined);
        req.baseUrl = this.sharedBaseUrl;
        req.onfinish = finish;
        req.get();
        return req;
    };
    // 以 POST 方式发送 JSON 对象
    HttpRequest.POST = function (url, payload, finish) {
        var data = payload && JSON.stringify(payload);
        var req = new HttpRequest(url, data);
        req.baseUrl = this.sharedBaseUrl;
        req.onfinish = finish;
        req.contentType(ContentType.JSON);
        req.post();
        return req;
    };
    // 以 POST 方式发送 表单 数据 (data 内容形式 "a=val1&b=val2&c=123")
    HttpRequest.FORM = function (url, data, finish) {
        var req = new HttpRequest(url, data);
        req.baseUrl = this.sharedBaseUrl;
        req.onfinish = finish;
        req.contentType(ContentType.FORM);
        req.post();
        return req;
    };
    /**
     * 取消请求
     */
    HttpRequest.prototype.cancel = function () {
        if (!this._done) {
            this._cancel = true;
            if (this.onfinish) {
                this.onfinish(false, "request cancelled");
            }
        }
        return this;
    };
    /**
     * 设定超时
     */
    HttpRequest.prototype.timeout = function (seconds) {
        var _this = this;
        if (!this._done) {
            setTimeout(function () {
                if (!_this._done) {
                    _this._cancel = true;
                    if (_this.onfinish) {
                        _this.onfinish(false, "request timeout");
                    }
                }
            }, seconds * 1000);
        }
        return this;
    };
    /**
     * 伪造 User-Agent
     */
    HttpRequest.prototype.userAgent = function (agent) {
        // this._headers.push("User-Agent", agent || "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
        this._req.SetRequestHeader("User-Agent", agent || "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_13_6) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36");
    };
    HttpRequest.prototype.contentType = function (type) {
        // this._headers.push("Content-Type", type);
        this._req.SetRequestHeader("Content-Type", type);
    };
    HttpRequest.prototype.onComplete = function (succ, res) {
        this._done = true;
        // console.log("httprequest", this._cancel, res);
        if (this.onfinish && !this._cancel) {
            this.onfinish(succ, res);
        }
    };
    HttpRequest.prototype.get = function () {
        // console.log("GET", this._url);
        this._req.SendGetRequest(this.baseUrl + this._url, this.onComplete.bind(this));
    };
    HttpRequest.prototype.post = function () {
        // console.log("POST", this._url, this._data);
        this._req.SendPostRequest(this.baseUrl + this._url, this._data, this.onComplete.bind(this));
    };
    HttpRequest.sharedBaseUrl = "";
    return HttpRequest;
}());
/// <reference path="./ut/component_system.ts" />
/// <reference path="./duktape/http.ts" />
var UObject = UnityEngine.Object;
var GameObject = UnityEngine.GameObject;
var Transform = UnityEngine.Transform;
var Vector3 = UnityEngine.Vector3;
var Quaternion = UnityEngine.Quaternion;
var Time = UnityEngine.Time;
if (!window["__reloading"]) {
    console.log("hello, javascript! (no stacktrace)", DuktapeJS.DUK_VERSION);
    // enable js stacktrace in print (= console.log)
    enableStacktrace(true);
    console.log("hello, javascript! again!! (with stacktrace)");
    addSearchPath("Assets/Duktape/Examples/Scripts/libs");
    window["Promise"] = require("bluebird.core.js");
    dofile("protobuf-library.js");
    dofile("test.pb.js");
    sample();
    circle();
    fmathtest();
    new Promise(function (resolve) {
        console.log("promise.resolve", Time.realtimeSinceStartup);
        setTimeout(function () {
            resolve(123);
        }, 1000);
    }).then(function (value) {
        console.log("promise.then", value, Time.realtimeSinceStartup);
    });
    console.log("timeout begin", Time.realtimeSinceStartup);
    setTimeout(function () {
        console.log("timeout 3s", Time.realtimeSinceStartup);
    }, 1000 * 3);
    console.log("interval begin", Time.realtimeSinceStartup);
    setInterval(function () {
        console.log("interval 15s", Time.realtimeSinceStartup);
    }, 1000 * 15);
    new ut.ComponentSystem();
}
// window["OnBeforeSourceReload"] = function () {
//     console.log("before source reload");
//     window["__reloading"] = true;
// }
// window["OnAfterSourceReload"] = function () {
//     console.log("after source reload !!!");
// }
function sample() {
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
        HttpRequest.GET("http://t.weather.sojson.com/api/weather/city/101030100", null, function (status, res) {
            console.warn("http response:", status, res);
            if (status) {
                var obj = JSON.parse(res);
                console.log("as object", obj.message);
            }
        });
    })();
    (function () {
        var Vector3 = UnityEngine.Vector3;
        var start = Date.now();
        var v1 = new Vector3(0, 0, 0);
        for (var i = 1; i < 200000; i++) {
            v1.Set(i, i, i);
            v1.Normalize();
        }
        console.log("js/vector3/normailize", (Date.now() - start) / 1000);
        var v = Vector3.zero;
        var w = Vector3.one;
        for (var i = 1; i < 200000; i++) {
            v.Scale(w);
        }
        console.log("js/vector3/scale", (Date.now() - start) / 1000);
        start = Date.now();
        var sum = 0;
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
                this.rotx += UnityEngine.Time.deltaTime * 30;
                this.roty += UnityEngine.Time.deltaTime * 15;
                if (UnityEngine.Input.GetMouseButtonUp(0)) {
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
    })();
    // (function () {
    //     console.log("[error] tests");
    //     let u = null;
    //     console.log(u.value);
    // })();
}
var mm;
(function (mm) {
    var FooBase = /** @class */ (function () {
        function FooBase() {
        }
        FooBase.prototype.greet = function () {
            console.log("hello, I am nobody");
        };
        return FooBase;
    }());
    mm.FooBase = FooBase;
    var Foo = /** @class */ (function (_super) {
        __extends(Foo, _super);
        function Foo(age) {
            var _this = _super.call(this) || this;
            _this.nickname = "type-t";
            _this.name = "test";
            _this.age = 0;
            _this.age = age;
            return _this;
        }
        Foo.foo = function () {
            console.log("static foo");
        };
        Foo.prototype.greet = function () {
            _super.prototype.greet.call(this);
            console.log("hello, I am " + this.name + ", " + this.age + ".");
        };
        return Foo;
    }(FooBase));
    mm.Foo = Foo;
})(mm || (mm = {}));
//# sourceMappingURL=code.js.map