var start = performance.now()
var v = new DuktapeJS.Vector3(1, 2, 3);
for (var i = 0; i < 10; i++) {
    v.Normalize();
}
print("js/perf", (performance.now() - start) / 1000);

var Quaternion = DuktapeJS.Quaternion;
var q1 = Quaternion.Euler(30, 0, 0);
var q2 = Quaternion.Euler(60, 0, 0);

print("angle", Quaternion.Angle(q1, q2));
print("slerp", Quaternion.Slerp(q1, q2, 0.5));
print("lerp", Quaternion.Lerp(q1, q2, 0.5));

function foo() {
    print("js/foo");
}

(function () {
    print("in anonymous function");
    print("here?");
    foo();
})();

// var buffer = new Buffer(1024);
// var sock = new DuktapeJS.Socket(1, 0);
// var count = 0;
// sock.connect("localhost", 1234);
// print("buffer.length:", buffer.length);
// while (true) {
//     count++;
//     sock.send("test" + count);
//     var recv_size = sock.recv(buffer, 0, buffer.length);
//     if (recv_size > 0) {
//         print("echo", buffer.toString("utf8", 0, recv_size));
//     }
//     sleep(1000);
// }