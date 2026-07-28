using System.Globalization;

namespace _116.Mailer.Contracts.Application;

/// <summary>
/// Resolves the culture argument for <see cref="IMailer.EnqueueAsync" /> from
/// the ambient request culture, so every consumer localizes the same way.
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
