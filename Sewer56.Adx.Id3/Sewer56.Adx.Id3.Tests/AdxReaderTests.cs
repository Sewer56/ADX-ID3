using System;
using System.IO;
using Sewer56.Adx.Id3.Exceptions;
using Xunit;

namespace Sewer56.Adx.Id3.Tests;

public class AdxReaderTests
{
    #region Loop Present but no Tag
    [Fact]
    public unsafe void GetId3Tag_WithVersion3_WithNoTag_WithPointer_ReturnsNull()
    {
        var bytes = File.ReadAllBytes(Assets.Version3Loop);
        fixed (byte* bytesPtr = &bytes[0])
        {
            Assert.Null(AdxReader.GetId3Tag(bytesPtr, bytes.Length));
        }
    }

    [Fact]
    public void GetId3Tag_WithVersion3_WithNoTag_ReturnsNull()
    {
        using var fileStream = new FileStream(Assets.Version3Loop, FileMode.Open);
        Assert.Null(AdxReader.GetId3Tag(fileStream));
    }

    [Fact]
    public void GetId3Tag_WithVersion4_WithNoTag_ReturnsNull()
    {
        using var fileStream = new FileStream(Assets.Version4Loop, FileMode.Open);
        Assert.Null(AdxReader.GetId3Tag(fileStream));
    }
    #endregion Loop Present but no Tag

    #region No Loop in ADX Header
    [Fact]
    public void GetId3Tag_WithNoLoop_WithVersion3_ReturnsNull()
    {
        using var fileStream = new FileStream(Assets.Version3NoLoop, FileMode.Open);
        Assert.Null(AdxReader.GetId3Tag(fileStream));
    }

    [Fact]
    public void GetId3Tag_WithNoLoop_WithVersion4_ReturnsNull()
    {
        using var fileStream = new FileStream(Assets.Version4NoLoop, FileMode.Open);
        Assert.Null(AdxReader.GetId3Tag(fileStream));
    }
    #endregion No Loop in ADX Header

    #region Exceptions: Stream Overload
    [Fact]
    public void GetId3Tag_WithEmptyStream_ThrowsInsufficientDataException()
    {
        using var adxStream = new MemoryStream();
        using var outputStream = new MemoryStream();

        Assert.Throws<InsufficientDataException>(() => AdxReader.GetId3Tag(adxStream));
    }

    [Fact]
    public void GetId3Tag_WithNonAdxStream_ThrowsNotAnAdxException()
    {
        using var adxStream = new MemoryStream();
        adxStream.Write(new byte[] { 0x7F, 0x00, 0x00 });
        adxStream.Position = 0;

        using var outputStream = new MemoryStream();

        Assert.Throws<NotAnAdxException>(() => AdxReader.GetId3Tag(adxStream));
    }

    [Fact]
    public void GetId3Tag_WithMissingHeaderSizeStream_ThrowsInsufficientDataException()
    {
        using var adxStream = new MemoryStream();
        adxStream.Write(new byte[] { 0x80, 0x00 });
        adxStream.Position = 0;

        using var outputStream = new MemoryStream();
        Assert.Throws<InsufficientDataException>(() => AdxReader.GetId3Tag(adxStream));
    }

    [Fact]
    public void GetId3Tag_WithMissingHeaderStream_ThrowsInsufficientDataException()
    {
        using var adxStream = new MemoryStream();
        adxStream.Write(new byte[] { 0x80, 0x00, 0x00, 0xFF });
        adxStream.Position = 0;

        using var outputStream = new MemoryStream();
        Assert.Throws<InsufficientDataException>(() => AdxReader.GetId3Tag(adxStream));
    }
    #endregion Exceptions: Stream Overload

    #region Exceptions: Data Ptr Overload
    [Fact]
    public unsafe void GetId3Tag_WithNonAdxData_ThrowsNotAnAdxException()
    {
        using var adxStream = new MemoryStream();
        adxStream.Write(new byte[] { 0x7F, 0x00, 0x00, 0x00 });
        adxStream.Position = 0;

        fixed (byte* bytePtr = &adxStream.GetBuffer()[0])
        {
            try
            {
                AdxReader.GetId3Tag(bytePtr, (int)adxStream.Length);
                Assert.True(false);
            }
            catch (NotAnAdxException ex)
            {
                /* Should throw this. */
            }
            catch (Exception ex)
            {
                Assert.True(false);
            }
        }
    }

    [Fact]
    public unsafe void GetId3Tag_WithMissingAdxData_ThrowsInsufficientDataException()
    {
        using var adxStream = new MemoryStream();
        adxStream.Write(new byte[] { 0x80, 0x00, 0x00, 0xFF });
        adxStream.Position = 0;

        fixed (byte* bytePtr = &adxStream.GetBuffer()[0])
        {
            try
            {
                AdxReader.GetId3Tag(bytePtr, (int)adxStream.Length);
                Assert.True(false);
            }
            catch (InsufficientDataException ex)
            {
                /* Should throw this. */
            }
            catch (Exception ex)
            {
                Assert.True(false);
            }
        }
    }
    #endregion Exceptions: Data Ptr Overload
}