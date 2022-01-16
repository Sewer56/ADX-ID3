using System;
using System.IO;
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
        throw new NotImplementedException();
    }

    [Fact]
    public void WriteAdx_WithNonAdxStream_ThrowsNotAnAdxException()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void WriteAdx_WithMissingHeaderSizeStream_ThrowsInsufficientDataException()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void WriteAdx_WithMissingHeaderStream_ThrowsInsufficientDataException()
    {
        throw new NotImplementedException();
    }
    #endregion Exceptions: Stream Overload

    #region Exceptions: Data Ptr Overload
    [Fact]
    public void WriteAdx_WithNonAdxData_ThrowsNotAnAdxException()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void WriteAdx_WithMissingAdxData_ThrowsInsufficientDataException()
    {
        throw new NotImplementedException();
    }
    #endregion Exceptions: Data Ptr Overload
}