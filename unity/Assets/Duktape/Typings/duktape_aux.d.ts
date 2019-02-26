
declare function dofile(filename: string): void
declare function dostring(source: string, filename?: string): void

declare namespace DuktapeJS {
    const COMPLETE: string
    const ERROR: string

    class Enum {
        static GetName(type: any, val: number): string
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
