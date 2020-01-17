
import Time = UnityEngine.Time;

export function promiseTest() {
    new Promise((resolve: (value: any) => void) => {
        console.log("promise.resolve", Time.realtimeSinceStartup);
        setTimeout(() => {
            resolve(123);
        }, 1000);
    }).then((value: any) => {
        console.log("promise.then", value, Time.realtimeSinceStartup);
    });
    console.log("timeout begin", Time.realtimeSinceStartup)
    setTimeout(() => {
        console.log("timeout 3s", Time.realtimeSinceStartup)
    }, 1000 * 3);

    console.log("interval begin", Time.realtimeSinceStartup)
    setInterval(() => {
        console.log("interval 15s", Time.realtimeSinceStartup)
    }, 1000 * 15);
}