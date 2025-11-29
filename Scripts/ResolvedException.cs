using System;

namespace Resolved.Scripts;

/// <summary>
/// Resolved 애플리케이션에서 발생하는 예외.
/// </summary>
public class ResolvedException : Exception
{
    public ResolvedException() : base() { }
    public ResolvedException(string message) : base(message) { }
}