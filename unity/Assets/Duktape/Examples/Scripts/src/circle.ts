import GameObject = UnityEngine.GameObject;
import Transform = UnityEngine.Transform;
import Vector3 = UnityEngine.Vector3;
import Time = UnityEngine.Time;
import Quaternion = UnityEngine.Quaternion;
import UObject = UnityEngine.Object;
import { MyClass } from "./my_class";
import { Profiling, Profiler } from "./duktape/profile";

export class MyCircleBridge {
    gameObject: GameObject
    transform: Transform
    rot = 0

    private myClass: MyClass;

    root_cw: Transform
    root_ccw: Transform

    Awake() {
        this.myClass = new MyClass();
        console.log(this.gameObject)

        let cube = GameObject.Find("/abox")
        let root_cw = new GameObject("cube instances cw")
        this.root_cw = root_cw.transform;
        this.root_cw.localPosition = Vector3.zero
        let root_ccw = new GameObject("cube instances ccw")
        this.root_ccw = root_ccw.transform;
        this.root_ccw.localPosition = Vector3.zero

        let secs = 10
        let up = new Vector3(0, 5, 0)
        // let copy = <GameObject>UObject.Instantiate(cube, cube.transform)
        // copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, 45))
        for (let i = 0; i < secs; i++) {
            let slice = i * 360 / secs
            let copy = <GameObject>UObject.Instantiate(cube, this.root_cw)
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice))
        }
        for (let i = 0; i < secs; i++) {
            let slice = i * 360 / secs
            let copy = <GameObject>UObject.Instantiate(cube, this.root_ccw)
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice))
        }
    }

    @Profiling
    Update() {
        this.rot += Time.deltaTime * 1.2
        this.root_cw.localRotation = Quaternion.Euler(0, 0, this.rot)
        this.root_ccw.localRotation = Quaternion.Euler(0, 0, -this.rot)
        this.myClass.update();
    }
}

export function circle() {
    let bridge = UnityEngine.Camera.main.gameObject.AddComponent(DuktapeJS.Bridge)
    let target = new MyCircleBridge();
    target.gameObject = bridge.gameObject;
    target.transform = bridge.transform;
    bridge.SetBridge(target)
}
