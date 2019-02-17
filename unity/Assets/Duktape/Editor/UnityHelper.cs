using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    using UnityEditor;

    public class UnityHelper
    {
        #region All Menu Items
        [MenuItem("Duktape/Generate Bindings")]
        public static void GenerateBindings()
        {
            var bm = new BindingManager();
            bm.Generate();
            AssetDatabase.Refresh();
        }
        #endregion
    }
}
