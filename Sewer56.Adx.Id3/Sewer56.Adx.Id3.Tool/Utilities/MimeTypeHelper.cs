using System.Collections.Generic;
using System.Reflection;
using TagLib;

namespace Sewer56.Adx.Id3.Tool.Utilities;

internal class MimeTypeHelper
{
    public static readonly Dictionary<string, string> MimeTypeToExtension = new Dictionary<string, string>();

    static MimeTypeHelper()
    {
        var mimeTypes = (List<SupportedMimeType>) typeof(SupportedMimeType).GetField("mimetypes", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        foreach (var mimeType in mimeTypes)
        {
            if (string.IsNullOrEmpty(mimeType.Extension))
            {
                var index = mimeType.MimeType.IndexOf('/');
                if (index >= 0 && (index + 1) <= mimeType.MimeType.Length)
                    MimeTypeToExtension[mimeType.MimeType] = mimeType.MimeType.Substring(index + 1);
            }
            else
                MimeTypeToExtension[mimeType.MimeType] = mimeType.Extension;
        }
    }
}