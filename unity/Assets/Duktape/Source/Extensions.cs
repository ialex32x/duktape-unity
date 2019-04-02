using System;
using System.Collections;

namespace Duktape
{
    using UnityEngine;

    // [JSNaming("DuktapeJS.")]
    // public class Break : YieldInstruction
    // {
    // }

    // [JSType]
    // [JSOmit]
    // public static class Extensions
    // {
    //     public static Coroutine StartCoroutine(this MonoBehaviour mb, DuktapeFunction fn)
    //     {
    //         return mb.StartCoroutine(DuktapeThreadRun(fn));
    //     }

    //     private static IEnumerator DuktapeThreadRun(DuktapeFunction fn)
    //     {
    //         var ctx = fn.ctx;
    //         var idx = DuktapeDLL.duk_push_thread(ctx);
    //         var thread_ctx = DuktapeDLL.duk_get_context(ctx, idx);
    //         var thread_refid = DuktapeDLL.duk_unity_ref(ctx);
    //         var thread = new DuktapeThread(thread_ctx, thread_refid, fn);
    //         while (true)
    //         {
    //             var instruction = thread.Resume();
    //             if (instruction is Break)
    //             {
    //                 yield break;
    //             }
    //             yield return instruction;
    //         }
    //     }
    // }
}
