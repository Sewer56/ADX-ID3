using System;

namespace Sewer56.Adx.Id3.Exceptions;

/// <summary>
/// Exception when there isn't enough data for an ID3 tag.
/// </summary>
public class InsufficientDataException : Exception
{
    /// <inheritdoc />
    public InsufficientDataException(string? message) : base(message) { }
}