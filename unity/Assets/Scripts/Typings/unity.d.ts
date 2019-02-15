
declare namespace UnityEngine {
    class Debug {
        /** Log Message */
        static Log(msg: string)
        static Warn(msg: string)
        static Error(msg: string)
    }

    class Object {
        constructor()
        Foo()
        static Destroy(v: Object)
    }

    class GameObject extends Object {
        constructor()
        SetActive(v: boolean)
    }
}