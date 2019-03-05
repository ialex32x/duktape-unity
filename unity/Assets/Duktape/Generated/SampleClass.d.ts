declare class SampleClass {
    /**
     * 简单构造函数测试
     * @param name 测试字符串
     * @param additional 测试可变参数
     */
    constructor(name: string, ...additional: string[])    TestDelegate1(): void
    TestVector3(v: any): void
    TestType1(type: any): any
    SetEnum(sampleEnum: SampleEnum): boolean
    CheckingVA(...args: number[]): number
    CheckingVA2(b: number, ...args: number[]): number
    MethodOverride(x: number, y: number): void
    MethodOverride(x: number): void
    MethodOverride(x: string): void
    MethodOverride(): void
    MethodOverride2(x: number): void
    MethodOverride2F(x: number): void
    MethodOverride3(x: number, y: number, z: number, args: any): void
    MethodOverride3(x: number, y: number, z: number): void
    MethodOverride3(x: number, y: number, z: number, ...args: number[]): void
    MethodOverride3(x: number, y: number): void
    MethodOverride3(x: number, y: number, ...args: number[]): void
    MethodOverride3(x: number): void
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
