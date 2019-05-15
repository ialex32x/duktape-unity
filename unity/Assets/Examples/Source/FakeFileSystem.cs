using System;
using UnityEngine;

// 临时
public class FakeFileSystem : Duktape.IFileSystem
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
