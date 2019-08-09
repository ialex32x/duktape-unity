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
        private static int _id;
        private static Dictionary<int, Coroutine> _timers = new Dictionary<int, Coroutine>();

        public static DuktapeRunner GetRunner()
        {
            if (_runner == null)
            {
                var go = new GameObject();
                go.hideFlags = HideFlags.HideAndDontSave;
                GameObject.DontDestroyOnLoad(go);
                _runner = go.AddComponent<DuktapeRunner>();
            }
            return _runner;
        }

        public static int SetLoop(Action fn)
        {
            var id = ++_id;
            GetRunner().AddLoop(id, fn);
            return id;
        }

        public static int SetTimeout(DuktapeFunction fn, double ms)
        {
            return SetTimeout(fn, (float)ms);
        }

        public static int SetTimeout(DuktapeFunction fn, float ms)
        {
            var id = ++_id;
            GetRunner().AddTimeout(id, fn, ms * 0.001f);
            return id;
        }

        public static int SetInterval(DuktapeFunction fn, double ms)
        {
            return SetInterval(fn, (float)ms);
        }

        public static int SetInterval(DuktapeFunction fn, float ms)
        {
            var id = ++_id;
            GetRunner().AddInterval(id, fn, ms * 0.001f);
            return id;
        }

        public static int SetInterval(Action fn, float ms)
        {
            var id = ++_id;
            GetRunner().AddInterval(id, fn, ms * 0.001f);
            return id;
        }

        public static bool Clear(int id)
        {
            return GetRunner().RemoveTimer(id);
        }

        private void AddTimeout(int id, DuktapeFunction fn, float seconds)
        {
            _timers[id] = StartCoroutine(_Timeout(id, fn, seconds));
        }

        private void AddInterval(int id, DuktapeFunction fn, float seconds)
        {
            _timers[id] = StartCoroutine(_Interval(id, fn, seconds));
        }

        private void AddInterval(int id, Action fn, float seconds)
        {
            _timers[id] = StartCoroutine(_Interval(id, fn, seconds));
        }

        private void AddLoop(int id, Action fn)
        {
            _timers[id] = StartCoroutine(_Loop(id, fn));
        }

        private bool RemoveTimer(int id)
        {
            Coroutine coroutine;
            if (_timers.TryGetValue(id, out coroutine))
            {
                StopCoroutine(coroutine);
                return _timers.Remove(id);
            }
            return false;
        }

        private IEnumerator _Loop(int id, Action fn)
        {
            while (true)
            {
                yield return null;
                fn();
            }
        }

        private IEnumerator _Timeout(int id, DuktapeFunction fn, float seconds)
        {
            var wait = seconds > 0 ? new WaitForSeconds(seconds) : null;
            yield return wait;
            _timers.Remove(id);
            fn.Invoke();
        }

        private IEnumerator _Interval(int id, DuktapeFunction fn, float seconds)
        {
            var wait = seconds > 0 ? new WaitForSeconds(seconds) : null;
            while (true)
            {
                yield return wait;
                fn.Invoke();
            }
        }

        private IEnumerator _Interval(int id, Action fn, float seconds)
        {
            var wait = seconds > 0 ? new WaitForSeconds(seconds) : null;
            while (true)
            {
                yield return wait;
                fn();
            }
        }
    }
}
