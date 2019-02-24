using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;

    public interface IDuktapeListener
    {
        void OnTypesBinding(DuktapeVM vm);
        void OnBindingError(DuktapeVM vm, Type type);
        void OnProgress(DuktapeVM vm, int step, int total);
        void OnLoaded(DuktapeVM vm);
    }
}
