using System;

namespace Duktape
{
    public interface IContextualValue
    {
        DuktapeContext context { get; }
    }
}