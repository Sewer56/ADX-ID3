using System;
using System.IO;
using Sewer56.Adx.Id3.Tests.Misc;
using Xunit;

namespace Sewer56.Adx.Id3.Tests;

/// <summary>
/// Tests combining both reader and writer.
/// </summary>
public class CombinedTests
{
    #region Data Write and Retrieval
    [Fact]
    public void WriteAndReadAdx_WithVersion3_RetrievesCorrectData()
    {
        // Create new ADX.
        using var adxStream = new FileStream(Assets.Version3NoLoop, FileMode.Open);
        using var outputStream = new MemoryStream();

        AssertRetrievesCorrectData(adxStream, outputStream);
    }

    [Fact]
    public void WriteAndReadAdx_WithVersion4_RetrievesCorrectData()
    {
        // Create new ADX.
        using var adxStream = new FileStream(Assets.Version4NoLoop, FileMode.Open);
        using var outputStream = new MemoryStream();

        AssertRetrievesCorrectData(adxStream, outputStream);
    }

    [Fact]
    public void WriteAndReadAdx_WithVersion3Looped_RetrievesCorrectData()
    {
        // Create new ADX.
        using var adxStream    = new FileStream(Assets.Version3Loop, FileMode.Open);
        using var outputStream = new MemoryStream();

        AssertRetrievesCorrectData(adxStream, outputStream);
    }

    [Fact]
    public void WriteAndReadAdx_WithVersion4Looped_RetrievesCorrectData()
    {
        // Create new ADX.
        using var adxStream    = new FileStream(Assets.Version4Loop, FileMode.Open);
        using var outputStream = new MemoryStream();

        AssertRetrievesCorrectData(adxStream, outputStream);
    }

    private static void AssertRetrievesCorrectData(Stream adxStream, Stream outputStream)
    {
        var originalTag = TestUtils.CreateRandomId3Tag();

        AdxWriter.WriteAdx(adxStream, outputStream, originalTag);
        adxStream.Position = 0;
        outputStream.Position = 0;

        // Read new ADX.
        var id3Bytes = AdxReader.GetId3Tag(outputStream);

        Assert.Equal(originalTag, id3Bytes);
    }


    #endregion Data Write and Retrieval
}