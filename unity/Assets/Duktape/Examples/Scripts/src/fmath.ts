
export function fmathTest() {
    let f1 = FMath.from_int(2);
    let f2 = FMath.from_int(5000);
    for (let i = 0; i < 5; i++) {
        f2 = FMath.div(f2, f1);
        console.log(FMath.to_number(f2), FMath.to_number(FMath.sin(f2)));
    }

    let seed_start = 0;
    let seed_end = seed_start + 5;
    for (let seed = seed_start; seed < seed_end; seed++) {
        let r = new FMath.Random(seed);
        let c = 0;
        let times = 100;
        for (let i = 0; i < times; i++) {
            // let v = r.range(0, full) / full;
            let v = r.value;
            if (v > 0.5) {
                c++;
            }
        }
        console.log("random:", c * 100 / times, "% with seed:", seed)
    }
}
