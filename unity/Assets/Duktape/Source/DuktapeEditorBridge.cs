using System;
using UnityEngine;

namespace Duktape
{
    public static class DuktapeEditorBridge
    {
        private static IDuktapeEditorListener _listener;

        public static void SetListener(IDuktapeEditorListener listener)
        {
#if UNITY_EDITOR
            _listener = listener;
#endif
        }

        public static void OnSourceModified()
        {
            if (_listener != null)
            {
                _listener.OnSourceModified();
            }
        }
    }
}
