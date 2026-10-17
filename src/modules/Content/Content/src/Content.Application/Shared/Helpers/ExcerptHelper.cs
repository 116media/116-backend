namespace _116.Content.Application.Shared.Helpers;

/// <summary>
/// Trims plain text to a maximum length without splitting a word.
/// </summary>
public static class ExcerptHelper
{
    private const int MaxLength = 200;

    /// <summary>
    /// Returns the text cut to at most 200 characters, ending on a word boundary.
    /// </summary>
    /// <param name="text">The source text; null or whitespace yields an empty string.</param>
    /// <returns>The truncated text.</returns>
    public static string Truncate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        string trimmed = text.Trim();

        if (trimmed.Length <= MaxLength)
        {
            return trimmed;
        }

        int boundary = trimmed.LastIndexOf(' ', MaxLength);

        return boundary <= 0 ? trimmed[..MaxLength] : trimmed[..boundary];
    }
}
