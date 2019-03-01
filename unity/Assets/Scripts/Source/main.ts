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
console.log(`ss.field_a = ${ss.field_a}`)

let scxx = new SampleClass("testcase of SampleClass:", "a1", "a2", "a3")
scxx.SetEnum(SampleEnum.b)
scxx.TestVector3([1, 2, 3])
scxx.delegateFoo1 = new Delegate2()
scxx.delegateFoo1.on(scxx, (a, b) => {
    console.log("delegate callback from SampleClass", a, b)
})
scxx.TestDelegate1()

console.log(`${scxx.name}.sum = ${scxx.Sum([1, 2, 3, 4, 5])}`)
console.log(`sampleEnum = ${scxx.sampleEnum}`)
let res1 = scxx.CheckingVA(1, 2, 3, 4, 5)
let res2 = scxx.CheckingVA2(1, 2, 3, 4, 5)
console.log(`res1 = ${res1}`)
console.log(`res2 = ${res2}`)

for (let p in DuktapeJS.Enum) {
    console.log(p)
}

let timer1 = setInterval(() => {
    console.log("interval tick 1")
}, 1000)

setInterval(() => {
    console.log("interval tick 2")
}, 2000)

setTimeout((a, b) => {
    console.log("timeout tick", a, b)
    clearInterval(timer1)
}, 3500, "额外参数1", 222)

console.log("JSON?:", JSON)
console.log("encodeURIComponent?:", encodeURIComponent)
console.log("decodeURIComponent?:", decodeURIComponent)

// DuktapeJS.Delegate.on(this, (a: string) => {
//     console.log(a)
// })
