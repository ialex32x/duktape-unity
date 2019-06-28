
class MyCircleBridge {
    gameObject: GameObject
    transform: Transform
    rot = 0

    root_cw: GameObject
    root_ccw: GameObject

    Awake() {
        console.log(this.gameObject)

        let cube = GameObject.Find("/abox")
        this.root_cw = new GameObject("cube instances cw")
        this.root_cw.transform.localPosition = Vector3.zero
        this.root_ccw = new GameObject("cube instances ccw")
        this.root_ccw.transform.localPosition = Vector3.zero

        let secs = 10
        let up = new Vector3(0, 5, 0)
        // let copy = <GameObject>UObject.Instantiate(cube, cube.transform)
        // copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, 45))
        for (let i = 0; i < secs; i++) {
            let slice = i * 360 / secs
            let copy = <GameObject>UObject.Instantiate(cube, this.root_cw.transform)
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice))
        }
        for (let i = 0; i < secs; i++) {
            let slice = i * 360 / secs
            let copy = <GameObject>UObject.Instantiate(cube, this.root_ccw.transform)
            copy.transform.localPosition = UnityExtensions.Vector3Rot(up, Quaternion.Euler(0, 0, slice))
        }
    }

    Update() {
        this.rot += Time.deltaTime * 50
        this.root_cw.transform.localRotation = Quaternion.Euler(0, 0, this.rot)
        this.root_ccw.transform.localRotation = Quaternion.Euler(0, 0, -this.rot)
    }
}

function circle() {
    let bridge = UnityEngine.Camera.main.gameObject.AddComponent(DuktapeJS.Bridge)
    bridge.SetBridge(new MyCircleBridge())
}
