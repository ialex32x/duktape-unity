using System;
using System.Collections;

namespace Duktape
{
    using UnityEngine;

    [JSType]
    [JSOmit]
    public static class Extensions
    {
        public static Coroutine StartCoroutine(this MonoBehaviour mb, DuktapeObject val)
        {
            return mb.StartCoroutine(DuktapeCoroutineRun(val));
        }
        
        private static IEnumerator DuktapeCoroutineRun(DuktapeObject val)
        {
            // scratch code

            val.PushProperty(val.ctx, "thread");
            var thread = DuktapeDLL.duk_get_context(val.ctx, -1);
            DuktapeDLL.duk_pop(val.ctx);
            if (thread == IntPtr.Zero || thread == val.ctx)
            {
                Debug.LogError("invalid thread ptr");
                yield break;
            }
            var context = new DuktapeContext(val.context.vm, thread);
            bool returnValue;
            do
            {
                returnValue = val.InvokeMemberWithBooleanReturn("next");
                var value = val.GetProperty("value");
                yield return value;
            } while (returnValue);
            context.Destroy();
        }
        
        public static Coroutine StartCoroutine(this MonoBehaviour mb, DuktapeFunction fn)
        {
            Debug.LogError("DONT DO THIS, NOT IMPLEMENTED CORRECTLY");
            return mb.StartCoroutine(DuktapeThreadRun(fn));
        }

        private static IEnumerator DuktapeThreadRun(DuktapeFunction fn)
        {
            var thread = new DuktapeThread(fn);
            while (true)
            {
                object instruction;
                if (thread.Resume(out instruction))
                {
                    yield return instruction;
                }
                yield break;
            }
        }
    }
}
