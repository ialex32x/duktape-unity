
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
let root = new GameObject("cube instances")
root.transform.localPosition = Vector3.zero

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
            let copy = <GameObject>UObject.Instantiate(cube, root.transform)
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice))
        }
    }

    Update() {
        this.rot += Time.deltaTime * 50
        root.transform.localRotation = Quaternion.Euler(0, 0, this.rot)
    }
}

let bridge = UnityEngine.Camera.main.gameObject.AddComponent(DuktapeJS.Bridge)
bridge.SetBridge(new MyBridge())

