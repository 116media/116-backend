using System.Net;
using System.Text.RegularExpressions;

namespace _116.Content.Application.Shared.Helpers;

/// <summary>
/// Turns a body of rich text into a plain-text excerpt: markup stripped, entities
/// decoded, whitespace collapsed, then cut to a maximum length on a word boundary.
/// </summary>
public static partial class ExcerptHelper
{
    private const int MaxLength = 200;

    /// <summary>
    /// Matches one HTML tag. Video descriptions and article headlines are authored as
    /// rich text, so a raw excerpt would reach the client as markup.
    /// </summary>
    [GeneratedRegex(@"<[^>]+>", RegexOptions.None, matchTimeoutMilliseconds: 200)]
    private static partial Regex TagPattern { get; }

    /// <summary>
    /// Matches a run of whitespace, including the newlines a stripped block tag leaves.
    /// </summary>
    [GeneratedRegex(@"\s+", RegexOptions.None, matchTimeoutMilliseconds: 200)]
    private static partial Regex WhitespacePattern { get; }

    /// <summary>
    /// Returns the text as plain prose, cut to at most 200 characters on a word boundary.
    /// </summary>
    /// <param name="text">The source text, which may carry markup; null or whitespace yields an empty string.</param>
    /// <returns>The plain-text excerpt.</returns>
    public static string Truncate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        // A tag is replaced by a space rather than removed, so "<p>a</p><p>b</p>" does
        // not come back as "ab".
        string stripped = TagPattern.Replace(text, " ");
        string decoded = WebUtility.HtmlDecode(stripped);
        string trimmed = WhitespacePattern.Replace(decoded, " ").Trim();

        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        if (trimmed.Length <= MaxLength)
        {
            return trimmed;
        }

        int boundary = trimmed.LastIndexOf(' ', MaxLength);

        return boundary <= 0 ? trimmed[..MaxLength] : trimmed[..boundary];
    }
}
