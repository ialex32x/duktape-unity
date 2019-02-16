using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public enum NewLineStyle
    {
        AUTO,
        CR,
        LF,
        CRLF,
    }

    // duktape 配置 (editor only)
    public class Prefs
    {
        private static Prefs _prefs;
        public const string PATH = "ProjectSettings/duktape.json";

        private bool _dirty;

        // 静态绑定代码的生成目录
        public string outDir = "Assets/Duktape/Generated";

        public NewLineStyle newLineStyle;

        public static Prefs GetPrefs()
        {
            if (_prefs == null)
            {
                try
                {
                    if (System.IO.File.Exists(PATH))
                    {
                        _prefs = JsonUtility.FromJson<Prefs>(PATH);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(exception);
                }
                _prefs = new Prefs();
            }
            return _prefs;
        }

        public void MarkAsDirty()
        {
            if (!_dirty)
            {
                _dirty = true;
                EditorApplication.delayCall += Save;
            }
        }

        public void Save()
        {
            if (_dirty)
            {
                _dirty = false;
                try
                {
                    var json = JsonUtility.ToJson(this, true);
                    System.IO.File.WriteAllText(PATH, json);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(exception);
                }
            }
        }
    }
}