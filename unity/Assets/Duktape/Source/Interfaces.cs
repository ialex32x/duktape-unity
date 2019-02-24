using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;

    public interface IDuktapeListener
    {
        void OnTypesBinding(DuktapeVM vm);
        void OnProgress(DuktapeVM vm, int step, int total);
        void onLoaded(DuktapeVM vm);
    }
}
