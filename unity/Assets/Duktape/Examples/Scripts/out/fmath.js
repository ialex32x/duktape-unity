"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
function fmathTest() {
    var f1 = FMath.from_int(2);
    var f2 = FMath.from_int(5000);
    for (var i = 0; i < 5; i++) {
        f2 = FMath.div(f2, f1);
        console.log(FMath.to_number(f2), FMath.to_number(FMath.sin(f2)));
    }
    var seed_start = 0;
    var seed_end = seed_start + 5;
    for (var seed = seed_start; seed < seed_end; seed++) {
        var r = new FMath.Random(seed);
        var c = 0;
        var times = 100;
        for (var i = 0; i < times; i++) {
            // let v = r.range(0, full) / full;
            var v = r.value;
            if (v > 0.5) {
                c++;
            }
        }
        console.log("random:", c * 100 / times, "% with seed:", seed);
    }
}
exports.fmathTest = fmathTest;
//# sourceMappingURL=fmath.js.map