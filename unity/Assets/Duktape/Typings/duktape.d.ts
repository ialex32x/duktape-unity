
type DUKTAPE_ENCODING = "hex" | "base64" | "jx" | "jc"

declare function print(...args: any[]): void

declare class Duktape {
    /**	Duktape version number: (major * 10000) + (minor * 100) + patch. */
    static version: number
    /**	Cryptic, version dependent summary of most important effective options like endianness and architecture. */
    static env: string

    /**	Set or get finalizer of an object. */
    static fin: (o: any, fn?: Function) => Function | undefined
    /**	Encode a value (hex, base-64, JX, JC): Duktape.enc('hex', 'foo'). */
    static enc: (tp: DUKTAPE_ENCODING, o: any, replacer?: string, space?: number) => string
    /**	Decode a value (hex, base-64, JX, JC): Duktape.dec('base64', 'Zm9v'). */
    static dec: (tp: DUKTAPE_ENCODING, o: string) => any
    /**	Get internal information (such as heap address and alloc size) of a value in a version specific format. The C API equivalent is duk_inspect_value(). */
    static info: (o: any) => any
    /**	Get information about call stack entry. */
    static act: (depth: number) => any
    /**	Trigger mark-and-sweep garbage collection. */
    static gc: (flags?: number) => void
    /**	Compact the memory allocated for a value (object). */
    static compact: (o: any) => any
    /**	Callback to modify/replace a created error. */
    static errCreate: (e: any) => any
    /**	Callback to modify/replace an error about to be thrown. */
    static errThrow: (e: any) => any
    /**	Pointer constructor (function). */
    static Pointer: any
    /**	Thread constructor (function). */
    static Thread: any
}
