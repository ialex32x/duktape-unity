![logo](res/logo.png "duktape-unity")

# 简介
将[duktape](https://github.com/svaarala/duktape)集成到Unity中, 使您可以在运行时动态加载执行javascript脚本. <br/>
另外也提供了对typescript的集成支持, 建议使用typescript进行脚本编写.

技术交流QQ群: 859823032

![editing script](res/ts_editing_1.png "so brilliant!")

# 特点 
* nodejs形式的模块系统 或者 单一脚本 (根据不同的 tsconfig 配置)
* 生成静态绑定代码以及对应的d.ts声明文件
* setTimeout/setInterval/clearTimeout/clearInterval 写法基本兼容
* c# delegates 封装
* Unity常用值类型优化 (无GC) (Vector2/3,Quaternion,Color...)
* 集成定点数运算支持 ([libfixmath](https://github.com/PetteriAimonen/libfixmath))
* 集成 websocket ([libwebsockets](https://github.com/warmcat/libwebsockets))
* iOS (64bit, bitcode)
* Android (v7a, v8a, x86)
* 远程调试器支持 (vscode) 
* 集成 promise (bluebird.js)
* coroutine (duktape thread)
* socket (tcp/udp)
* kcp (not implemented)

您可以在项目中使用大部分纯js实现的的库, 比如 protobufjs.
![protobufjs](res/test_protobufjs.png)

# 类型定义文件 (d.ts)
自动生成的 d.ts 声明文件将提供完整的类型信息, 使您可以使用代码提示/重构/定义跳转/引用查询等各种方便有用的辅助功能!

- delegate 类型信息
![type definition files](res/type_definition_1.png)
- out/ref 参数的泛型约束
![type definition files](res/type_definition_2.png)
- Unity 常用泛型接口(AddComponent/GetComponent)的泛型约束
![type definition files](res/type_definition_3.png)

# 开发环境
如果您使用typescript编写脚本, 则需要在系统中安装 typescript (以便在 Unity 中调用 tsc)
```shell
npm install -g typescript
```

# Clone 
```shell
# 建议附带参数 --depth=1
git clone https://github.com/ialex32x/duktape-unity --depth=1
```

# 构建
如果您需要从 duktape 源代码进行构建, 必须先安装 python/pip/pyyaml.
```shell
pip install pyyaml
```

duktape 源代码在目录 /duktape-<version>/src-input 下
```shell
./configure_duktape.bat # 生成经过预处理的 duktape 最终源代码
# 生成的代码位于:
# /build/src-debug (带调试器支持代码) 
# /build/src-release (不带调试器支持代码)
./make_duktape_<platform>.bat # 在 osx 中可用 make_duktape<platform>.sh
```
注: 如果在 Windows 下构建 Android 库, 则需要在 VS交叉编译命令行中执行bat (比如 VS2015 x64 ARM Cross Tools Command Prompt).<br/><br/>

'./scratch' 是一个简单的功能测试命令行工程, 可以在一个简单的环境中运行测试一些 duktape 的功能, 方便调试.
```shell
./configure_duktape_scratch.bat
./make_duktape_scratch.bat
```

# 基本例子

```ts

// 如果在 tsconfig.json 中定义了 {"module": "commonjs"}, 那么就可以在脚本中使用类似 nodejs 的模块系统
// 否则可以像一般的 H5 工程类似, 将代码编译为单一js脚本, 进而可以进行 uglify 等操作.


// 以下代码是模块形式组织的 
// import module
import { B } from "base/b"
// import module with relative path (. or ..)
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
        this.go.transform.localPosition = new Vector3(1, 2, 3) 
        // use Bridge to receive Enable/Disable/Update 
        // don't use the approach a lot, it's better to dispatch futher logic in a single Bridge
        this.go.AddComponent(DuktapeJS.Bridge).SetBridge(new MyPlayer()) 

        let f = new Custom()
        // you can assign function to c# delegate
        f.onopen = function () {
            // ...
        }
        // if you want to register multiple listener, use DuktapeJS.Dispather 
        // you can also use typed Dispatcher generated in _DuktapeDelegates.d.ts, it provides type checks
        f.onload = new DuktapeJS.Dispatcher1<void, string>() 
        f.onload.on(this, this.onload)  // add listener
        f.onload.off(this, this.onload) // remove listener
        f.onload.off(this)              // clear all listeners of this
        f.onload.clear()                // clear all

        // 'out' parameter in c#
        let v = {}
        if (System.Int32.TryParse("123", v)) {
            console.log(v.target)
        }
    }

    private onload(ev: string) {
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

// websocket
let ws = DuktapeJS.WebSocket()
ws.on("open", () => {
    console.log("connected")
    setInterval(() => {
        ws.send("hello, world") // string or buffer 
    }, 1000)
})
ws.on("data", data => {
    console.log("ws receive data", data) // string or buffer (depends on websocket message type you use)
})
ws.on("close", () => {
    console.log("connection lost")
})
ws.connect("ws://127.0.0.1:8080/echo")
setInterval(() => {
    ws.poll()
}, 50)

```

```ts
// http request example
console.log("http requesting...");
HttpRequest.GET("http://t.weather.sojson.com/api/weather/city/101030100", null, (status, res) => {
    console.warn("http response:", status, res);
    if (status) {
        let obj = JSON.parse(res);
        console.log("as object", obj.message);
    }
});
```

```ts
// coroutine example
let co = new Coroutine(function (x) {
    console.log("duktape thread, start:", x);
    for (var i = 1; i <= 5; ++i) {
        let r = Coroutine.yield(i);
        console.log("duktape thread, yield:", r);
    }
    // Coroutine.break();
    return "all done!";
});
let c = 'A'.charCodeAt(0);
while (co.next(String.fromCharCode(c++))) {
    console.log("duktape thread, next:", co.value);
}
console.log("duktape thread, done:", co.value);
```

# 开发进度
目前还没有充分测试, 请不要用于生产环境!!! <br/>
Vector2/Matrix3x3/Matrix4x4/Quaternion 等值类型一部分在*C*中实现, 尚未经过完整测试. <br/>

# 基本使用流程
首次打开需要执行菜单 [Duktape -> Generate Bindings] 生成绑定代码.
如果检测到工程目录中存在 tsconfig.json, Unity 编辑器将自动尝试在 typescript 脚本变更时进行编译.

## 如何自定义导出

* duktape.json
可以修改 ./duktape.json 配置文件 (详细可配置项可以参考 Assets/Duktape/Editor/Prefs.cs 中的定义及注释说明)
```javascript
{
    "outDir": "Assets/Generated",
    "typescriptDir": "Assets/Generated",
    "extraExt": "",
    // rootpath of ts/js project
    "workspace": "",
    "logPath": "Temp/duktape.log",
    // auto, cr, lf, crlf
    "newLineStyle": "auto",
    "implicitAssemblies": [
        "UnityEngine",
        "UnityEngine.CoreModule", 
        "UnityEngine.UI", 
        "UnityEngine.UIModule"
    ], 
    "explicitAssemblies": [
        "Assembly-CSharp"
    ],
    // types in blacklist will not be exported
    "typePrefixBlacklist": [
        "JetBrains.",
        "Unity.Collections.",
        "Unity.Jobs.",
        "Unity.Profiling.",
        "UnityEditor.",
        "UnityEditorInternal.",
        "UnityEngineInternal.",
        "UnityEditor.Experimental.",
        "UnityEngine.Experimental.",
        "Unity.IO.LowLevel.",
        "Unity.Burst.",
        // more types ...
        "UnityEngine.Assertions."
    ], 
    "ns": "DuktapeJS",
    "tab": "    "
}
```
* 自定义一个编辑器运行时的类型并实现接口 Duktape.IBindingProcess (或者直接继承抽象类 AbstractBindingProcess)
```c#
public class MyCustomBinding : AbstractBindingProcess
{
    public override void OnPreCollectTypes(BindingManager bindingManager)
    {
        /*
        bindingManager.AddExportedType(typeof(MyCustomClass));
        bindingManager.TransformType(typeof(MyCustomClass))
            .SetMethodBlocked("AMethodName")
            .SetMethodBlocked("AMethodNameWithParameters", typeof(string), typeof(int))
        */
    }

    public override void OnCleanup(BindingManager bindingManager)
    {
        Debug.Log($"finish @ {DateTime.Now}");
    }
}
```

## 例子场景
Assets/Scenes/main.unity 展示了一些基本的使用情况.<br/>

# 调试器

可以通过调试器进行断点调试:
* 支持 js/ts 的远程调试
## unity项目目录中默认编译的动态库是带调试器功能的, 如果需要使用不带调试器的release版本, 可以使用 /prebuilt/release 目录中预编译的版本. 或自行编译.

![debugger](res/debugger.png)

调试插件为:
```yml
Name: Duktape Debugger
Id: haroldbrenes.duk-debug
Description: Debug Adapter for Duktape runtimes.
Version: 0.5.6
Publisher: HaroldBrenes
VS Marketplace Link: https://marketplace.visualstudio.com/items?itemName=HaroldBrenes.duk-debug
```
请使用最新版本 0.5.6, 该插件调试ts时存在bug, 可以使用[修改过的版本](https://github.com/ialex32x/vscode-duktape-debug/releases/download/v0.5.6ts/duk-debug-0.5.6.vsix)

# Referenced libraries

* [duktape](https://github.com/svaarala/duktape)
* [slua](https://github.com/pangweiwei/slua)
* [xLua](https://github.com/Tencent/xLua)
* [typescript-for-unity](https://github.com/SpiralP/typescript-for-unity)
* [godot](https://github.com/godotengine/godot)
* [libwebsockets](https://github.com/warmcat/libwebsockets)
* [mbedtls](https://github.com/ARMmbed/mbedtls)
* [libfixmath](https://github.com/PetteriAimonen/libfixmath)
* [bluebird.js](https://github.com/petkaantonov/bluebird)

# Misc.

* [vscode-duktape-debug](https://github.com/harold-b/vscode-duktape-debug)
* [duktape-doc-debugger](https://github.com/svaarala/duktape/blob/master/doc/debugger.rst)

