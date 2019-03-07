using System;
using System.Collections;

namespace Duktape
{
    using UnityEngine;

    // JS 实现的 MonoBehaviour
    public class DuktapeBehaviour : MonoBehaviour
    {
        private DuktapeValue _prototype;
        private DuktapeObject _instance;

        public void MakeBridge(DuktapeObject instance)
        {
            _instance = instance;
            if (_instance != null)
            {
                var ctx = _instance.ctx;
                _instance.PushPrototype(ctx);
                var refid = DuktapeDLL.duk_unity_ref(ctx);
                _prototype = new DuktapeValue(ctx, refid);
                _instance.InvokeMember("Awake");
                if (enabled)
                {
                    _instance.InvokeMember("OnEnable");
                }
                if (_instance.GetMember("Update") != null)
                {
                    StartCoroutine(_Update());
                }
            }
        }

        IEnumerator _Update()
        {
            while (true)
            {
                _instance.InvokeMember("Update");
                yield return null;
            }
        }

        void Start()
        {
            if (_instance != null)
            {
                _instance.InvokeMember("Start");
            }
        }

        void OnEnable()
        {
            if (_instance != null)
            {
                _instance.InvokeMember("OnEnable");
            }
        }

        void OnDisable()
        {
            if (_instance != null)
            {
                _instance.InvokeMember("OnDisable");
            }
        }

        void OnDestroy()
        {
            if (_instance != null)
            {
                _instance.InvokeMember("OnDestroy");
                _instance = null;
            }

            if (_prototype != null)
            {
                _prototype = null;
            }
        }
    }
}
