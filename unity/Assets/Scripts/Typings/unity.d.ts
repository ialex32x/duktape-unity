
declare namespace UnityEngine {
    class Debug {
        /** Log Message */
        static Log(msg: string)
        static Warn(msg: string)
        static Error(msg: string)
    }
}

declare namespace UnityEngine {
    class GameObject extends Object {
        constructor(name?: string)
        readonly activeSelf: boolean
        SetActive(v: boolean)
    }
}

declare namespace UnityEngine {
    class Object {
        constructor()
        Foo()
        static Destroy(v: Object)
    }
}
