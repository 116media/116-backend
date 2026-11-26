using System.Text;
using System.Text.RegularExpressions;

namespace _116.Content.Application.Shared.Helpers;

/// <summary>
/// Provides compile-time slug generation for converting display names
/// into URL-safe, lowercase, hyphen-separated identifiers.
/// Mirrors the frontend <c>slugify</c> and <c>generateSlug</c> utilities.
/// </summary>
public static partial class SlugHelper
{
    private const int MaxSuffixAttempts = 5;
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
        string lowered = stripped.ToLowerInvariant();
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

    /// <summary>
    /// Builds the slug a record is stored under. Mirrors the dashboard's
    /// <c>generateSlug(text, { unique })</c>: <paramref name="unique" /> content always carries a
    /// random suffix, the rest takes the plain slug and only falls back to a suffix when taken.
    /// </summary>
    /// <param name="name">The display name to slugify.</param>
    /// <param name="isTakenAsync">Reports whether a candidate slug is already in use.</param>
    /// <param name="unique">Whether the slug always carries a random suffix.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A slug no existing record holds.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no free slug is found within <see cref="MaxSuffixAttempts" /> attempts.
    /// </exception>
    public static async Task<string> ToAvailableSlugAsync(
        string name,
        Func<string, CancellationToken, Task<bool>> isTakenAsync,
        bool unique = false,
        CancellationToken cancellationToken = default
    )
    {
        string baseSlug = ToSlug(name);
        string firstChoice = unique ? $"{baseSlug}-{GenerateSuffix()}" : baseSlug;

        if (!await isTakenAsync(firstChoice, cancellationToken))
        {
            return firstChoice;
        }

        for (int attempt = 0; attempt < MaxSuffixAttempts; attempt++)
        {
            string candidate = $"{baseSlug}-{GenerateSuffix()}";

            if (!await isTakenAsync(candidate, cancellationToken))
            {
                return candidate;
            }
        }

        throw new InvalidOperationException($"No slug available for '{name}' after {MaxSuffixAttempts} attempts.");
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
    /// Whitespace is spelled out rather than <c>\s</c>, which covers a different set in .NET
    /// than in the browser, so both produce the same slug.
    /// </summary>
    [GeneratedRegex(@"[^a-z0-9\t\n\v\f\r \u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff-]")]
    private static partial Regex NonAlphanumericRegex();

    /// <summary>
    /// Matches one or more whitespace or underscore characters for hyphen replacement.
    /// </summary>
    [GeneratedRegex(@"[\t\n\v\f\r \u00a0\u1680\u2000-\u200a\u2028\u2029\u202f\u205f\u3000\ufeff_]+")]
    private static partial Regex WhitespaceRegex();

    /// <summary>
    /// Matches two or more consecutive hyphens for collapsing.
    /// </summary>
    [GeneratedRegex(@"-{2,}")]
    private static partial Regex RepeatedHyphensRegex();
}
