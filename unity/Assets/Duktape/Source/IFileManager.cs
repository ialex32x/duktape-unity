using System;
using System.Collections.Generic;
using UnityEngine;

namespace Duktape
{
    public interface IFileManager
    {
        bool Exists(string path);
        string LoadText(string path);
    }
}
