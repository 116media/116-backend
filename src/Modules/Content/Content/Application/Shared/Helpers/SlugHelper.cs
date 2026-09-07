using System.Text;
using System.Text.RegularExpressions;

namespace _116.Core.Application.Shared.Helpers;

/// <summary>
/// Provides compile-time slug generation for converting display names
/// into URL-safe, lowercase, hyphen-separated identifiers.
/// Mirrors the frontend <c>slugify</c> and <c>generateSlug</c> utilities.
/// </summary>
public static partial class SlugHelper
{
    private const int UniqueSuffixLength = 8;
    private const string SuffixChars = "abcdefghijklmnopqrstuvwxyz0123456789";

    /// <summary>
    /// Converts a display name to a URL-safe base slug.
    /// Strips diacritics, lowercases, trims whitespace, removes non-alphanumeric
    /// characters, replaces whitespace with hyphens, and collapses repeated hyphens.
    /// Mirrors the frontend <c>slugify(text)</c> function exactly.
    /// </summary>
    /// <param name="name">The display name to slugify (e.g. "Fally Ipupa").</param>
    /// <returns>
    /// A lowercase hyphen-separated slug (e.g. "fally-ipupa").
    /// </returns>
    public static string ToSlug(string name)
    {
        string normalized = name.Normalize(NormalizationForm.FormD);
        string stripped = DiacriticsRegex().Replace(normalized, string.Empty);
        string lowered = stripped.ToLowerInvariant().Trim();
        string cleaned = NonAlphanumericRegex().Replace(lowered, string.Empty);
        string hyphenated = WhitespaceRegex().Replace(cleaned, "-");
        return RepeatedHyphensRegex().Replace(hyphenated, "-").Trim('-');
    }

    /// <summary>
    /// Converts a display name to a URL-safe slug with a random alphanumeric suffix
    /// to guarantee uniqueness.
    /// Mirrors the frontend <c>generateSlug(text, &#123; unique: true &#125;)</c> function.
    /// </summary>
    /// <param name="name">The display name to slugify.</param>
    /// <returns>
    /// A slug with an 8-character random suffix (e.g. "fally-ipupa-a3x9k2m7").
    /// </returns>
    public static string ToUniqueSlug(string name)
    {
        string baseSlug = ToSlug(name);
        string suffix = GenerateSuffix();
        return $"{baseSlug}-{suffix}";
    }

    private static string GenerateSuffix()
    {
        var result = new char[UniqueSuffixLength];
        for (int i = 0; i < UniqueSuffixLength; i++)
        {
            result[i] = SuffixChars[Random.Shared.Next(SuffixChars.Length)];
        }
        return new string(result);
    }

    /// <summary>
    /// Matches Unicode combining diacritical marks for removal after NFD normalization.
    /// </summary>
    [GeneratedRegex(@"[\u0300-\u036f]")]
    private static partial Regex DiacriticsRegex();

    /// <summary>
    /// Matches any character that is not a lowercase letter, digit, whitespace, or hyphen.
    /// </summary>
    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex NonAlphanumericRegex();

    /// <summary>
    /// Matches one or more whitespace or underscore characters for hyphen replacement.
    /// </summary>
    [GeneratedRegex(@"[\s_]+")]
    private static partial Regex WhitespaceRegex();

    /// <summary>
    /// Matches two or more consecutive hyphens for collapsing.
    /// </summary>
    [GeneratedRegex(@"-{2,}")]
    private static partial Regex RepeatedHyphensRegex();
}
