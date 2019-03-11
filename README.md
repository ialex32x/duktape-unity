
# 简介
在 unity 中集成和封装 duktape.
使你能动态执行 javascript. 
也可以使用 typescript 编写脚本, 提供完整的类型检查, 以及代码提示.


# 特性支持 (已实现)
* 支持 nodejs 风格的模块
* 生成 C# to js 静态绑定, 自动生成对应 d.ts 声明 
* setTimeout/setInterval/clearTimeout/clearInterval 兼容
* delegate 操作接口 
* 针对Vector3等常用值类型的绑定优化 (待细化)
* 可使用 protobufjs

# 特性支持 (未实现)
* 支持在脚本层面扩展 MonoBehaviour
* 基本的 eventloop 支持
* Android/iOS 支持 (热更)
* socket (tcp/udp)
* websocket ()
* enable debugger support (vscode)

# 依赖环境
使用 typescript 编写脚本时, 需要安装 typescript
```shell
npm install -g typescript
```

如果重新生成 duktape 源代码, 需要安装 python, pip, 以及 pyyaml
```shell
pip install pyyaml
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
        this.go.AddComponent(MyPlayer) 

        let f = new Custom()
        // delegate 操作 
        f.onload = new DuktapeJS.Dispatcher() // 如果脚本需要注册多个监听, 用 Dispatcher
        // f.onload = () => { ... }              // 也可以直接注册函数 (会覆盖原值)
        f.onload.on(this, this.onload)  // 添加this.onload监听
        f.onload.off(this, this.onload) // 移除this.onload监听
        f.onload.off(this)              // 清空this监听
        f.onload.clear()                // 清空所有监听

        // out 取参
        let v = {}
        if (System.Int32.TryParse("123", v)) {
            console.log(v.target)
        }
    }

    private onload() {
        let timer1 = setInterval(() => {
            console.log("interval")
        }, 1000)

        setTimeout((a, b) => {
            console.log("timeout", a, b)
            clearInterval(timer1)
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
* [duktape-doc-debugger](https://github.com/svaarala/duktape/blob/master/doc/debugger.rst)

