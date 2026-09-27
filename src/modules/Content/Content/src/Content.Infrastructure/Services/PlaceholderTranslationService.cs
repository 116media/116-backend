using _116.Content.Application.Shared.Services;

namespace _116.Content.Infrastructure.Services;

/// <summary>
/// Temporary no-op stand-in for <see cref="ITranslationService" />, pending integration of a
/// real LLM translation provider. No third-party translation API is configured in this
/// codebase yet — until one is wired up, this implementation returns the source text
/// unchanged so the AI-translation request/review workflow remains fully functional (and
/// fully deterministic for integration tests) without depending on an external call.
/// </summary>
public class PlaceholderTranslationService : ITranslationService
{
    /// <inheritdoc />
    public Task<string> TranslateAsync(
        string text,
        string targetLanguage,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(text);
    }
}
