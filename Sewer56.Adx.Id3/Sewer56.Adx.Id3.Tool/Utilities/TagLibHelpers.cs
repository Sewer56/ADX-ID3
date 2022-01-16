using System;
using System.IO;
using System.Text;
using TagLib;

namespace Sewer56.Adx.Id3.Tool.Utilities;

internal static class TagLibHelpers
{
    public static string ToString(Tag tag)
    {
        var builder = new StringBuilder(1024);

        if (!string.IsNullOrEmpty(tag.Title))
            builder.AppendLine($"Title: {tag.Title}");

        if (tag.Performers.Length >= 1)
            builder.AppendLine($"Artist(s): {string.Join(",", tag.Performers)}");

        if (!string.IsNullOrEmpty(tag.Album))
            builder.AppendLine($"Album: {tag.Album}");

        if (tag.AlbumArtists.Length >= 1)
            builder.AppendLine($"Album Artist(s): {string.Join(",", tag.AlbumArtists)}");

        if (tag.Year > 0)
            builder.AppendLine($"Year: {tag.Year}");

        if (tag.Genres.Length >= 1)
            builder.AppendLine($"Genre(s): {string.Join(",", tag.Genres)}");

        if (tag.Track > 0)
            builder.AppendLine($"Track: {tag.Track}");

        if (tag.Disc > 0)
            builder.AppendLine($"Disc No: {tag.Disc}");

        return builder.ToString();
    }

    public static TagLib.Id3v2.Tag GetId3FromOtherFile(string filePath, out TagLib.Tag originalTag)
    {
        originalTag = TagLib.File.Create(filePath).Tag;
        if (originalTag == null)
            throw new Exception("Tag Not Found");

        var id3Tag = new TagLib.Id3v2.Tag();
        originalTag.CopyTo(id3Tag, true);
        return id3Tag;
    }

    public static TagLib.Id3v2.Tag GetId3FromAdx(string filePath)
    {
        using var fileStream = new FileStream(filePath, FileMode.Open);
        var id3Bytes = AdxReader.GetId3Tag(fileStream);

        if (id3Bytes == null)
            throw new Exception("ID3 Tag Not Found");

        return new TagLib.Id3v2.Tag(new ByteVector(id3Bytes, id3Bytes.Length));
    }
}