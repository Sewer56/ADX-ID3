using System;

namespace Sewer56.Adx.Id3.Exceptions;

/// <summary>
/// Thrown when the ID3 data appended is too big.
/// </summary>
public class OffsetTooBigException : Exception
{
    /// <inheritdoc />
    public OffsetTooBigException(string? message) : base(message) { }
}