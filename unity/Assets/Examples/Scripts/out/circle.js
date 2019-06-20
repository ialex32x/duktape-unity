// enable js stacktrace in print (= console.log)
enableStacktrace(true);
addSearchPath("Assets/Examples/Scripts/libs");
var UObject = UnityEngine.Object;
var GameObject = UnityEngine.GameObject;
var Transform = UnityEngine.Transform;
var Vector3 = UnityEngine.Vector3;
var Quaternion = UnityEngine.Quaternion;
var Time = UnityEngine.Time;
var cube = GameObject.Find("/Cube");
var root_cw = new GameObject("cube instances cw");
root_cw.transform.localPosition = Vector3.zero;
var root_ccw = new GameObject("cube instances ccw");
root_ccw.transform.localPosition = Vector3.zero;
var MyBridge = /** @class */ (function () {
    function MyBridge() {
        this.rot = 0;
    }
    MyBridge.prototype.Awake = function () {
        console.log(this.gameObject);
    };
    MyBridge.prototype.Start = function () {
        var secs = 10;
        var up = new Vector3(0, 5, 0);
        // let copy = <GameObject>UObject.Instantiate(cube, cube.transform)
        // copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, 45))
        for (var i = 0; i < secs; i++) {
            var slice = i * 360 / secs;
            var copy = UObject.Instantiate(cube, root_cw.transform);
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice));
        }
        for (var i = 0; i < secs; i++) {
            var slice = i * 360 / secs;
            var copy = UObject.Instantiate(cube, root_ccw.transform);
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice));
        }
    };
    MyBridge.prototype.Update = function () {
        this.rot += Time.deltaTime * 50;
        root_cw.transform.localRotation = Quaternion.Euler(0, 0, this.rot);
        root_ccw.transform.localRotation = Quaternion.Euler(0, 0, -this.rot);
    };
    return MyBridge;
}());
var bridge = UnityEngine.Camera.main.gameObject.AddComponent(DuktapeJS.Bridge);
bridge.SetBridge(new MyBridge());
//# sourceMappingURL=circle.js.map