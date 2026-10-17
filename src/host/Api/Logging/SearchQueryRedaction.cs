using System.Text.RegularExpressions;

namespace _116.Api.Logging;

/// <summary>
/// Redacts the search term from a logged request path so reader queries are never
/// persisted in the log store. The route and every other parameter are kept.
/// </summary>
public static partial class SearchQueryRedaction
{
    private const string Placeholder = "q=[redacted]";

    /// <summary>
    /// Returns the request path with the value of the <c>q</c> parameter replaced.
    /// </summary>
    /// <param name="path">The request path, with or without a query string.</param>
    /// <returns>The path safe to log.</returns>
    public static string Redact(string? path)
    {
        return string.IsNullOrEmpty(path) ? string.Empty : QueryTermPattern().Replace(path, Placeholder);
    }

    [GeneratedRegex(@"(?<=[?&])q=[^&]*", RegexOptions.IgnoreCase)]
    private static partial Regex QueryTermPattern();
}
