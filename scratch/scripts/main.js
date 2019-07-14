var start = Date.now()
var v = new DuktapeJS.Vector3(1, 2, 3);
for (var i = 0; i < 1; i++) {
    v.Normalize();
}
print("js/perf", (Date.now() - start) / 1000);
