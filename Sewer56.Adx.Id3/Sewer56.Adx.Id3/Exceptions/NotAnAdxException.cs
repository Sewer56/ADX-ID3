using System;

namespace Sewer56.Adx.Id3.Exceptions;

/// <summary>
/// Thrown when the file is not an ADX file.
/// </summary>
public class NotAnAdxException : Exception
{
    /// <summary/>
    public NotAnAdxException(string? message) : base(message) { }
}