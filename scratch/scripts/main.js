var ev = new DuktapeJS.EventDispatcher();
var JSO = /** @class */ (function () {
    function JSO(name) {
        this.name = name;
    }
    JSO.prototype.foo1 = function (phase) {
        console.log(phase, this.name, "foo1");
    };
    JSO.prototype.foo2 = function (phase) {
        console.log(phase, this.name, "foo2");
    };
    return JSO;
}());
var jso1 = new JSO("A");
var jso2 = new JSO("B");
ev.on("test", jso1, jso1.foo1);
ev.on("test", jso1, jso1.foo2);
ev.on("test", jso2, jso2.foo1);
ev.on("test", jso2, jso2.foo2);
ev.dispatch("test", "FIRST");
ev.off("test", jso1, jso1.foo1);
console.log(ev.events["test"].handlers);
ev.dispatch("test", "SECOND");
ev.off("test", jso2);
ev.dispatch("test", "THIRD");
