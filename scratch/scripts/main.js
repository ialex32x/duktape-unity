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
