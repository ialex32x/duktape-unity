"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var Foo = /** @class */ (function () {
    function Foo(age) {
        this.nickname = "type-t";
        this.name = "test";
        this.age = 0;
        this.age = age;
    }
    Foo.foo = function () {
        console.log("static foo");
    };
    Foo.prototype.greet = function () {
        console.log("hello, I am " + this.name + ", " + this.age + ".");
    };
    return Foo;
}());
exports.Foo = Foo;
//# sourceMappingURL=foo.js.map