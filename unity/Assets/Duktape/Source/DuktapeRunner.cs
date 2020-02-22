using System;
using System.Collections;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

    // 利用 coroutine 实现的简易定时运行管理器 (interval/timeout/loop)
    public class DuktapeRunner : MonoBehaviour
    {
        private static uint _idgen;
        private static DuktapeRunner _runner;

        private Dictionary<uint, ulong> _timers = new Dictionary<uint, ulong>();

        private Scheduler _scheduler = new Scheduler();

        public static DuktapeRunner GetRunner()
        {
            if (_runner == null)
            {
                if (Application.isPlaying)
                {
                    var go = new GameObject { hideFlags = HideFlags.HideAndDontSave };
                    GameObject.DontDestroyOnLoad(go);
                    _runner = go.AddComponent<DuktapeRunner>();
                }
            }
            return _runner;
        }

        public static uint SetTimeout(DuktapeFunction fn, int ms)
        {
            return CreateTimer(fn, ms, true);
        }

        public static uint SetTimeout(Action fn, int ms)
        {
            return CreateTimer(fn, ms, true);
        }

        public static uint SetInterval(DuktapeFunction fn, int ms)
        {
            return CreateTimer(fn, ms, false);
        }

        public static uint SetInterval(Action fn, int ms)
        {
            return CreateTimer(fn, ms, false);
        }

        private static uint CreateTimer(Action fn, int ms, bool once)
        {
            var runner = GetRunner();
            if (runner != null)
            {
                var id = ++_idgen;
                var timer = runner._scheduler.Add(ms, once, new InvokableAction(fn));
                runner._timers.Add(id, timer);
                return id;
            }
            return 0;
        }

        private static uint CreateTimer(DuktapeFunction fn, int ms, bool once)
        {
            var runner = GetRunner();
            if (runner != null)
            {
                var id = ++_idgen;
                var timer = runner._scheduler.Add(ms, once, fn);
                runner._timers.Add(id, timer);
                return id;
            }
            return 0;
        }

        public static bool Clear(uint id)
        {
            var runner = GetRunner();
            if (runner != null)
            {
                ulong timer;
                if (runner._timers.TryGetValue(id, out timer))
                {
                    runner._timers.Remove(id);
                    runner._scheduler.Remove(timer);
                    return true;
                }
            }
            return false;
        }

        private void Update()
        {
            _scheduler.Update((int)(Time.deltaTime * 1000f));
        }
    }
}
