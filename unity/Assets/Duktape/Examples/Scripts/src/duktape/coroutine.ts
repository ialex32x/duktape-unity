
/**
 * https://github.com/svaarala/duktape/issues/1101
 */
export class Coroutine {
    private static _break = {};
    private _thr: Duktape.Thread = null;
    private _done = false;
    private _value: any = null;

    constructor(fn: Function) {
        this._thr = new Duktape.Thread(fn);
    }

    static yield(v?: any, f?: any): any {
        return Duktape.Thread.yield(v, f);
    }

    static break(): void {
        Duktape.Thread.yield(this._break);
    }

    next(v?: any): boolean {
        if (this._done) {
            return false;
        }
        this._value = Duktape.Thread.resume(this._thr, v);  // assuming no-throw behavior
        if (this._value === Coroutine._break) {
            this._done = true;
            this._thr = undefined;
            this._value = undefined;
            console.warn("thread break");
        } else {
            this._done = Duktape.info(this._thr).tstate === 5;
        }
        return !this._done;
    }

    close() {
        this._thr = undefined;
        this._value = undefined;
        this._done = true;
    }

    get value() {
        return this._value;
    }

    get done() {
        return this._done;
    }
}
