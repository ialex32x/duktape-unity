"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var Time = UnityEngine.Time;
function promiseTest() {
    new Promise(function (resolve) {
        console.log("promise.resolve", Time.realtimeSinceStartup);
        setTimeout(function () {
            resolve(123);
        }, 1000);
    }).then(function (value) {
        console.log("promise.then", value, Time.realtimeSinceStartup);
    });
    console.log("timeout begin", Time.realtimeSinceStartup);
    setTimeout(function () {
        console.log("timeout 3s", Time.realtimeSinceStartup);
    }, 1000 * 3);
    console.log("interval begin", Time.realtimeSinceStartup);
    setInterval(function () {
        console.log("interval 15s", Time.realtimeSinceStartup);
    }, 1000 * 15);
}
exports.promiseTest = promiseTest;
//# sourceMappingURL=promise_test.js.map