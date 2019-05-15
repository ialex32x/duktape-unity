"use strict";
var __extends = (this && this.__extends) || (function () {
    var extendStatics = function (d, b) {
        extendStatics = Object.setPrototypeOf ||
            ({ __proto__: [] } instanceof Array && function (d, b) { d.__proto__ = b; }) ||
            function (d, b) { for (var p in b) if (b.hasOwnProperty(p)) d[p] = b[p]; };
        return extendStatics(d, b);
    };
    return function (d, b) {
        extendStatics(d, b);
        function __() { this.constructor = d; }
        d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
var FooBase = /** @class */ (function () {
    function FooBase() {
    }
    FooBase.prototype.greet = function () {
        console.log("hello, I am nobody");
    };
    return FooBase;
}());
exports.FooBase = FooBase;
var Foo = /** @class */ (function (_super) {
    __extends(Foo, _super);
    function Foo(age) {
        var _this = _super.call(this) || this;
        _this.nickname = "type-t";
        _this.name = "test";
        _this.age = 0;
        _this.age = age;
        return _this;
    }
    Foo.foo = function () {
        console.log("static foo");
    };
    Foo.prototype.greet = function () {
        _super.prototype.greet.call(this);
        console.log("hello, I am " + this.name + ", " + this.age + ".");
    };
    return Foo;
}(FooBase));
exports.Foo = Foo;
//# sourceMappingURL=foo.js.map