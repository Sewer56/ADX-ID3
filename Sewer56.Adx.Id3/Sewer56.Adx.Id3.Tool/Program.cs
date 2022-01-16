using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using Sewer56.Adx.Id3.Tool.Utilities;
using TagLib;
using TagLib.Id3v2;
using static Sewer56.Adx.Id3.Tool.Utilities.TagLibHelpers;
using File = System.IO.File;
using Tag = TagLib.Id3v2.Tag;

namespace Sewer56.Adx.Id3.Tool;

public class Program
{
    public const string LinkMimeType = "-->";

    static void Main(string[] args)
    {
        var parser = new Parser(with =>
        {
            with.AutoHelp = true;
            with.CaseSensitive = false;
            with.CaseInsensitiveEnumValues = true;
            with.EnableDashDash = true;
            with.HelpWriter = null;
        });

        // Setup ID3 Settings
        Tag.ForceDefaultEncoding = true;
        Tag.DefaultEncoding = StringType.UTF8;
        Tag.ForceDefaultVersion = true;
        Tag.DefaultVersion = 4;

        var parserResult = parser.ParseArguments<ViewOptions, CopyOptions>(args);
        parserResult.WithParsed<ViewOptions>(ViewTags)
            .WithParsed<CopyOptions>(CopyOptions)
            .WithNotParsed(errs => HandleParseError(parserResult, errs));
    }

    private static void CopyOptions(CopyOptions copyOptions)
    {
        if (string.IsNullOrEmpty(copyOptions.Destination))
            copyOptions.Destination = copyOptions.SourceAdx;

        Directory.CreateDirectory(Path.GetDirectoryName(copyOptions.Destination));

        // Get tag
        TagLib.Tag otherFileTag = null;
        var id3Tag = IsAdxExtension(copyOptions.Source) ? GetId3FromAdx(copyOptions.Source) : GetId3FromOtherFile(copyOptions.Source, out otherFileTag);
        
        // Copy non-embedded pictures in existing copied tag.
        foreach (var picture in id3Tag.Pictures)
        {
            if (picture.MimeType != LinkMimeType) 
                continue;

            var encoding = GetEncodingForPicture(picture);

            try
            {
                var relativePath = picture.Data.ToString(encoding);
                var originalFilePath = Path.Combine(Path.GetDirectoryName(copyOptions.Source), relativePath);
                var targetFilePath = Path.Combine(Path.GetDirectoryName(copyOptions.Destination), relativePath);
                File.Copy(originalFilePath, targetFilePath, true);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        // Copy pictures from tag to new tag.
        if (otherFileTag != null)
        {
            var frames = new List<AttachmentFrame>();
            foreach (var picture in otherFileTag.Pictures)
            {
                if (!MimeTypeHelper.MimeTypeToExtension.TryGetValue(picture.MimeType, out string extension)) 
                    continue;

                // Write image to disk.
                var fileName = $"{copyOptions.PictureName}_{picture.Type}.{extension}";
                var filePath = Path.Combine(Path.GetDirectoryName(copyOptions.Destination), fileName);
                File.WriteAllBytes(filePath, picture.Data.Data);

                // Add image
                frames.Add(new AttachmentFrame()
                {
                    MimeType = LinkMimeType,
                    Data = fileName,
                    Type = picture.Type,
                });
            }

            id3Tag.Pictures = frames.ToArray();
        }

        // Create Tag
        byte[] sourceTagBytes = id3Tag.Render().Data;

        // Make ADX with tag. 
        // In case of error, we write to memory first, then to file.
        using var originalAdx       = new MemoryStream(File.ReadAllBytes(copyOptions.SourceAdx));
        using var newAdx            = new MemoryStream((int)originalAdx.Length + sourceTagBytes.Length + AdxWriter.DiscSectorAlignment);
        AdxWriter.WriteAdx(originalAdx, newAdx, sourceTagBytes);

        // In case of error, we would have thrown exception by now.
        using var newAdxStream = new FileStream(copyOptions.Destination, FileMode.Create);
        newAdx.Position = 0;
        newAdx.CopyTo(newAdxStream);
    }

    private static StringType GetEncodingForPicture(IPicture picture)
    {
        if (picture is AttachmentFrame frame)
            return frame.TextEncoding;

        return StringType.UTF8;
    }

    private static void ViewTags(ViewOptions viewOptions)
    {
        Tag tag = IsAdxExtension(viewOptions.Source) ? GetId3FromAdx(viewOptions.Source) : GetId3FromOtherFile(viewOptions.Source, out _);
        Console.WriteLine(TagLibHelpers.ToString(tag));
    }

    private static bool IsAdxExtension(string filePath)
    {
        return Path.GetExtension(filePath).Equals(".adx", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Errors or --help or --version.
    /// </summary>
    static void HandleParseError(ParserResult<object> options, IEnumerable<Error> errs)
    {
        var helpText = HelpText.AutoBuild(options, help =>
        {
            help.Copyright = "Created by Sewer56, licensed under MIT License";
            help.AutoHelp = false;
            help.AutoVersion = false;
            help.AddDashesToOption = true;
            help.AddEnumValuesToHelpText = true;
            help.AdditionalNewLineAfterOption = true;
            return HelpText.DefaultParsingErrorsHandler(options, help);
        }, example => example, true);

        Console.WriteLine(helpText);
    }
}
