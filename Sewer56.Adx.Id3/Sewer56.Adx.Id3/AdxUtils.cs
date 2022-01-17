using System;
using Sewer56.Adx.Id3.Structures;
using Sewer56.Adx.Id3.Utilities;

namespace Sewer56.Adx.Id3;

/// <summary>
/// General use utilities to use with ADX files.
/// </summary>
public static unsafe class AdxUtils
{
    /// <summary>
    /// Size of CRI ADX Loop Info.
    /// </summary>
    public const int LoopInfoSize = 24;

    /// <summary>
    /// Size of a CRI ADX Version 3 Base Header.
    /// </summary>
    public const int Version3BaseHeaderSize = 20;

    /// <summary>
    /// Size of a CRI ADX Version 4 Base Header.
    /// </summary>
    public const int Version4BaseHeaderSize = 32;

    /// <summary>
    /// Size of a CRI ADX Version 3 Full Header.
    /// </summary>
    public const int Version3FullHeaderSize = 44;

    /// <summary>
    /// Size of a CRI ADX Version 4 Full Header.
    /// </summary>
    public const int Version4FullHeaderSize = 56;

    /// <summary>
    /// Calculates the offset of an ID3 tag in an ADX file.
    /// </summary>
    /// <param name="adxData">Pointer to the start of the ADX file.</param>
    /// <param name="headerLength">Length of the ADX header. This returns a positive value for any recognized ADX version.</param>
    /// <returns>Positive value if potential position of ID3 tag has been found, else false.</returns>
    /// <remarks>A return value of -1 with <paramref name="headerLength"/> of -1 indicates unsupported version.</remarks>
    public static int GetId3Tag_Offset(byte* adxData, out int headerLength)
    {
        var header  = (AdxCommonHeader*)adxData;
        var version = header->Version;
        var versionHeaderSize = version == 4 ? Version4FullHeaderSize : Version3FullHeaderSize;

        // No loop information (and no ID3 tag)
        if (header->HeaderSize.AsBigEndian() < versionHeaderSize)
        {
            headerLength = version == 4 ? Version4BaseHeaderSize : Version3BaseHeaderSize;
            return -1;
        }

        headerLength = versionHeaderSize;
        return versionHeaderSize;
    }
}