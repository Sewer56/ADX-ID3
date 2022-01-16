using System;
using Sewer56.Adx.Id3.Structures;
using Sewer56.Adx.Id3.Utilities;

namespace Sewer56.Adx.Id3;

/// <summary>
/// General use utilities to use with ADX files.
/// </summary>
public static unsafe class AdxUtils
{
    const int LoopStructSize = 20;

    /// <summary>
    /// Calculates the offset of an ID3 tag in an ADX file.
    /// </summary>
    /// <param name="adxData">Pointer to the start of the ADX file.</param>
    /// <param name="headerLength">Length of the ADX header. This returns a positive value for any recognized ADX version.</param>
    /// <returns>Positive value if potential position of ID3 tag has been found, else false.</returns>
    /// <remarks>A return value of -1 with <paramref name="headerLength"/> of -1 indicates unsupported version.</remarks>
    public static int GetId3Tag_Offset(byte* adxData, out int headerLength)
    {
        var header = (AdxCommonHeader*)adxData;
        var version = header->Version;
        byte* versionHeader = (byte*)(header + 1);  // Get version specific data.
        int versionHeaderOffset = (int)((nint)versionHeader - (nint)adxData);

        if (version == 3)
        {
            // No loop information (and no ID3 tag)
            if (versionHeaderOffset + LoopStructSize >= header->HeaderSize.AsBigEndian())
            {
                headerLength = versionHeaderOffset;
                return -1;
            }

            var loopCount = (*(ushort*)(versionHeader + 2)).AsBigEndian(); // loopCount = 0x16 [short]
            var loopBytes = loopCount * LoopStructSize;  // loopCount * 20

            headerLength = 0x18 + loopBytes; // [0x18] Offset to first loop item.
            return headerLength;
        }

        if (version == 4)
        {
            var historySize = Math.Max(header->ChannelCount * 4, 8); // Size of extra sample history.
            var postHistoryOffset = 4 + historySize; // (Padding + History). End of v4 non-looping header.

            // No loop information (and no ID3 tag)
            if (versionHeaderOffset + postHistoryOffset + LoopStructSize >= header->HeaderSize.AsBigEndian())
            {
                headerLength = versionHeaderOffset + postHistoryOffset;
                return -1;
            }

            // Skip to loop count.
            var loopCountOffset = postHistoryOffset + 2; // +AlignmentSamples[2]
            var loopCount = (*(ushort*)(versionHeader + loopCountOffset)).AsBigEndian();
            var loopBytes = loopCount * LoopStructSize; // loopCount * 20

            headerLength = versionHeaderOffset + loopCountOffset + sizeof(short) + loopBytes;
            return headerLength;
        }

        headerLength = -1;
        return -1;
    }
}