var Vector3 = DuktapeJS.Vector3
var v1 = new Vector3(1, 2, 3)

print(v1[0], v1[1], v1[2])
print(v1.x, v1.y, v1.z)

var m = v1.magnitude
var n = v1.normalized

print("magnitude = " + m)
print(n.x, n.y, n.z)
n = Vector3.Mul(2, n)
print("Mul", n.x, n.y, n.z)

var v2 = new Vector3(-1, 2, 3)
var t = 0
for (var t = 0; t < 1; t += 0.1) {
    var v3 = Vector3.Slerp(v1, v2, t)
    print("v3", v3.x, v3.y, v3.z)
}
