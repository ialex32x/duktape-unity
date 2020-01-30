
type DUKTAPE_ENCODING = "hex" | "base64" | "jx" | "jc";

declare function print(...args: any[]): void;

declare namespace Duktape {
    /**	Duktape version number: (major * 10000) + (minor * 100) + patch. */
    const version: number;
    /**	Cryptic, version dependent summary of most important effective options like endianness and architecture. */
    const env: string;

    /**	Set or get finalizer of an object. */
    function fin(o: any, fn?: Function): Function | undefined;

    /**	Encode a value (hex, base-64, JX, JC): Duktape.enc('hex', 'foo'). */
    function enc(tp: DUKTAPE_ENCODING, o: any, replacer?: string, space?: number): string;

    /**	Decode a value (hex, base-64, JX, JC): Duktape.dec('base64', 'Zm9v'). */
    function dec(tp: DUKTAPE_ENCODING, o: string): any;

    /**	Get internal information (such as heap address and alloc size) of a value in a version specific format. The C API equivalent is duk_inspect_value(). */
    function info(o: any): any;

    /**	Get information about call stack entry. */
    function act(depth: number): any;

    /**	Trigger mark-and-sweep garbage collection. */
    function gc(flags?: number): void;

    /**	Compact the memory allocated for a value (object). */
    function compact(o: any): any;

    /**	Callback to modify/replace a created error. */
    function errCreate(e: any): any;

    /**	Callback to modify/replace an error about to be thrown. */
    function errThrow(e: any): any;

    /**	Pointer constructor (function). */
    class Pointer {
        constructor(o: any);
        toString(): string;
        valueOf(): any;
    }

    /** Duktape Thread */
    class Thread {
        /**	Thread constructor (function). */
        constructor(fn: Function);
        static resume(thread: Thread, v?: any, f?: any);
        static yield(v?: any, f?: any);
        static current(): Thread;
    }

}

/** available in duktape-2.5.0 */
declare class CBOR {
    static encode(o: any): any;
    static decode(o: any): any;
}

// declare class TextEncoder {
//     constructor();
// }

// declare class TextDecoder {
//     constructor();
// }
