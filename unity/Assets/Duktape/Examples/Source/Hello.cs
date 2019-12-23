using System;
using UnityEngine;

namespace SampleNamespace
{
    [Duktape.JSType]
    public class Hello : MonoBehaviour
    {
        void Awake()
        {
            // GetComponents();
            Debug.Log("Hello, World");
        }
    }
}