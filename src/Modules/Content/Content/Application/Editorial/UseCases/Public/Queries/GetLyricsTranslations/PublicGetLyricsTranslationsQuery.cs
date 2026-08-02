using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations;

/// <summary>
/// Query for retrieving every translation of a lyrics page, across all requested languages.
/// </summary>
/// <param name="LyricsId">The lyrics page whose translations are being listed.</param>
public record PublicGetLyricsTranslationsQuery(Guid LyricsId) : IQuery<PublicGetLyricsTranslationsResult>;

/// <summary>
/// A single translation of a lyrics page into one language.
/// </summary>
/// <param name="Id">The unique identifier of the translation.</param>
/// <param name="Language">ISO 639-1 (or BCP-47) code of the translation's language.</param>
/// <param name="Text">The current published translated text.</param>
/// <param name="Source">Where the current text came from — <c>Ai</c> or <c>Community</c>.</param>
public record TranslationDto(Guid Id, string Language, string Text, string Source);

/// <summary>
/// Result of the <see cref="PublicGetLyricsTranslationsQuery" />.
/// </summary>
/// <param name="Translations">Every translation of the lyrics page, one per requested language.</param>
public record PublicGetLyricsTranslationsResult(IReadOnlyList<TranslationDto> Translations);
