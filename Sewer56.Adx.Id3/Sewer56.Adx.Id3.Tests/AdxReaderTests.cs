using System;
using System.IO;
using Xunit;

namespace Sewer56.Adx.Id3.Tests;

public class AdxReaderTests
{
    #region Tag Present
    [Fact]
    public void GetId3Tag_WithVersion3_ReturnsTag()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void GetId3Tag_WithVersion4_ReturnsTag()
    {
        throw new NotImplementedException();
    }
    #endregion Tag Present

    #region Loop Present but no Tag
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
        throw new NotImplementedException();
    }

    [Fact]
    public void GetId3Tag_WithNonAdxStream_ThrowsNotAnAdxException()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void GetId3Tag_WithMissingHeaderSizeStream_ThrowsInsufficientDataException()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void GetId3Tag_WithMissingHeaderStream_ThrowsInsufficientDataException()
    {
        throw new NotImplementedException();
    }
    #endregion Exceptions: Stream Overload

    #region Exceptions: Data Ptr Overload
    [Fact]
    public void GetId3Tag_WithNonAdxData_ThrowsNotAnAdxException()
    {
        throw new NotImplementedException();
    }

    [Fact]
    public void GetId3Tag_WithMissingAdxData_ThrowsInsufficientDataException()
    {
        throw new NotImplementedException();
    }
    #endregion Exceptions: Data Ptr Overload
}