using Sewer56.Adx.Id3.Utilities;

namespace Sewer56.Adx.Id3;

/// <summary>
/// Utilities for working with ID3 tags.
/// </summary>
public static class Id3Utils
{
    private const uint Id3HeaderBytes = 0x49443300;
    private const uint Id3FooterBytes = 0x33444900;

    private const uint Id3Mask = 0xFFFFFF00;
    private const int Id3HeaderLength = 10;

    /// <summary>
    /// Returns true if a complete ID3 tag exists in this location.
    /// </summary>
    /// <param name="id3TagPtr">Pointer to the id3 tag.</param>
    /// <param name="numBytesAvailable">Number of bytes available.</param>
    /// <returns>Positive number indicating size if a whole id3 tag can be found. Else a negative number if tag not found or not enough bytes.</returns>
    public static unsafe int GetId3V2TagLength(byte* id3TagPtr, int numBytesAvailable)
    {
        // Implemented to official spec at https://id3.org/id3v2.3.0#ID3v2_header
        // Check ID3 string.
        
        if (!IsId3Header((uint*) id3TagPtr))
            return -1;

        if (id3TagPtr[3] >= 0xFF || id3TagPtr[4] >= 0xFF)
            return -1;

        if (id3TagPtr[6] >= 0x80 || id3TagPtr[7] >= 0x80 || id3TagPtr[8] >= 0x80 || id3TagPtr[9] >= 0x80)
            return -1;

        // Read Size
        var afterHeaderSize = id3TagPtr[9]       |
                              id3TagPtr[8] << 7  |
                              id3TagPtr[7] << 14 |
                              id3TagPtr[6] << 21;

        var headerSize = afterHeaderSize + Id3HeaderLength; 

        // Check for footer. 
        if (IsId3Footer((uint*)(id3TagPtr + headerSize)))
            return headerSize + Id3HeaderLength;

        return headerSize;
    }

    private static unsafe bool IsId3Header(uint* id3TagPtr)
    {
        return ((*id3TagPtr).AsBigEndian() & Id3Mask) == Id3HeaderBytes;
    }
    private static unsafe bool IsId3Footer(uint* id3TagPtr)
    {
        return ((*id3TagPtr).AsBigEndian() & Id3Mask) == Id3FooterBytes;
    }
}