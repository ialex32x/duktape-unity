
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
* websocket

# 特性支持 (未实现)
* 支持在脚本层面扩展 MonoBehaviour 
* 基本的 eventloop 支持
* Android/iOS 支持 (热更)
* socket (tcp/udp)
* enable debugger support (vscode)

# 依赖环境
使用 typescript 编写脚本时, 需要安装 typescript
```shell
npm install -g typescript
```

如果重新生成 duktape 源代码, 需要安装 python, pip, 以及 pyyaml
```shell
pip install pyyaml

# duktape-2.3.0/src-input: duktape source code
# duktape-2.3.0/src-custom: combined duktape source code
./configure_duktape.bat 
```

# Example

```ts

// 导入模块
import { B } from "base/b"
// 相对路径导入模块
import { C } from "./base/c"

class MyPlayer {
    Start() {
        console.log("MyPlayer.Start")
        this.Jump()
    }

    // Update() {
    // }

    Jump() {
        console.log("MyPlayer.Jump")
    }
}

export class A {
    private go: GameObject
    constructor () {
        this.go = new GameObject("test go")
        this.go.transform.localPosition = new Vector3(1, 2, 3) // (not implemented)
        // 可以使用 Bridge 将 Enable/Disable/Update 等 Unity 流程引入脚本控制
        // 不建议大量使用, 可以作为一个入口, 然后由脚本进行更复杂的逻辑
        this.go.AddComponent(DuktapeJS.Bridge).SetBridge(new MyPlayer()) 

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

    square() {
        console.log("A.square")
    }
}

```

# 状态 
开发中, 修改调整幅度比较大, 暂不能用于生产环境.  <br/>
Vector2/Matrix3x3/Matrix4x4/Quaternion 值类型优化基本写完, 还没测试正确性/差异性. <br/>
###### 由于 C# 中有些方法重载或其他定义在 d.ts 中无法产生直接对应的申明, 目前 d.ts 文件在 tsc 时会报错, 但不会实际影响脚本编译. 后续再考虑解决此问题.

# 使用方法

Unity 执行菜单项 Duktape/Generate Bindings 可以生成静态绑定代码.


## 定制导出类型

* 通过配置
创建/修改配置文件 ProjectSettings\duktape.json (具体可配置项参考 Assets\Duktape\Editor\Prefs.cs)
```json
{
    "outDir": "Assets/Source/Generated",
    "implicitAssemblies": [
        "UnityEngine.CoreModule"
    ], 
    "explicitAssemblies": [
        "Assembly-CSharp"
    ],
    "tab": "    "
}
```
* 通过实现 Duktape.IBindingProcess 接口或直接继承 AbstractBindingProcess 类
```c#
public class MyCustomBinding : AbstractBindingProcess
{
    public override void OnPreCollectTypes(BindingManager bindingManager)
    {
        // 添加导出
        // bindingManager.AddExport(typeof(MyCustomClass));
    }

    public override void OnCleanup(BindingManager bindingManager)
    {
        Debug.Log($"finish @ {DateTime.Now}");
    }
}
```

## 编写脚本
基本用法可以参考 Assets/Scenes/main.unity (Sample.cs) <br/>
使用 javascript 直接开发不需要额外配置, 如果使用 typescript 开发需要配置 tsconfig.json.  <br/>
将自动生成的绑定代码目录配置到 typeRoots 中即可使用完整的代码提示. <br/>
另外可以考虑开启 watch 自动编译 typescript. <br/>

(后续会详细补充 protobufjs 等的使用方法)<br/>

# 使用及参考的库

* [duktape](https://github.com/svaarala/duktape)
* [slua](https://github.com/pangweiwei/slua)
* [xLua](https://github.com/Tencent/xLua)
* [typescript-for-unity](https://github.com/SpiralP/typescript-for-unity)

# 其他

* [vscode-duktape-debug](https://github.com/harold-b/vscode-duktape-debug)
* [duktape-doc-debugger](https://github.com/svaarala/duktape/blob/master/doc/debugger.rst)

