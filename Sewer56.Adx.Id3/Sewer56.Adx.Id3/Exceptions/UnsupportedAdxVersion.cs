using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sewer56.Adx.Id3.Exceptions;

/// <summary>
/// Thrown when an unsupported ADX version is used.
/// </summary>
public class UnsupportedAdxVersion : Exception
{
    /// <inheritdoc />
    public UnsupportedAdxVersion(string? message) : base(message) { }
}