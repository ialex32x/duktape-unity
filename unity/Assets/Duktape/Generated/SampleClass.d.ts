// julio 2019/2/25 22:35:44
declare class SampleClass {
    constructor(name: string, ...additional: string[])
    SetEnum(sampleEnum: SampleEnum): boolean
    CheckingVA(...args: number[]): number
    CheckingVA2(b: number, ...args: number[]): number
    Sum(all: number[]): number
    readonly name: string
    readonly sampleEnum: SampleEnum
}
