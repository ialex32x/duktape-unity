
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
let o = new Vector3(1, 2, 3)

console.log(o instanceof Array, o.length)
console.log(o[0], o.x)
console.log(o[1], o.y)
o.x = 3
o.y = 4
console.log(o[0], o.x)
console.log(o[1], o.y)
console.log(o.normalized)

// console.log(p.x, p.y)
// p.x = 3
// p.y = 4
// console.log(p.x, p.y)

