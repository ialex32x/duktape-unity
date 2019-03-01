declare class SampleClass {
    /**
     * 简单构造函数测试
     * @param name 测试字符串
     * @param additional 测试可变参数
     */
    constructor(name: string, ...additional: string[])
    TestDelegate1(): void
    SetEnum(sampleEnum: SampleEnum): boolean
    CheckingVA(...args: number[]): number
    CheckingVA2(b: number, ...args: number[]): number
    Sum(all: number[]): number
    readonly name: string
    readonly sampleEnum: SampleEnum
    delegateFoo1: Delegate2<void, string, string>
    delegateFoo2: Delegate2<void, string, string>
    delegateFoo4: Delegate2<number, number, number>
    action1: Delegate0<void>
    action2: Delegate1<void, string>
    actions1: Delegate0<void>[]
}
