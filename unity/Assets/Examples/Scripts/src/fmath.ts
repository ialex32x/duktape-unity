
let f1 = FMath.from_int(2)
let f2 = FMath.from_int(5000)
for (let i = 0; i < 5; i++) {
    f2 = FMath.div(f2, f1)
    console.log(FMath.to_number(f2), FMath.to_number(FMath.sin(f2)))
}
