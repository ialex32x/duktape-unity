using System;

namespace Duktape
{
    public interface IContextualValue
    {
        IntPtr ctx { get; }
    }
}