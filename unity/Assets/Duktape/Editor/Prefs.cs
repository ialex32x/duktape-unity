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
        public const string PATH = "ProjectSettings/duktape.json";

        public string logPath = "Temp/duktape.log";

        private bool _dirty;

        // 静态绑定代码的生成目录
        public string outDir = "Assets/Duktape/Generated";

        public string extraExt = ""; // 生成文件的额外后缀

        public NewLineStyle newLineStyle;

        public string newline
        {
            get
            {
                switch (newLineStyle)
                {
                    case NewLineStyle.CR: return "\r";
                    case NewLineStyle.LF: return "\n";
                    case NewLineStyle.CRLF: return "\r\n";
                    default: return Environment.NewLine;
                }
            }
        }

        public string tab = "    ";

        // 生成的绑定类所在命名空间
        public string ns = "DuktapeJS";

        // 默认不导出任何类型, 需要指定导出类型列表
        public List<string> explicitAssemblies = new List<string>(new string[]
        {
            //TODO: codegen 完善后再默认开启
            // "Assembly-CSharp-firstpass",
            "Assembly-CSharp",
        });

        // 默认导出所有类型, 过滤黑名单
        public List<string> implicitAssemblies = new List<string>(new string[]
        {
            //TODO: codegen 完善后再默认开启
            // "UnityEngine",
            // "UnityEngine.CoreModule",
            // "UnityEngine.UIModule",
            // "UnityEngine.TextRenderingModule",
            // "UnityEngine.TextRenderingModule",
            // "UnityEngine.UnityWebRequestWWWModule",
            // "UnityEngine.Physics2DModule",
            // "UnityEngine.AnimationModule",
            // "UnityEngine.TextRenderingModule",
            // "UnityEngine.IMGUIModule",
            // "UnityEngine.UnityWebRequestModule",
            // "UnityEngine.PhysicsModule",
            // "UnityEngine.UI",
        });

        // type.FullName 前缀满足以下任意一条时不会被导出
        public List<string> typePrefixBlacklist = new List<string>(new string[]
        {
            "JetBrains.",
            "Unity.Collections.",
            "Unity.Jobs.",
            "Unity.Profiling.",
            "UnityEditor.",
            "UnityEditorInternal.",
            "UnityEngineInternal.",
            "UnityEditor.Experimental.",
            "UnityEngine.Experimental.",
            "Unity.IO.LowLevel.",
            "Unity.Burst.",
        });

        public Prefs MarkAsDirty()
        {
            if (!_dirty)
            {
                _dirty = true;
                EditorApplication.delayCall += Save;
            }
            return this;
        }

        public static Prefs Load(string path)
        {
            try
            {
                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    Debug.Log($"load prefs: {json}");
                    return JsonUtility.FromJson<Prefs>(json);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning(exception);
            }
            return new Prefs();
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