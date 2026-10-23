using _116.Content.Application.Shared.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Shared.Helpers;

/// <summary>
/// Pins <see cref="SlugHelper.ToSlug" /> to the dashboard's <c>slugify</c>. Each expectation is
/// the output the dashboard utility produced for that input when run directly.
/// </summary>
public class SlugParityTests
{
    public static TheoryData<string, string> DashboardOutputs =>
        new()
        {
            { "Fally Ipupa", "fally-ipupa" },
            { "116 Le Focus", "116-le-focus" },
            { "  Hello   World  ", "hello-world" },
            { "cafe & creme", "cafe-creme" },
            { "Clip Video - Youtube", "clip-video-youtube" },
            { "Deja Vu \u2014 Ete 2026", "deja-vu-ete-2026" },
            { "a__b", "ab" },
            { "--lead--trail--", "lead-trail" },
            { "\u00c0\u00c9\u00ce\u00d5\u00dc \u00c7", "aeiou-c" },
            { "\u0130stanbul", "istanbul" },
            { "\u1e9eTRASSE", "trasse" },
            { "emoji \ud83c\udfb5 test", "emoji-test" },
            { "tab\u0009sep", "tab-sep" },
            { "nl\u000asep", "nl-sep" },
            { "cr\u000dsep", "cr-sep" },
            { "nbsp\u00a0sep", "nbsp-sep" },
            { "nel\u0085sep", "nelsep" },
            { "bom\ufeffsep", "bom-sep" },
            { "ideographic\u3000sep", "ideographic-sep" },
            { "zwsp\u200bsep", "zwspsep" },
            { "\u00df sharp", "sharp" },
            { "100% pure", "100-pure" },
            { "a.b.c", "abc" },
            { "multi---hyphen", "multi-hyphen" },
            { "", "" },
            { "   ", "" },
            { "---", "" },
            { "\u00c5NGSTR\u00d6M", "angstrom" },
            { "vert\u000bsep", "vert-sep" },
            { "formfeed\u000csep", "formfeed-sep" },
            { "ls\u2028sep", "ls-sep" },
        };

    [Theory]
    [MemberData(nameof(DashboardOutputs))]
    public void ToSlug_ShouldMatchTheDashboardSlugify(string input, string dashboardSlug)
    {
        SlugHelper.ToSlug(input).Should().Be(dashboardSlug);
    }
}
