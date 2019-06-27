
// enable js stacktrace in print (= console.log)
enableStacktrace(true)

addSearchPath("Assets/Examples/Scripts/libs")

import UObject = UnityEngine.Object
import GameObject = UnityEngine.GameObject
import Transform = UnityEngine.Transform
import Vector3 = UnityEngine.Vector3
import Quaternion = UnityEngine.Quaternion
import Time = UnityEngine.Time

let cube = GameObject.Find("/Cube")
let root_cw = new GameObject("cube instances cw")
root_cw.transform.localPosition = Vector3.zero
let root_ccw = new GameObject("cube instances ccw")
root_ccw.transform.localPosition = Vector3.zero

class MyBridge {
    gameObject: GameObject
    transform: Transform
    rot = 0

    Awake() {
        console.log(this.gameObject)
    }

    Start() {
        let secs = 10
        let up = new Vector3(0, 5, 0)
        // let copy = <GameObject>UObject.Instantiate(cube, cube.transform)
        // copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, 45))
        for (let i = 0; i < secs; i++) {
            let slice = i * 360 / secs
            let copy = <GameObject>UObject.Instantiate(cube, root_cw.transform)
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice))
        }
        for (let i = 0; i < secs; i++) {
            let slice = i * 360 / secs
            let copy = <GameObject>UObject.Instantiate(cube, root_ccw.transform)
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice))
        }
    }

    Update() {
        this.rot += Time.deltaTime * 50
        root_cw.transform.localRotation = Quaternion.Euler(0, 0, this.rot)
        root_ccw.transform.localRotation = Quaternion.Euler(0, 0, -this.rot)
    }
}

let bridge = UnityEngine.Camera.main.gameObject.AddComponent(DuktapeJS.Bridge)
bridge.SetBridge(new MyBridge())

let f1 = FMath.from_int(2)
let f2 = FMath.from_int(5000)
for (let i = 0; i < 5; i++) {
    f2 = FMath.div(f2, f1)
    console.log(FMath.to_number(f2), FMath.to_number(FMath.sin(f2)))
}
