using System;
using System.Collections;

namespace Duktape
{
    using UnityEngine;

    [JSType]
    [JSOmit]
    public static class Extensions
    {
        // public static Coroutine StartCoroutine(this MonoBehaviour mb, DuktapeObject val)
        // {
        //     return mb.StartCoroutine(DuktapeCoroutineRun(val));
        // }
        //
        // private static IEnumerator DuktapeCoroutineRun(DuktapeObject val)
        // {
        //     bool returnValue;
        //     do
        //     {
        //         returnValue = val.InvokeMemberWithBooleanReturn("next");
        //         var value = val.GetProperty("value");
        //         yield return value;
        //     } while (returnValue);
        // }
        
        public static Coroutine StartCoroutine(this MonoBehaviour mb, DuktapeFunction fn)
        {
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
