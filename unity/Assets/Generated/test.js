var Foo = /** @class */ (function () {
    function Foo(age) {
        this.nickname = "type-t";
        this.name = "test";
        this.age = 0;
        this.age = age;
    }
    Foo.prototype.greet = function () {
        console.log("hello, I am " + this.name + ", " + this.age + ".");
    };
    return Foo;
}());
var foo = new Foo(12);
foo.greet();
//# sourceMappingURL=test.js.map