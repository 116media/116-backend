namespace _116.Shared.Domain.Constants;

/// <summary>
/// The platform's locale vocabulary: the default culture and the cultures it serves.
/// </summary>
public static class LocaleConstants
{
    /// <summary>
    /// Culture used when a user or recipient has no locale of their own.
    /// </summary>
    public const string DefaultLocale = "fr";

    /// <summary>
    /// Cultures the platform renders in, default first.
    /// </summary>
    public static readonly string[] SupportedLocales = ["fr", "en"];

    /// <summary>
    /// Longest locale tag persisted on a profile.
    /// </summary>
    public const int MaxLocaleLength = 5;
}
