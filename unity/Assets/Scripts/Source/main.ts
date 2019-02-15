import { Foo } from "./mm/foo";
import { UnityEngine } from "../Typings/unity";

let foo = new Foo(12)

foo.greet()

// UnityEngine.Debug.Log("greeting")
let go = new UnityEngine.GameObject()
go.SetActive(false)
go.Foo()
// UnityEngine.Object.Destroy(go)
