using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;
    
    // duktape 配置 (editor only)
    public class Prefs
    {
        private static Prefs _prefs;
        public const string PATH = "ProjectSettings/duktape.json";

        private bool _dirty;

        // 静态绑定代码的生成目录
        public string outDir = "Assets/Duktape/Generated";

        public static Prefs GetSettings()
        {
            if (_prefs == null)
            {
                _prefs = JsonUtility.FromJson<Prefs>(PATH);
            }
            return _prefs;
        }

        public void MarkAsDirty()
        {
            _dirty =  true;
        }

        public void Save()
        {
            if (_dirty) 
            {
                var json = JsonUtility.ToJson(this, true);
                System.IO.File.WriteAllText(PATH, json);
            }
        }
    }
}