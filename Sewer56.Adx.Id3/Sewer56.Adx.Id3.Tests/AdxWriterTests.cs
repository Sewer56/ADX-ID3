using System;
using System.IO;
using Sewer56.Adx.Id3.Exceptions;
using Standart.Hash.xxHash;
using Xunit;

namespace Sewer56.Adx.Id3.Tests;

public class AdxWriterTests
{
    #region Sanity Checks
    [Fact]
    public void WriteAdx_WithVersion3Looped_WithNoData_MatchesVGAudio()
    {
        WriteAdx_WithVersionLooped_WithNoData_MatchesVGAudio(new FileStream(Assets.Version3Loop, FileMode.Open));
    }

    [Fact]
    public void WriteAdx_WithVersion4Looped_WithNoData_MatchesVGAudio()
    {
        WriteAdx_WithVersionLooped_WithNoData_MatchesVGAudio(new FileStream(Assets.Version4Loop, FileMode.Open));
    }
    
    private void WriteAdx_WithVersionLooped_WithNoData_MatchesVGAudio(Stream adxStream)
    {
        using var memoryStream = new MemoryStream();

        AdxWriter.WriteAdx(adxStream, memoryStream, Span<byte>.Empty);

        adxStream.Position = 0;
        memoryStream.Position = 0;

        Assert.Equal(adxStream.Length, memoryStream.Length);
        Assert.Equal(xxHash64.ComputeHash(adxStream), xxHash64.ComputeHash(memoryStream));
    }
    #endregion Tag Present

    #region Exceptions: Stream Overload
    [Fact]
    public void WriteAdx_WithEmptyStream_ThrowsInsufficientDataException()
    {
        using var adxStream = new MemoryStream();
        using var outputStream = new MemoryStream();

        Assert.Throws<InsufficientDataException>(() => AdxWriter.WriteAdx(adxStream, outputStream, Span<byte>.Empty));
    }

    [Fact]
    public void WriteAdx_WithNonAdxStream_ThrowsNotAnAdxException()
    {
        using var adxStream = new MemoryStream();
        adxStream.Write(new byte[] { 0x7F, 0x00, 0x00 });
        adxStream.Position = 0;

        using var outputStream = new MemoryStream();
        Assert.Throws<NotAnAdxException>(() => AdxWriter.WriteAdx(adxStream, outputStream, Span<byte>.Empty));
    }

    [Fact]
    public void WriteAdx_WithMissingHeaderSizeStream_ThrowsInsufficientDataException()
    {
        using var adxStream = new MemoryStream();
        adxStream.Write(new byte[] { 0x80, 0x00 });
        adxStream.Position = 0;

        using var outputStream = new MemoryStream();

        Assert.Throws<InsufficientDataException>(() => AdxWriter.WriteAdx(adxStream, outputStream, Span<byte>.Empty));
    }

    [Fact]
    public void WriteAdx_WithMissingHeaderStream_ThrowsInsufficientDataException()
    {
        using var adxStream = new MemoryStream();
        adxStream.Write(new byte[] { 0x80, 0x00, 0x00, 0xFF });
        adxStream.Position = 0;

        using var outputStream = new MemoryStream();

        Assert.Throws<InsufficientDataException>(() => AdxWriter.WriteAdx(adxStream, outputStream, Span<byte>.Empty));
    }
    #endregion Exceptions: Stream Overload
}