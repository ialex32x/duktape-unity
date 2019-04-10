var ws = new DuktapeJS.WebSocket()
var tick = 0
var pc = 0

ws.on("open", this, function () {
    print("open")
})

ws.on("close", this, function () {
    print("close")
})

ws.on("data", this, function (data) {
    print("receiving", data)
})

ws.connect("ws://127.0.0.1:8080/websocket")

while (true) {
    ws.poll()
    DuktapeJS.sleep(1)
    tick += 1
    if (tick > 3) {
        tick = 0
        pc += 1
        ws.send("test message #" + pc)
    }
}

// var Vector3 = DuktapeJS.Vector3
// var v1 = new Vector3(1, 2, 3)

// print(v1[0], v1[1], v1[2])
// print(v1.x, v1.y, v1.z)

// var m = v1.magnitude
// var n = v1.normalized

// print("magnitude = " + m)
// print(n.x, n.y, n.z)
// n = Vector3.Mul(2, n)
// print("Mul", n.x, n.y, n.z)

// var v2 = new Vector3(-1, 2, 3)
// var t = 0
// for (var t = 0; t < 1; t += 0.1) {
//     var v3 = Vector3.Slerp(v1, v2, t)
//     print("v3", v3.x, v3.y, v3.z)
// }

// print("gettime", DuktapeJS.gettime())
// print("sleep begin")
// DuktapeJS.sleep(3)
// print("sleep end")
