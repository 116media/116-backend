using System.Globalization;

namespace _116.Shared.Application.Localization;

/// <summary>
/// Resolves the culture argument mail and notification ports take, from the ambient request
/// culture, so every caller localizes the same way.
/// </summary>
public static class EmailCulture
{
    /// <summary>
    /// The two-letter language code of the current request culture.
    /// </summary>
    /// <returns>The code the template resources are resolved with (e.g. "en", "fr").</returns>
    public static string Current()
    {
        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
    }
}
