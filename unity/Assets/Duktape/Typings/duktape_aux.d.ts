
declare function dofile(filename: string): void
declare function dostring(source: string, filename?: string): void

declare namespace DuktapeJS {
    const COMPLETE: string
    const ERROR: string

    class Handler {
        constructor(caller: any, fn: Function, once: boolean)
        /**
         * 判断是否与指定的 caller, fn 组合等价, 不指定 fn 时, 只要 caller 相等即为等价
         */
        equals(caller: any, fn?: Function): boolean
        invoke(...args: any[]): any
    }

    class Dispatcher {
        constructor()

        /**
         * 添加监听
         * @param caller 回调函数执行时绑定的 this
         * @param fn 回调函数
         * @param once 是否单次出发, 默认 false
         */
        on(caller: any, fn: Function, once?: boolean): Dispatcher

        /**
         * 移除监听
         * @param caller 移除指定 caller 对应的回调
         * @param fn 移除指定回调, 不指定 fn 时, 移除所有 caller 注册的回调
         */
        off(caller: any, fn?: Function): void

        /**
         * 触发事件
         */
        dispatch(...args: any[]): any

        // 不要在 dispatch 过程中调用
        compact(): void

        clear(): void
    }

    class EventDispatcher {
        constructor()
        /**
         * 添加监听
         * @param caller 回调函数执行时绑定的 this
         * @param fn 回调函数
         * @param once 是否单次出发, 默认 false
         */
        on(type: string, caller: any, fn: Function, once?: boolean): Dispatcher

        /**
         * 移除监听
         * @param caller 移除指定 caller 对应的回调
         * @param fn 移除指定回调, 不指定 fn 时, 移除所有 caller 注册的回调
         */
        off(type: string, caller: any, fn?: Function): void

        /**
         * 触发事件
         */
        dispatch(type: string, ...args: any[]): any

        compact(): void

        clear(type: string): void
    }

    class WebSocket extends EventDispatcher {
        readonly connected: boolean
        constructor()

        connect(scheme: string, host: string, port: number, path: string)
        close()
        send(data: any): boolean
    }

    class Enum {
        static GetName(type: any, val: number): string
    }

    class Delegate {
        // not implemented
        static on<R>(caller: any, fn: () => R): Delegate
        static on<R, T0>(caller: any, fn: (arg0: T0) => R): Delegate
        static on<R, T0, T1>(caller: any, fn: (arg0: T0, arg1: T1) => R): Delegate
    }

    /*
    class Handler {
        caller: any
        method: Function
        args: any[]
        once: boolean
        run(): void
        runWith(...args: any[]): void
    }

    class Dispatcher {
        on(type: string, caller: any, listener: Function, ...args: any[]): Dispatcher
        once(type: string, caller: any, listener: Function, ...args: any[]): Dispatcher
        off(type: string, caller: any, listener: Function): Dispatcher
        offAll(type: string, caller: any): Dispatcher
        offAll(type: string): Dispatcher
        offAll(): Dispatcher
        event(type: string, ...args: any[]): boolean
    }

    class Socket extends Dispatcher {
        readonly connected: boolean

        constructor()
        connect(host: string, port: number): void
        close(): void
        send(data: any): void
    }

    class HttpRequest extends Dispatcher {
        send(url: string, data?: any, method?: string, type?: string, headers?: any[]): void
    }
    */
}

/**
 * polyfills for es5
 */
declare interface Object {
    /**
     * Sets the prototype of a specified object o to  object proto or null. Returns the object o.
     * @param o The object to change its prototype.
     * @param proto The value of the new prototype or null.
     */
    setPrototypeOf(o: any, proto: object | null): any
}
