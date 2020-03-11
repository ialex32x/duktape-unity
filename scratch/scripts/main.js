var start = performance.now()
var v = new DuktapeJS.Vector3(1, 2, 3);
for (var i = 0; i < 10; i++) {
    v.Normalize();
}
print("js/perf", (performance.now() - start) / 1000);

function foo() {
    print("js/foo");
}

(function () {
    print("in anonymous function");
    print("here?");
    foo();
})();

var buffer = new Buffer(1024);
var sock = new Socket(1, 0);
var count = 0;
sock.connect("localhost", 1234);
print("buffer.length:", buffer.length);
while (true) {
    count++;
    sock.send("test" + count);
    var recv_size = sock.recv(buffer, 0, buffer.length);
    if (recv_size > 0) {
        print("echo", buffer.toString("utf8", 0, recv_size));
    }
    sleep(1000);
}