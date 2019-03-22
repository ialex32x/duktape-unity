using System;
using System.Collections;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;

    // 提供简单的功能桥接
    [JSType]
    [JSNaming("DuktapeJS.Bridge")]
    public class DuktapeBridge : MonoBehaviour
    {
        private DuktapeObject _instance;

        public void SetBridge(DuktapeObject obj)
        {
            _instance = obj;
            if (enabled)
            {
                _instance.InvokeMember("OnEnable");
            }
            if (_instance.GetMember("Update") != null)
            {
                StartCoroutine(_Update());
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
        }
    }
}
