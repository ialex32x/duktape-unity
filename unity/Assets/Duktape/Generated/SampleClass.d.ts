// julio 2019/2/26 20:15:10
declare class SampleClass {
    constructor(name: string, ...additional: string[])
    SetEnum(sampleEnum: SampleEnum): boolean
    CheckingVA(...args: number[]): number
    CheckingVA2(b: number, ...args: number[]): number
    Sum(all: number[]): number
    readonly name: string
    readonly sampleEnum: SampleEnum
}
