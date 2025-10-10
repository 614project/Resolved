using System;

namespace Resolved.Scripts;

public class ResolvedException : Exception
{
    public ResolvedException() : base() { }
    public ResolvedException(string message) : base(message) { }
}