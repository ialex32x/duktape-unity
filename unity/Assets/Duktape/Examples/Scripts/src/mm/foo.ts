export class FooBase {
    greet() {
        console.log(`hello, I am nobody`)
    }
}

export class Foo extends FooBase {
    nickname = "type-t"
    private name = "test"
    private age = 0

    constructor(age: number) {
        super()
        this.age = age
    }

    static foo() {
        console.log("static foo")
    }

    greet() {
        super.greet()
        console.log(`hello, I am ${this.name}, ${this.age}.`)
    }
}
