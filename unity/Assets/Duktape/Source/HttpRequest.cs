using System;
using System.Collections;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using UnityEngine.Networking;

    [JSType]
    [JSNaming("DuktapeJS.HttpRequest")]
    public class HttpRequest
    {
        private bool _requesting;
        private UnityWebRequest _req;

        public HttpRequest()
        {
            _req = new UnityWebRequest();
        }

        private IEnumerator Run(Action<bool, string> oncomplete)
        {
            _req.downloadHandler = new DownloadHandlerBuffer();
            yield return _req.SendWebRequest();
            if (_requesting)
            {
                if (_req.isDone)
                {
                    var res = _req.downloadHandler.text;
                    _requesting = false;
                    oncomplete?.Invoke(true, res);
                }
                else
                {
                    var res = _req.error;
                    _requesting = false;
                    oncomplete?.Invoke(false, res);
                }
            }
        }

        public void Cancel()
        {
            if (_requesting)
            {
                _requesting = false;
                _req.Abort();
            }
        }

        public void SendGetRequest(string url, Action<bool, string> oncomplete)
        {
            if (!_requesting)
            {
                var runner = DuktapeRunner.GetRunner();
                if (runner != null)
                {
                    _req.method = "GET";
                    _req.url = url;
                    _requesting = true;
                    runner.StartCoroutine(Run(oncomplete));
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void SendPostRequest(string url, string body, Action<bool, string> oncomplete)
        {
            if (!_requesting)
            {
                var runner = DuktapeRunner.GetRunner();
                if (runner != null)
                {
                    _req.method = "GET";
                    _req.url = url;
                    _requesting = true;
                    runner.StartCoroutine(Run(oncomplete));
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        public void SetRequestHeader(string key, string value)
        {
            if (!_requesting)
            {
                _req.SetRequestHeader(key, value);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
