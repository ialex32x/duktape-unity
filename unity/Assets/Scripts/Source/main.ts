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

// setInterval(() => {
//     console.log("interval tick 2")
// }, 2000)

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


let Vector3 = function (x, y, z) {
    this.push(x)
    this.push(y)
    this.push(z)
}
Vector3.prototype = Object.setPrototypeOf({}, Array).prototype 
Vector3.prototype.toString = function () {
    return "test"
}
Object.defineProperties(Vector3.prototype, {
    "normalized": {
        get: function () {
            let rlen = 1 / Math.sqrt(this[0] * this[0] + this[1] * this[1] + this[2] * this[2])
            return new Vector3(this[0] * rlen, this[1] * rlen, this[2] * rlen)
        }
    },
    "x": {
        get: function () {
            return this[0]
        },
        set: function (v) {
            this[0] = v
        }
    },
    "y": {
        get: function () {
            return this[1]
        },
        set: function (v) {
            this[1] = v
        }
    },
    "z": {
        get: function () {
            return this[2]
        },
        set: function (v) {
            this[2] = v
        }
    }
})
let scyy = new SampleClass("scyy")
let v3 = new Vector3(1, 2, 3)
scyy.TestVector3(v3)
console.log(v3.x, v3.y, v3.z)
