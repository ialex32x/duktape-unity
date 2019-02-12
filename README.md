
# 简介
在 unity 中集成和封装 duktape.
使你能动态执行 javascript. 
可以使用 typescript 编写脚本, 提供完整的类型检查, 以及代码提示.

# 目标特性 (未实现)
* 生成 C# to js 静态绑定, 自动生成对应 d.ts 声明 
* 支持 nodejs 风格的模块 (简单实现)

# Example
base/a.ts
```ts

export class A {
    private go: GameObject
    constructor () {
        this.go = new GameObject("test go")
        this.go.transform.localPosition = new Vector3(1, 2, 3)
    }

    square() {
        console.log("A.square")
    }
}

```

base/b.ts
```ts
import { A } from "./a"

export class B extends A {

    static foo() {

    }

    square() {
        super.square()
        console.log("A.square")
    }
}

```

main.ts
```ts
import { B } from "base/b"

B.foo()
let b = new B()
b.square()

```
