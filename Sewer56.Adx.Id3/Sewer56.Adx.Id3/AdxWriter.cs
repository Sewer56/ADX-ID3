using System;
using System.IO;
using System.Runtime.CompilerServices;
using Sewer56.Adx.Id3.Exceptions;
using Sewer56.Adx.Id3.Structures;
using Sewer56.Adx.Id3.Utilities;

namespace Sewer56.Adx.Id3;

/// <summary>
/// The <see cref="AdxWriter"/> class rewrites the ADX header to include ID3 information. 
/// </summary>
public static unsafe class AdxWriter
{
    /// <summary>
    /// Sector alignment for CD-ROMs.
    /// </summary>
    public const int DiscSectorAlignment = 2048;
    
    private const int DummyLoopMaxBytes = 1080; // 1064: Max possible header size (V4 with 255 channels). Added extra just in case.

    /// <summary>
    /// Dummy Loop data for ADX V3.
    /// </summary>
    private static readonly byte[] DummyLoopDataV3 = new byte[]
    {
        0x00, 0x00, // Alignment Samples
        0x00, 0x01, // Loop Count
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 // Dummy Loop
    };

    /// <summary>
    /// Writes an ID3 tag to a given ADX file.
    /// </summary>
    /// <param name="adxStream">Stream to the start of an existing ADX file.</param>
    /// <param name="outputStream">The stream to which the new ADX file should be written to.</param>
    /// <param name="data">The ID3 data to append to the new ADX file.</param>
    /// <exception cref="NotAnAdxException">File is not an ADX file.</exception>
    /// <exception cref="InsufficientDataException">Stream is not long enough to contain all of the ID3 information.</exception>
    /// <exception cref="UnsupportedAdxVersion">ADX stream uses a version of ADX that is not supported.</exception>
    [SkipLocalsInit]
    public static void WriteAdx(Stream adxStream, Stream outputStream, Span<byte> data)
    {
        if (!adxStream.TryReadSafe(out ushort magic))
            ThrowHelpers.ThrowInsufficientDataException(ThrowHelpers.ErrorCannotGetHeader);

        if (!AdxCommonHeader.IsAdxMagic(magic.AsBigEndian()))
            ThrowHelpers.ThrowNotAnAdxException(ThrowHelpers.ErrorInvalidMagicValue);

        if (!adxStream.TryReadSafe(out ushort headerSize))
            ThrowHelpers.ThrowInsufficientDataException(ThrowHelpers.ErrorCannotGetHeaderSize);

        // Allocate data for previous full ADX header and possible header extension.
        var headerSizeLe                = headerSize.AsBigEndian();
        byte* origHeaderBytes           = stackalloc byte[headerSizeLe + DummyLoopMaxBytes];
        *(ushort*)(origHeaderBytes)     = magic;
        *(ushort*)(origHeaderBytes + 2) = headerSize;

        if (!adxStream.TryReadSafe(new Span<byte>(origHeaderBytes + 4, headerSizeLe - 6))) // -6 to exclude the (c part of the CRI copyright header.
            ThrowHelpers.ThrowInsufficientDataException(ThrowHelpers.ErrorCannotGetHeaderData);
        
        // Calculate New Header Offset
        bool hasLoop = AdxUtils.GetId3Tag_Offset(origHeaderBytes, out var adxHeaderSize) != -1;
        int extraHeaderSize = 0;
        if (!hasLoop)
        {
            // Add dummy loop if necessary.
            if (adxHeaderSize == -1)
                ThrowHelpers.ThrowUnsupportedAdxVersion("Unsupported ADX version.");

            // Add dummy loop data.
            DummyLoopDataV3.AsSpanFast().CopyTo(new Span<byte>(origHeaderBytes + adxHeaderSize, DummyLoopDataV3.Length));
            extraHeaderSize = DummyLoopDataV3.Length;
        }

        int alignmentBytes = CalculateAlignmentBytes(origHeaderBytes, adxHeaderSize + extraHeaderSize + data.Length);
        int newHeaderSize  = alignmentBytes + adxHeaderSize + extraHeaderSize + data.Length;
        if (newHeaderSize > short.MaxValue)
            ThrowHelpers.ThrowOffsetTooBigException("Supplied data is too big. ADX header cannot fit.");

        *(ushort*)(origHeaderBytes + 2) = ((ushort)newHeaderSize).AsBigEndian(); // Set new header size.

        // Copy out header.
        outputStream.Write(new Span<byte>(origHeaderBytes, adxHeaderSize + extraHeaderSize)); // Rewrite Original ADX Header
        outputStream.Write(data);                                   // Write ID3 tag.
        outputStream.AddPadding(alignmentBytes - 2, 0x00);          // -2 for CRI header.
        
        // Write out rest of file.
        adxStream.CopyTo(outputStream);
    }
    
    /// <summary>
    /// Gets the data necessary for calculating alignment bytes.
    /// </summary>
    /// <param name="adxData">Pointer to ADX data.</param>
    /// <returns>Raw ADX data.</returns>
    private static AlignmentBytesData GetAlignmentBytesData(byte* adxData)
    {
        var header = (AdxCommonHeader*)adxData;
        var version = header->Version;
        const int versionHeaderOffset = 0x14;

        if (version == 3)
            return GetFromVersion3Header(adxData + versionHeaderOffset);

        if (version == 4)
        {
            var historySize = Math.Max(header->ChannelCount * 4, 8); // Size of extra sample history.
            var postHistoryOffset = 4 + historySize; // (Padding + History). End of v4 non-looping header.

            return GetFromVersion3Header(adxData + versionHeaderOffset + postHistoryOffset);
        }

        static AlignmentBytesData GetFromVersion3Header(byte* versionHeaderPtr)
        {
            return new AlignmentBytesData()
            {
                AlignmentSamples = (*(ushort*)(versionHeaderPtr + 0)).AsBigEndian(),
                LoopStartSample = (*(int*)(versionHeaderPtr + 0x8)).AsBigEndian()
            };
        }

        return default;
    }

    /*
        Code below modified from VGAudio by Alex Barney.
        Under MIT license. 
    */

    /// <param name="adxData">Pointer to the start of the ADX file.</param>
    /// <param name="baseHeaderSize">Size of the pre-existing ADX header.</param>
    private static int CalculateAlignmentBytes(byte* adxData, int baseHeaderSize)
    {
        var adxHeader = (AdxCommonHeader*) adxData;
        var data      = GetAlignmentBytesData(adxData);

        // Start loop frame offset should be a multiple of 0x800
        int startLoopOffset = SampleCountToByteCount(data.LoopStartSample, adxHeader->FrameSize) * adxHeader->ChannelCount + baseHeaderSize + 4;
        var alignmentBytes  = GetNextMultiple(startLoopOffset, DiscSectorAlignment) - startLoopOffset;

        //Version 3 pushes the loop start one block back for every full frame of alignment samples 
        if (adxHeader->Version == 3)
        {
            var samplesPerFrame  = (adxHeader->FrameSize - 2) * 2;
            alignmentBytes += data.AlignmentSamples / samplesPerFrame * DiscSectorAlignment;
        }

        return alignmentBytes;
    }

    private static int SampleCountToByteCount(int sampleCount, int frameSize)
    {
        return SampleCountToNibbleCount(sampleCount, frameSize).DivideBy2RoundUp();
    }

    private static int SampleCountToNibbleCount(int sampleCount, int frameSize)
    {
        int nibblesPerFrame = frameSize * 2;
        int samplesPerFrame = nibblesPerFrame - 4;

        int frames = sampleCount / samplesPerFrame;
        int extraSamples = sampleCount % samplesPerFrame;
        int extraNibbles = extraSamples == 0 ? 0 : extraSamples + 4;

        return nibblesPerFrame * frames + extraNibbles;
    }

    private static int DivideBy2RoundUp(this int value) => (value / 2) + (value & 1);

    private static int GetNextMultiple(int value, int multiple)
    {
        if (multiple <= 0)
            return value;

        if (value % multiple == 0)
            return value;

        return value + multiple - value % multiple;
    }

    private struct AlignmentBytesData
    {
        internal int LoopStartSample;
        internal ushort AlignmentSamples;
    }
}