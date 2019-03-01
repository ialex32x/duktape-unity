"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
function Vector3(x, y, z) {
    this.push(x);
    this.push(y);
    this.push(z);
}
exports.Vector3 = Vector3;
Vector3.prototype = Object.defineProperties(Object.setPrototypeOf({}, Array).prototype, {
    "normalized": {
        get: function () {
            var rlen = 1 / Math.sqrt(this[0] * this[0] + this[1] * this[1] + this[2] * this[2]);
            return new Vector3(this[0] * rlen, this[1] * rlen, this[2] * rlen);
        }
    },
    "x": {
        get: function () {
            return this[0];
        },
        set: function (v) {
            this[0] = v;
        }
    },
    "y": {
        get: function () {
            return this[1];
        },
        set: function (v) {
            this[1] = v;
        }
    },
    "z": {
        get: function () {
            return this[2];
        },
        set: function (v) {
            this[2] = v;
        }
    }
});
//# sourceMappingURL=vector3.js.map