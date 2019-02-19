import { Foo } from "./mm/foo"


let foo = new Foo(12)

foo.greet()

// UnityEngine.Debug.Log("greeting")
let go = new UnityEngine.GameObject("testing")
var cgo = go.Foo()
console.log("back ref test", cgo === go)
console.log("go.activeSelf", go.activeSelf)
go.SetActive(false)
console.log("go.activeSelf", go.activeSelf)
// UnityEngine.Object.Destroy(go)
