
import "./mm/foo"

(function () {
    let Vector3 = UnityEngine.Vector3
    let start = Date.now()
	for (let i = 1; i < 200000; i++) {
		let v = new Vector3(i, i, i)
		v.Normalize()
    }
	console.log("vector3/js ", (Date.now() - start) / 1000);
})();

(function () {
    console.log("### Vector3 (replaced)")
    let v1 = new UnityEngine.Vector3(1, 2, 3)
    console.log(`v: ${v1.x}, ${v1.y}, ${v1.z} (${v1.magnitude})`)
    console.log(`v: ${v1[0]}, ${v1[1]}, ${v1[2]}`)
    let v2 = v1.normalized
    console.log(`v: ${v2.x}, ${v2.y}, ${v2.z} (${v2.magnitude})`)
    v2.x += 10
    console.log(`v: ${v2.x}, ${v2.y}, ${v2.z} (${v2.magnitude})`)

    let q1 = new UnityEngine.Quaternion(1, 2, 3, 1)
    console.log(`q: ${q1.x}, ${q1.y}, ${q1.z} eulerAngles: ${q1.eulerAngles.ToString()}`)
})();

(function () {
    console.log("### Delegates begin")
    let d = new DuktapeJS.Delegate0<void>()
    d.on(this, () => {
        console.log("delegate0")
    })
    d.dispatch()
    d.clear()
    d.dispatch()
    console.log("### Delegates end")
})();

(function () {
    SampleNamespace.SampleClass.TestDelegate(function () {
        console.log(this, "TestDelegate")
    })
    let d = new DuktapeJS.Dispatcher()
    d.on("this", function () {
        console.log(this, "TestDelegate")
    })
    SampleNamespace.SampleClass.TestDelegate(d)
})();

console.log(UnityEngine.Mathf.PI)
// UnityEngine.Debug.Log("greeting")
let go = new UnityEngine.GameObject("testing")
let hello = go.AddComponent(SampleNamespace.Hello)
let bridge = go.AddComponent(DuktapeJS.Bridge)

bridge.SetBridge({
    OnEnable: () => {
        console.log("bridge.OnEnable")
    }, 

    Start: () => {
        console.log("bridge.Start")
    }, 

    OnDisable: () => {
        console.log("bridge.OnDisable")
    }, 

    OnDestroy: () => {
        console.log("bridge.OnDestroy")
    }, 
})

console.log("hello.name = ", hello.gameObject.name)
// let go2 = new UnityEngine.GameObject("testing2")
console.log("go.activeSelf", go.activeSelf)
console.log("go.activeSelf", go.activeSelf)

setTimeout(() => {
    go.SetActive(false)
}, 3500)

setTimeout(() => {
    UnityEngine.Object.Destroy(go)
}, 30000)
