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
        private Dictionary<string, string> headers = new Dictionary<string, string>();

        public HttpRequest()
        {
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
            _req = new UnityWebRequest();
            if (!_requesting)
            {
                var runner = DuktapeRunner.GetRunner();
                if (runner != null)
                {
                    _req.method = "GET";
                    _req.url = url;
                    _requesting = true;
                    this.ApplyHeaders();
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
            var form = new WWWForm();
            var ps = body.Split('&');
            foreach (var p in ps)
            {
                var pp = p.Split('=');
                form.AddField(pp[0], pp[1] != null ? pp[1] : "");
            }
            _req = UnityWebRequest.Post(url, form);
            if (!_requesting)
            {
                var runner = DuktapeRunner.GetRunner();
                if (runner != null)
                {
                    _requesting = true;
                    this.ApplyHeaders();
                    runner.StartCoroutine(Run(oncomplete));
                }
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        protected void ApplyHeaders()
        {
            foreach (var p in this.headers)
            {
                _req.SetRequestHeader(p.Key, p.Value);
            }
        }

        public void SetRequestHeader(string key, string value)
        {
            if (!_requesting)
            {
                this.headers[key] = value;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
