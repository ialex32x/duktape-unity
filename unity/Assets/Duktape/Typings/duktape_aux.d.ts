
declare function dofile(filename: string): void
declare function dostring(source: string, filename?: string): void

declare namespace DuktapeJS {
    class Enum {
        static GetName(type: any, val: number): string
    }
}