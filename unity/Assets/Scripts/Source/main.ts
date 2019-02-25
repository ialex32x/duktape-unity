import { Foo } from "./mm/foo"

let foo = new Foo(12)

foo.greet()

// UnityEngine.Debug.Log("greeting")
let go = new UnityEngine.GameObject("testing")
let go2 = new UnityEngine.GameObject("testing2")
let cgo = go.Foo()
console.log("back ref test", cgo == go, go == go2, cgo == go2)
console.log("go.activeSelf", go.activeSelf)
go.SetActive(false)
console.log("go.activeSelf", go.activeSelf)
// UnityEngine.Object.Destroy(go)

let ss = new SampleStruct()

ss.field_a = 12345
console.log(ss.field_a)

let sc = new SampleClass("testcase of SampleClass")
sc.SetEnum(SampleEnum.b)
console.log(`${sc.name}.sum = ${sc.Sum([1, 2, 3, 4, 5])}`)
console.log(`${sc.sampleEnum}`)
