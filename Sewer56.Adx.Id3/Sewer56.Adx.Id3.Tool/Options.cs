using CommandLine;

namespace Sewer56.Adx.Id3.Tool;

[Verb("copy", HelpText = "Copies an id3 tag from a non-ADX file to an ADX file.")]
internal class CopyOptions
{
    [Option(Required = true, HelpText = "The file to copy tags from.")]
    public string Source { get; internal set; }

    [Option(Required = true, HelpText = "The ADX file to add the tags to.")]
    public string SourceAdx { get; internal set; }

    [Option(Required = false, HelpText = "Where to save the modified ADX file. If not specified, will equal to SourceAdx.")]
    public string Destination { get; internal set; }

    [Option(Required = false, HelpText = "Name of file used for album art cover, without extension.", Default = "Picture")]
    public string PictureName { get; internal set; }
}

[Verb("view", HelpText = "Reads a file and prints contents of the id3 tag. Supports ADX and some other formats.")]
internal class ViewOptions
{
    [Option(Required = true, HelpText = "The music file to read tag from.")]
    public string Source { get; internal set; }
}

