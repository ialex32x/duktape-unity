var ev = new DuktapeJS.EventDispatcher();
var JSO = (function () {
    function JSO(name) {
        this.name = name;
    }
    JSO.prototype.foo1 = function (phase) {
        print(phase, this.name, "foo1");
    };
    JSO.prototype.foo2 = function (phase) {
        print(phase, this.name, "foo2");
    };
    JSO.prototype.foo3 = function (phase) {
        print(phase, this.name, "foo3");
    };
    return JSO;
}());
function inspect(o) {
    print(o.length, o)
    for (var i = 0; i < o.length; i++) {
        print("index:", i, "value:", o[i])
    }
}
var jso1 = new JSO("A");
var jso2 = new JSO("B");
ev.on("test", jso1, jso1.foo1);
ev.on("test", jso2, jso2.foo1);
ev.on("test", jso1, jso1.foo2);
ev.on("test", jso1, jso1.foo3);
print(ev.events["test"] === ev.events["test"])
// ev.events["test"].off(jso1)
ev.off("test", jso1, jso1.foo2)
// inspect(ev.events["test"].handlers)
ev.dispatch("test", "DISPATCH #1")

ev.off("test", jso1)
ev.on("test", jso1, jso1.foo2);
ev.on("test", jso1, jso1.foo3);
// inspect(ev.events["test"].handlers)
ev.dispatch("test", "DISPATCH #2")

ev.clear("test")
ev.dispatch("test", "DISPATCH #3")

