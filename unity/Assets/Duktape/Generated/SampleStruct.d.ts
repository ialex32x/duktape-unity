// julio 2019/2/26 20:15:10
declare class SampleStruct {
    constructor()
    ChangeFieldA(a: number): void
    static StaticMethodWithReturnAndNoOverride(a: any, /*ref*/ b: any, /*out*/ c: any): {
        ret: string, 
        b: any, 
        c: any, 
    }
    readonly readonly_property_c: number
    readwrite_property_d: number
    static_readwrite_property_d: number
    field_a: number
    static static_field_b: string
}
