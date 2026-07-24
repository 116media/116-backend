namespace _116.Content.Application.Shared.Services;

/// <summary>
/// Port for translating lyrics text into another language. Kept behind an interface so the
/// concrete LLM provider is swappable and mockable in tests — the same dependency-inversion
/// shape as <c>IUserLookupService</c>/<c>IFileRepository</c> elsewhere in this module.
/// </summary>
/// <remarks>
/// Which provider is used and how it is configured (API key, endpoint, model) is an
/// infrastructure concern outside the scope of the translation review workflow — only this
/// port and its consumption by <c>RequestLyricsTranslationHandler</c> belong to the
/// application layer.
/// </remarks>
public interface ITranslationService
{
    /// <summary>
    /// Translates the given text into the requested target language.
    /// </summary>
    /// <param name="text">The source text to translate.</param>
    /// <param name="targetLanguage">ISO 639-1 (or BCP-47) code of the language to translate into.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The translated text.</returns>
    Task<string> TranslateAsync(string text, string targetLanguage, CancellationToken cancellationToken = default);
}
