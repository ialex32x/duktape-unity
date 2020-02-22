"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var MyClass = /** @class */ (function () {
    function MyClass() {
        this.x = 0;
    }
    MyClass.prototype.update = function () {
        // 尝试重现调试器断点bug
        this.x += 1;
    };
    return MyClass;
}());
exports.MyClass = MyClass;
//# sourceMappingURL=my_class.js.map