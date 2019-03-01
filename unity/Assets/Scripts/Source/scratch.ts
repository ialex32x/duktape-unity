
// 只是草稿

class Handler {
    private caller: any
    private fn: Function
    private once: boolean

    constructor(caller: any, fn: Function, once: boolean) {
        this.caller = caller
        this.fn = fn
        this.once = once
    }

    equals(caller: any, fn?: Function) {
        return caller == this.caller && (fn == null || fn == this.fn)
    }

    invoke(...args: any[]) {
        return this.fn.call(this.caller, ...args)
    }
}

class DelegateBase {
    private _handlers = new Array()

    constructor() {
        this._handlers.push(0)
    }

    _dispatch(...args: any[]) {
        let ret
        for (let i = 0; i < this._handlers.length; i++) {
            let el = this._handlers[i]
            if (typeof el != "number") {
                ret = el.invoke(...args)
                if (el.once) {
                    this._handlers[i] = this._handlers[0]
                    this._handlers[0] = i
                }
            }
        }
        return ret
    }

    // 不要在 dispatch 过程中调用
    free() {
        this.clear()
        this._handlers.splice(1, this._handlers.length - 1)
        this._handlers[0] = 0
    }

    clear() {
        for (let i = 1; i < this._handlers.length; i++) {
            let el = this._handlers[i]
            if (typeof el != "number") {
                this._handlers[i] = this._handlers[0]
                this._handlers[0] = i
            }
        }
    }

    protected _on(caller: any, fn: Function, once: boolean = false) {
        if (fn == null) {
            return false
        }
        let freeslot = this._handlers[0]
        let handler = new Handler(caller, fn, once)
        if (freeslot == 0) {
            this._handlers.push(handler)
        } else {
            this._handlers[0] = this._handlers[freeslot]
            this._handlers[freeslot] = handler
        }
        return true
    }

    off(caller: any, fn?: Function) {
        for (let i = 0; i < this._handlers.length;) {
            let el = this._handlers[i]
            if (typeof el != "number" && el.equals(caller, fn)) {
                this._handlers[i] = this._handlers[0]
                this._handlers[0] = i
            } else {
                i++
            }
        }
    }
}

class Delegate0<R> extends DelegateBase {
    on(caller: any, fn: () => R): void {
        super._on(caller, fn)
    }

    once(caller: any, fn: () => R): void {
        super._on(caller, fn, true)
    }

    dispatch(): R {
        return this._dispatch()
    }
}

class Delegate1<R, T1> extends DelegateBase {
    on(caller: any, fn: (arg1: T1) => R): void {
        super._on(caller, fn)
    }

    once(caller: any, fn: (arg1: T1) => R): void {
        super._on(caller, fn, true)
    }

    dispatch(arg1: T1): R {
        return this._dispatch(arg1)
    }
}

class Delegate2<R, T1, T2> extends DelegateBase {
    on(caller: any, fn: (arg1: T1, arg2: T2) => R): void {
        super._on(caller, fn)
    }

    once(caller: any, fn: (arg1: T1, arg2: T2) => R): void {
        super._on(caller, fn, true)
    }

    dispatch(arg1: T1, arg2: T2): R {
        return this._dispatch(arg1, arg2)
    }
}

// class Sample {
//     onload: Delegate1<void, string>

//     constructor() {
//         this.onload = new Delegate1()
//     }
// }

// let caller1 = {}
// let caller2 = {}
// let sample = new Sample()
// let fn1 = arg => {
//     console.log("caller1.callback 1", arg)
// }
// sample.onload.on(caller1, fn1)
// sample.onload.on(caller1, arg => {
//     console.log("caller1.callback 2", arg)
// })
// sample.onload.once(caller1, arg => {
//     console.log("caller1.callback 3 (once)", arg)
// })
// sample.onload.on(caller2, arg => {
//     console.log("caller2.callback 4", arg)
// })

// console.log("#1")
// sample.onload.dispatch("testing")
// sample.onload.off(caller1, fn1)
// console.log("#2")
// sample.onload.dispatch("testing")
// console.log("#3")
// sample.onload.dispatch("testing")
// sample.onload.off(caller1)
// console.log("#4")
// sample.onload.dispatch("testing")
// sample.onload.clear()
// console.log("#5")
// sample.onload.dispatch("testing")


