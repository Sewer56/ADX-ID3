using System;
using System.IO;
using Sewer56.Adx.Id3.Exceptions;
using Sewer56.Adx.Id3.Structures;
using Sewer56.Adx.Id3.Utilities;

namespace Sewer56.Adx.Id3;

/// <summary>
/// Provides methods for reading ADX files.
/// </summary>
public static unsafe class AdxReader
{
    /// <summary>
    /// Retrieves an ID3 tag given a pointer to the start of an ADX file.
    /// </summary>
    /// <param name="adxData">Pointer to the first bytes of an ADX file.</param>
    /// <param name="numBytesAvailable">Number of bytes available at <paramref name="adxData"/>.</param>
    /// <exception cref="NotAnAdxException">File is not an ADX file.</exception>
    /// <exception cref="InsufficientDataException">Buffer is not long enough to contain all of the ID3 information.</exception>
    /// <returns>Bytes containing the ID3 header, else null.</returns>
    public static byte[] GetId3Tag(byte* adxData, int numBytesAvailable)
    {
        var header = (AdxCommonHeader*) adxData;
        if (!AdxCommonHeader.IsAdxMagic(header->Magic.AsBigEndian()))
            ThrowHelpers.ThrowNotAnAdxException(ThrowHelpers.ErrorInvalidMagicValue);

        var size = header->HeaderSize.AsBigEndian();
        if (size > numBytesAvailable)
            ThrowHelpers.ThrowInsufficientDataException(ThrowHelpers.ErrorCannotGetHeaderData);

        return GetId3Tag_Internal(adxData, numBytesAvailable);
    }

    /// <summary>
    /// Retrieves an ID3 tag given the start of an ADX stream.
    /// </summary>
    /// <param name="adxStream">Stream to the start of the ADX file.</param>
    /// <exception cref="NotAnAdxException">File is not an ADX file.</exception>
    /// <exception cref="InsufficientDataException">Stream is not long enough to contain all of the ID3 information.</exception>
    /// <returns>Bytes containing the ID3 header, else null.</returns>
    public static byte[] GetId3Tag(Stream adxStream)
    {
        if (!adxStream.TryReadSafe(out ushort magic))
            ThrowHelpers.ThrowInsufficientDataException(ThrowHelpers.ErrorCannotGetHeader);

        if (!AdxCommonHeader.IsAdxMagic(magic.AsBigEndian()))
            ThrowHelpers.ThrowNotAnAdxException(ThrowHelpers.ErrorInvalidMagicValue);

        if (!adxStream.TryReadSafe(out ushort headerSize))
            ThrowHelpers.ThrowInsufficientDataException(ThrowHelpers.ErrorCannotGetHeaderSize);

        // Allocate data for ADX header.
        var headerSizeLe = headerSize.AsBigEndian();
        byte* bytes           = stackalloc byte[headerSizeLe];
        *(ushort*)(bytes)     = magic;
        *(ushort*)(bytes + 2) = headerSize;

        if (!adxStream.TryReadSafe(new Span<byte>(bytes + 4, headerSizeLe - 4)))
            ThrowHelpers.ThrowInsufficientDataException(ThrowHelpers.ErrorCannotGetHeaderData);

        return GetId3Tag_Internal(bytes, headerSizeLe);
    }
    
    private static byte[] GetId3Tag_Internal(byte* adxData, int numBytesAvailable)
    {
        var offset = AdxUtils.GetId3Tag_Offset(adxData, out _);
        if (offset == -1)
            return null;

        var id3Ptr = (byte*)adxData + offset;
        var id3Length = Id3Utils.GetId3V2TagLength(id3Ptr, numBytesAvailable - offset);
        if (id3Length < 0)
            return null;

        // Normally would use Unsafe.CopyBlock, but in most cases, ID3 data is going to be short enough to not warrant it.
        var bytes = GC.AllocateUninitializedArray<byte>(id3Length);
        new Span<byte>(id3Ptr, id3Length).CopyTo(bytes);
        return bytes;
    }
}