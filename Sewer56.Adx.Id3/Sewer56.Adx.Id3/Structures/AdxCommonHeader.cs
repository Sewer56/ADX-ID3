#pragma warning disable CS1591
namespace Sewer56.Adx.Id3.Structures;

/// <summary>
/// Common elements of every ADX header.
/// </summary>
public struct AdxCommonHeader
{
    /*
     * Standard ADX Header
     * Note: Header is Big Endian.
     */
    
    /// <summary>
    /// Magic ADX value.
    /// </summary>
    public ushort Magic;

    /// <summary>
    /// Size of the ADX header.
    /// </summary>
    public ushort HeaderSize;
    
    public byte EncodingType;
    public byte FrameSize;
    public byte BitDepth;
    public byte ChannelCount;
    public int SampleRate;
    public int SampleCount;
    public short HighpassFreq;
    public byte Version;
    public byte Revision;

    /// <summary>
    /// True if this struct contains ADX magic (and is likely an ADX header).
    /// </summary>
    public static bool IsAdxMagic(ushort magicValue)
    {
        return magicValue == 0x8000;
    }
}