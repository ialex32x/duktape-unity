"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var GameObject = UnityEngine.GameObject;
var Vector3 = UnityEngine.Vector3;
var Time = UnityEngine.Time;
var Quaternion = UnityEngine.Quaternion;
var UObject = UnityEngine.Object;
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
exports.MyCircleBridge = MyCircleBridge;
function circle() {
    var bridge = UnityEngine.Camera.main.gameObject.AddComponent(DuktapeJS.Bridge);
    var target = new MyCircleBridge();
    target.gameObject = bridge.gameObject;
    target.transform = bridge.transform;
    bridge.SetBridge(target);
}
exports.circle = circle;
//# sourceMappingURL=circle.js.map