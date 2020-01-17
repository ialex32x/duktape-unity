using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public class RFileSystem : IFileSystem
{
    public bool Exists(string path)
    {
        var o = Resources.Load<TextAsset>(path);
        return o != null;
    }

    public byte[] ReadAllBytes(string path)
    {
        var o = Resources.Load<TextAsset>(path);
        return o.bytes;
    }
}
