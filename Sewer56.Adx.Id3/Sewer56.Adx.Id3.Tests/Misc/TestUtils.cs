using Bogus;
using TagLib;
using TagLib.Id3v2;
using Tag = TagLib.Id3v2.Tag;

namespace Sewer56.Adx.Id3.Tests.Misc;

public static class TestUtils
{
    private static Faker<Tag> _tagFaker = new();

    static TestUtils()
    {
        Tag.ForceDefaultEncoding = true;
        Tag.DefaultEncoding = StringType.UTF8;
        Tag.ForceDefaultVersion = true;
        Tag.DefaultVersion = 4;

        _tagFaker.RuleFor(x => x.Performers, (x) =>
        {
            var performers = new string[x.Random.Int(1, 3)];
            for (int y = 0; y < performers.Length; y++)
                performers[y] = x.Name.FirstName();

            return performers;
        }); 

        _tagFaker.RuleFor(x => x.Title, x => x.Commerce.Product());
        _tagFaker.RuleFor(x => x.Description, x => x.Commerce.ProductDescription());
    }

    /// <summary>
    /// Creates a randomized ID3 tag.
    /// </summary>
    public static byte[] CreateRandomId3Tag()
    {
        var fakeTag = _tagFaker.Generate();
        fakeTag.Flags |= HeaderFlags.FooterPresent;
        return fakeTag.Render().Data;
    }
}