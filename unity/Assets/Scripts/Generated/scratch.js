// 只是草稿
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
var Handler = /** @class */ (function () {
    function Handler(caller, fn, once) {
        this.caller = caller;
        this.fn = fn;
        this.once = once;
    }
    Handler.prototype.equals = function (caller, fn) {
        return caller == this.caller && (fn == null || fn == this.fn);
    };
    Handler.prototype.invoke = function () {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i] = arguments[_i];
        }
        var _a;
        return (_a = this.fn).call.apply(_a, [this.caller].concat(args));
    };
    return Handler;
}());
var DelegateBase = /** @class */ (function () {
    function DelegateBase() {
        this._handlers = new Array();
        this._handlers.push(0);
    }
    DelegateBase.prototype._dispatch = function () {
        var args = [];
        for (var _i = 0; _i < arguments.length; _i++) {
            args[_i] = arguments[_i];
        }
        var ret;
        for (var i = 0; i < this._handlers.length; i++) {
            var el = this._handlers[i];
            if (typeof el != "number") {
                ret = el.invoke.apply(el, args);
                if (el.once) {
                    this._handlers[i] = this._handlers[0];
                    this._handlers[0] = i;
                }
            }
        }
        return ret;
    };
    // 不要在 dispatch 过程中调用
    DelegateBase.prototype.free = function () {
        this.clear();
        this._handlers.splice(1, this._handlers.length - 1);
        this._handlers[0] = 0;
    };
    DelegateBase.prototype.clear = function () {
        for (var i = 1; i < this._handlers.length; i++) {
            var el = this._handlers[i];
            if (typeof el != "number") {
                this._handlers[i] = this._handlers[0];
                this._handlers[0] = i;
            }
        }
    };
    DelegateBase.prototype._on = function (caller, fn, once) {
        if (once === void 0) { once = false; }
        if (fn == null) {
            return false;
        }
        var freeslot = this._handlers[0];
        var handler = new Handler(caller, fn, once);
        if (freeslot == 0) {
            this._handlers.push(handler);
        }
        else {
            this._handlers[0] = this._handlers[freeslot];
            this._handlers[freeslot] = handler;
        }
        return true;
    };
    DelegateBase.prototype.off = function (caller, fn) {
        for (var i = 0; i < this._handlers.length;) {
            var el = this._handlers[i];
            if (typeof el != "number" && el.equals(caller, fn)) {
                this._handlers[i] = this._handlers[0];
                this._handlers[0] = i;
            }
            else {
                i++;
            }
        }
    };
    return DelegateBase;
}());
var Delegate0 = /** @class */ (function (_super) {
    __extends(Delegate0, _super);
    function Delegate0() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Delegate0.prototype.on = function (caller, fn) {
        _super.prototype._on.call(this, caller, fn);
    };
    Delegate0.prototype.once = function (caller, fn) {
        _super.prototype._on.call(this, caller, fn, true);
    };
    Delegate0.prototype.dispatch = function () {
        return this._dispatch();
    };
    return Delegate0;
}(DelegateBase));
var Delegate1 = /** @class */ (function (_super) {
    __extends(Delegate1, _super);
    function Delegate1() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Delegate1.prototype.on = function (caller, fn) {
        _super.prototype._on.call(this, caller, fn);
    };
    Delegate1.prototype.once = function (caller, fn) {
        _super.prototype._on.call(this, caller, fn, true);
    };
    Delegate1.prototype.dispatch = function (arg1) {
        return this._dispatch(arg1);
    };
    return Delegate1;
}(DelegateBase));
var Delegate2 = /** @class */ (function (_super) {
    __extends(Delegate2, _super);
    function Delegate2() {
        return _super !== null && _super.apply(this, arguments) || this;
    }
    Delegate2.prototype.on = function (caller, fn) {
        _super.prototype._on.call(this, caller, fn);
    };
    Delegate2.prototype.once = function (caller, fn) {
        _super.prototype._on.call(this, caller, fn, true);
    };
    Delegate2.prototype.dispatch = function (arg1, arg2) {
        return this._dispatch(arg1, arg2);
    };
    return Delegate2;
}(DelegateBase));
var Sample = /** @class */ (function () {
    function Sample() {
        this.onload = new Delegate1();
    }
    return Sample;
}());
var caller1 = {};
var caller2 = {};
var sample = new Sample();
var fn1 = function (arg) {
    console.log("caller1.callback 1", arg);
};
sample.onload.on(caller1, fn1);
sample.onload.on(caller1, function (arg) {
    console.log("caller1.callback 2", arg);
});
sample.onload.once(caller1, function (arg) {
    console.log("caller1.callback 3 (once)", arg);
});
sample.onload.on(caller2, function (arg) {
    console.log("caller2.callback 4", arg);
});
console.log("#1");
sample.onload.dispatch("testing");
sample.onload.off(caller1, fn1);
console.log("#2");
sample.onload.dispatch("testing");
console.log("#3");
sample.onload.dispatch("testing");
sample.onload.off(caller1);
console.log("#4");
sample.onload.dispatch("testing");
sample.onload.clear();
console.log("#5");
sample.onload.dispatch("testing");
//# sourceMappingURL=scratch.js.map