import { Foo } from "./mm/foo"


let foo = new Foo(12)

foo.greet()

// UnityEngine.Debug.Log("greeting")
let go = new UnityEngine.GameObject("testing")
var go2 = new UnityEngine.GameObject("testing2")
var cgo = go.Foo()
console.log("back ref test", cgo == go, go == go2, cgo == go2)
console.log("go.activeSelf", go.activeSelf)
go.SetActive(false)
console.log("go.activeSelf", go.activeSelf)
// UnityEngine.Object.Destroy(go)

var ss = new SampleStruct()

ss.field_a = 12345
console.log(ss.field_a)
