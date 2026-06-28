using System.Globalization;

namespace _116.Core.Application.Shared.Helpers;

/// <summary>
/// Pure helpers for working with <c>#RRGGBB</c> colors: validating/normalizing a
/// hex string and deriving an accessible foreground (text) color for a given
/// background using the WCAG 2.x relative-luminance contrast rule.
/// </summary>
/// <remarks>
/// The foreground is always pure black (<c>#000000</c>) or pure white
/// (<c>#FFFFFF</c>) so the result is guaranteed readable regardless of the
/// background — this is what resolves the "yellow background with white text"
/// problem (yellow is light, so it gets black text). The class is intentionally
/// dependency-free so it can run at upload time and be unit-tested in isolation.
/// </remarks>
public static class ColorContrastHelper
{
    /// <summary>
    /// Pure-black foreground, chosen for light backgrounds.
    /// </summary>
    public const string Black = "#000000";

    /// <summary>
    /// Pure-white foreground, chosen for dark backgrounds.
    /// </summary>
    public const string White = "#FFFFFF";

    /// <summary>
    /// WCAG luminance flip threshold. Backgrounds with a relative luminance above
    /// this value are treated as "light" (use black text); at or below, "dark"
    /// (use white text). 0.179 is the standard crossover that maximizes the
    /// minimum contrast ratio against both black and white.
    /// </summary>
    private const double LuminanceThreshold = 0.179;

    /// <summary>
    /// Validates and normalizes a color to canonical upper-case <c>#RRGGBB</c>.
    /// </summary>
    /// <param name="hex">The candidate color, with or without a leading <c>#</c>.</param>
    /// <returns>
    /// The normalized <c>#RRGGBB</c> string, or <c>null</c> when the input is not
    /// a valid 6-digit hex color.
    /// </returns>
    public static string? Normalize(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            return null;
        }

        string trimmed = hex.Trim();
        string digits = trimmed.StartsWith('#') ? trimmed[1..] : trimmed;

        if (digits.Length != 6)
        {
            return null;
        }

        foreach (char c in digits)
        {
            if (!Uri.IsHexDigit(c))
            {
                return null;
            }
        }

        return $"#{digits.ToUpperInvariant()}";
    }

    /// <summary>
    /// Computes the accessible foreground (text) color for a background.
    /// </summary>
    /// <param name="backgroundHex">The background color as <c>#RRGGBB</c> (with or without a leading <c>#</c>).</param>
    /// <returns>
    /// <see cref="Black" /> for light backgrounds and <see cref="White" /> for dark
    /// backgrounds, or <c>null</c> when <paramref name="backgroundHex" /> is not a
    /// valid hex color.
    /// </returns>
    public static string? ForegroundFor(string? backgroundHex)
    {
        string? normalized = Normalize(backgroundHex);
        if (normalized is null)
        {
            return null;
        }

        double r = ParseChannel(normalized, 1);
        double g = ParseChannel(normalized, 3);
        double b = ParseChannel(normalized, 5);

        double luminance = (0.2126 * Linearize(r)) + (0.7152 * Linearize(g)) + (0.0722 * Linearize(b));

        return luminance > LuminanceThreshold ? Black : White;
    }

    /// <summary>
    /// Parses a single 8-bit channel from a normalized <c>#RRGGBB</c> string into
    /// the sRGB range <c>[0, 1]</c>.
    /// </summary>
    /// <param name="normalizedHex">A normalized <c>#RRGGBB</c> color.</param>
    /// <param name="offset">The starting index of the channel's two hex digits.</param>
    /// <returns>The channel value scaled to <c>[0, 1]</c>.</returns>
    private static double ParseChannel(string normalizedHex, int offset)
    {
        int value = int.Parse(normalizedHex.AsSpan(offset, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        return value / 255.0;
    }

    /// <summary>
    /// Linearizes an sRGB channel (gamma expansion) as defined by WCAG 2.x, the
    /// prerequisite for computing relative luminance.
    /// </summary>
    /// <param name="channel">An sRGB channel value in <c>[0, 1]</c>.</param>
    /// <returns>The linearized channel value in <c>[0, 1]</c>.</returns>
    private static double Linearize(double channel)
    {
        return channel <= 0.03928 ? channel / 12.92 : Math.Pow((channel + 0.055) / 1.055, 2.4);
    }
}
