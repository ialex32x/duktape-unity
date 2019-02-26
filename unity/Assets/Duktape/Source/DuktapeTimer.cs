using System;
using System.Collections;
using System.Collections.Generic;
using AOT;

namespace Duktape
{
    using UnityEngine;

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
                _runner = go.AddComponent<DuktapeRunner>();
            }
            return _runner;
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

        private IEnumerator _Timeout(int id, DuktapeFunction fn, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _timers.Remove(id);
            fn.Call();
        }

        private IEnumerator _Interval(int id, DuktapeFunction fn, float seconds)
        {
            var wait = new WaitForSeconds(seconds);
            while (true)
            {
                yield return wait;
                fn.Call();
            }
        }
    }
}
