    
export class Foo {
    nickname = "type-t"
    private name = "test"
    private age = 0 

    constructor (age: number) {
        this.age = age
    }

    static foo() {
        console.log("static foo")
    }

    greet() {
        console.log(`hello, I am ${this.name}, ${this.age}.`)
    }
}

