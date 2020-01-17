"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/**
 * https://github.com/svaarala/duktape/issues/1101
 */
var Coroutine = /** @class */ (function () {
    function Coroutine(fn) {
        this._thr = null;
        this._done = false;
        this._value = null;
        this._thr = new Duktape.Thread(fn);
    }
    Coroutine.yield = function (v, f) {
        return Duktape.Thread.yield(v, f);
    };
    Coroutine.break = function () {
        Duktape.Thread.yield(this._break);
    };
    Coroutine.prototype.next = function (v) {
        if (this._done) {
            return false;
        }
        this._value = Duktape.Thread.resume(this._thr, v); // assuming no-throw behavior
        if (this._value === Coroutine._break) {
            this._done = true;
            this._thr = undefined;
            this._value = undefined;
            console.warn("thread break");
        }
        else {
            this._done = Duktape.info(this._thr).tstate === 5;
        }
        return !this._done;
    };
    Coroutine.prototype.close = function () {
        this._thr = undefined;
        this._value = undefined;
        this._done = true;
    };
    Object.defineProperty(Coroutine.prototype, "value", {
        get: function () {
            return this._value;
        },
        enumerable: true,
        configurable: true
    });
    Object.defineProperty(Coroutine.prototype, "done", {
        get: function () {
            return this._done;
        },
        enumerable: true,
        configurable: true
    });
    Coroutine._break = {};
    return Coroutine;
}());
exports.Coroutine = Coroutine;
//# sourceMappingURL=coroutine.js.map