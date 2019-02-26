// julio 2019/2/27 6:17:53
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
}
