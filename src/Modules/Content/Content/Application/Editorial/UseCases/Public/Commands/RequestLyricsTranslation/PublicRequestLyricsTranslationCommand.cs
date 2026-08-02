using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.RequestLyricsTranslation;

/// <summary>
/// Command to request an AI-generated translation of a lyrics page into a given language.
/// Idempotent — if a translation already exists for the <c>(LyricsId, Language)</c> pair, the
/// existing translation is returned without a second AI generation call.
/// </summary>
/// <param name="LyricsId">The lyrics page to translate.</param>
/// <param name="Language">ISO 639-1 (or BCP-47) code of the language to translate into.</param>
public record PublicRequestLyricsTranslationCommand(Guid LyricsId, string Language)
    : ICommand<PublicRequestLyricsTranslationResult>;

/// <summary>
/// Result of the <see cref="PublicRequestLyricsTranslationCommand" />.
/// </summary>
/// <param name="Text">The translated text, either freshly generated or previously stored.</param>
/// <param name="Source">
/// Where the translated text came from — <c>Ai</c> for a fresh or previously stored AI
/// generation, or <c>Community</c> if an accepted community revision has since superseded it.
/// </param>
public record PublicRequestLyricsTranslationResult(string Text, string Source);
