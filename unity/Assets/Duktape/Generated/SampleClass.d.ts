declare class SampleClass {
    /**
     * 简单构造函数测试
     * @param name 测试字符串
     * @param additional 测试可变参数
     */
    constructor(name: string, ...additional: string[])
    SetEnum(sampleEnum: SampleEnum): boolean
    CheckingVA(...args: number[]): number
    CheckingVA2(b: number, ...args: number[]): number
    Sum(all: number[]): number
    readonly name: string
    readonly sampleEnum: SampleEnum
    delegateFoo1: any
    delegateFoo2: any
    delegateFoo4: any
}
