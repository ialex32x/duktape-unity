
# 简介
在 unity 中集成和封装 duktape.
使你能动态执行 javascript. 
也可以使用 typescript 编写脚本, 提供完整的类型检查, 以及代码提示.


# 特性支持 (已实现)
* 支持 nodejs 风格的模块
* 生成 C# to js 静态绑定, 自动生成对应 d.ts 声明 (复杂类型/泛型/函数重载/delegate相关部分未完成)
* setTimeout/setInterval/clearTimeout/clearInterval 兼容

# 特性支持 (未实现)
* delegate 操作接口 (+=, -=, 以及清空)
* 针对Vector3等常用值类型的绑定优化
* 支持在脚本层面扩展 MonoBehaviour
* 基本的 eventloop 支持
* Android/iOS 支持 (热更)
* socket (tcp/udp)
* websocket ()
* 使用 protobufjs
* enable debugger support (vscode)

# 依赖环境
使用 typescript 编写脚本时, 需要安装 typescript
```shell
npm install -g typescript
```

# Example

```ts

// 导入模块
import { B } from "base/b"
// 相对路径导入模块
import { C } from "./base/c"

// 扩展 MonoBehaviour (not implemented)
export class MyPlayer extends MonoBehaviour {
    Start() {
        console.log("MyPlayer.Start")
    }

    Jump() {

    }
}

export class A {
    private go: GameObject
    constructor () {
        this.go = new GameObject("test go")
        this.go.transform.localPosition = new Vector3(1, 2, 3) // (not implemented)
        this.go.AddComponent(MyPlayer) // (not implemented)

        let f = new Custom()
        // delegate 操作 (not implemented)
        f.onload = DuktapeJS.on(this, this.onload)  // 添加监听
        f.onload = DuktapeJS.off(this, this.onload) // 移除监听
        f.onload = DuktapeJS.off(this)              // 清空监听
    }

    private onload() {
        let timer1 = setInterval(() => {
            console.log("interval")
        }, 1000)

        setTimeout((a, b) => {
            console.log("timeout", a, b)
            clearInterval(timer1 )
        }, 5000, "Arg1", 123)
    }

    test() {
        let player = this.go.GetComponent(MyPlayer)
        player.Jump()
    }

    square() {
        console.log("A.square")
    }
}

```

# 使用及参考的库

* [duktape](https://github.com/svaarala/duktape)
* [slua](https://github.com/pangweiwei/slua)
* [xLua](https://github.com/Tencent/xLua)
* [typescript-for-unity](https://github.com/SpiralP/typescript-for-unity)

# 其他

* [vscode-duktape-debug](https://github.com/harold-b/vscode-duktape-debug)
