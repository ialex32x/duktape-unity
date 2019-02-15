using System;
using System.Collections.Generic;

namespace Duktape
{
    using UnityEngine;
    
    public interface IFileSystem
    {
        bool Exists(string path);
        string ReadAllText(string path);
    }
}
