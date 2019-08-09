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

        public DuktapeObject SetBridge(DuktapeObject obj)
        {
            _instance = obj;
            _instance.InvokeMember("Awake");
            if (enabled)
            {
                _instance.InvokeMember("OnEnable");
            }
            return obj;
        }

        void Update()
        {
            if (_instance != null)
            {
                _instance.InvokeMember("Update", Time.deltaTime);
            }
        }

        void LateUpdate()
        {
            if (_instance != null)
            {
                _instance.InvokeMember("LateUpdate");
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

        void OnApplicationFocus()
        {
            if (_instance != null)
            {
                _instance.InvokeMember("OnApplicationFocus");
            }
        }

        void OnApplicationPause()
        {
            if (_instance != null)
            {
                _instance.InvokeMember("OnApplicationPause");
            }
        }

        void OnApplicationQuit()
        {
            if (_instance != null)
            {
                _instance.InvokeMember("OnApplicationQuit");
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
