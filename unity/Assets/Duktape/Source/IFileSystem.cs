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

    public class DefaultFileSystem : IFileSystem
    {
        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public string ReadAllText(string path)
        {
            try
            {
                return System.IO.File.ReadAllText(path);
            }
            catch (Exception exception)
            {
                Debug.LogError($"{path}: {exception}");
                return null;
            }
        }
    }
}
