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

        public string tab = "    ";

        // 生成的绑定类所在命名空间
        public string ns = "DuktapeJS";

        public static Prefs GetPrefs()
        {
            if (_prefs == null)
            {
                try
                {
                    if (System.IO.File.Exists(PATH))
                    {
                        var json = System.IO.File.ReadAllText(PATH);
                        _prefs = JsonUtility.FromJson<Prefs>(json);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogWarning(exception);
                }
                if (_prefs == null)
                {
                    _prefs = new Prefs();
                    _prefs.MarkAsDirty();
                }
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