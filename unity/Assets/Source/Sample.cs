using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Duktape;
using System;

public class Sample : MonoBehaviour
{
    DuktapeHeap heap = new DuktapeHeap();

    public static string GetFullPath(string path, string basePath, char sp)
    {
        var items = System.IO.Path.Combine(basePath, path).Split(sp);
        if (items.Length < 2)
        {
            return path;
        }
        var array = new List<string>(items.Length);
        for (var i = 0; i < items.Length; i++)
        {
            var item = items[i];
            switch (item)
            {
                case ".": break;
                case "..": array.RemoveAt(array.Count - 1); break;
                default: array.Add(item); break;
            }
        }
        return System.IO.Path.Combine(array.ToArray());
    }

    // Start is called before the first frame update
    void Start()
    {
        var path = System.IO.Path.Combine("D1", "D2", "D3\\D4", ".\\.\\..\\..\\D5", "..\\..\\..\\..\\..\\..\\", "File.txt");
        Debug.Log("!!! " + path);
        var fullpath = Sample.GetFullPath(path, "C:\\Documents\\julio\\files\\private\\", '\\');
        Debug.Log("!!! " + fullpath);

        heap.Test();
        DuktapeDLL.duk_push_string(heap.ctx, string.Format("test {0} {1} native varg", "hello", 123));
        var str = DuktapeAux.duk_to_string(heap.ctx, -1);
        Debug.LogFormat("testcase[1]: {0} ## {1}", str, DuktapeDLL.duk_get_top(heap.ctx));
        DuktapeDLL.duk_pop(heap.ctx);
        var err = DuktapeDLL.duk_peval_string_noresult(heap.ctx, @"
print(123);
var test = new Test();
test.foo();
// test = undefined;
Test.static_foo();
// print(typeof Duptake);
var pig = require('./game/base/pig');
print(pig);
        ");
        if (err != 0)
        {
            Debug.LogError("error");
        }
        DuktapeDLL.duk_gc(heap.ctx, DuktapeDLL.DUK_GC_COMPACT);
    }

    void OnDestroy()
    {
        heap.Destroy();
        heap = null;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
