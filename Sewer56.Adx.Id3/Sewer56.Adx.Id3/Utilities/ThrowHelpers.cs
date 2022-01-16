using System;
using Sewer56.Adx.Id3.Exceptions;

namespace Sewer56.Adx.Id3.Utilities;

/// <summary>
/// Classes that encapsulate the throwing of certain exceptions.
/// </summary>
public class ThrowHelpers
{
    /// <summary/>
    public const string ErrorCannotGetHeader = "Couldn't get ADX header.";

    /// <summary/>
    public const string ErrorInvalidMagicValue = "Invalid magic header value.";

    /// <summary/>
    public const string ErrorCannotGetHeaderData = "Couldn't get header data. Not enough bytes.";

    /// <summary/>
    public const string ErrorCannotGetHeaderSize = "Couldn't get header size.";

    /// <summary/>
    public static void ThrowInsufficientDataException(string message) => throw new InsufficientDataException(message);

    /// <summary/>
    public static void ThrowOffsetTooBigException(string message) => throw new OffsetTooBigException(message);

    /// <summary/>
    public static void ThrowNotAnAdxException(string message) => throw new NotAnAdxException(message);

    /// <summary/>
    public static void ThrowUnsupportedAdxVersion(string message) => throw new UnsupportedAdxVersion(message);
}