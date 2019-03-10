
var v1 = new DuktapeJS.Vector3(1, 2, 3)

print(v1[0], v1[1], v1[2])
print(v1.x, v1.y, v1.z)

var m = v1.magnitude
var n = v1.normalized

print("magnitude = " + m)
print(n.x, n.y, n.z)
print(n.x * m, n.y * m, n.z * m)
