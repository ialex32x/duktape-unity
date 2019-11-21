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
        private static DuktapeRunner _runner;

        private Dictionary<uint, Timer> _timers = new Dictionary<uint, Timer>();

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
            var runner = GetRunner();
            if (runner != null)
            {
                var timer = runner._scheduler.CreateTimer(ms, fn, 1);
                runner._timers.Add(timer.id, timer);
                return timer.id;
            }
            return 0;
        }

        public static uint SetInterval(DuktapeFunction fn, int ms)
        {
            var runner = GetRunner();
            if (runner != null)
            {
                var timer = runner._scheduler.CreateTimer(ms, fn, -1);
                runner._timers.Add(timer.id, timer);
                return timer.id;
            }
            return 0;
        }

        public static uint SetInterval(Action fn, int ms)
        {
            var runner = GetRunner();
            if (runner != null)
            {
                var timer = runner._scheduler.CreateTimer(ms, new InvokableAction(fn), -1);
                runner._timers.Add(timer.id, timer);
                return timer.id;
            }
            return 0;
        }

        public static bool Clear(uint id)
        {
            var runner = GetRunner();
            if (runner != null)
            {
                Timer timer;
                if (runner._timers.TryGetValue(id, out timer))
                {
                    timer.enabled = false;
                    return true;
                }
            }
            return false;
        }

        private bool RemoveTimer(uint id)
        {
            Timer timer;
            if (_timers.TryGetValue(id, out timer))
            {
                _timers.Remove(id);
                timer.enabled = false;
                return true;
            }

            return false;
        }

        private void Update()
        {
            _scheduler.Update((int)(Time.deltaTime * 1000f));
        }
    }
}