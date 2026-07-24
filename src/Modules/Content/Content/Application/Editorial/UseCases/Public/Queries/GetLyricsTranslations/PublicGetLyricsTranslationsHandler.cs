using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetLyricsTranslations;

/// <summary>
/// Handles the <see cref="PublicGetLyricsTranslationsQuery" /> to list every translation of a
/// lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="translationRepository">Repository for lyrics translation data access operations.</param>
public class PublicGetLyricsTranslationsHandler(
    ILyricsRepository lyricsRepository,
    ITranslationRepository translationRepository
) : IQueryHandler<PublicGetLyricsTranslationsQuery, PublicGetLyricsTranslationsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetLyricsTranslationsResult> Handle(
        PublicGetLyricsTranslationsQuery query,
        CancellationToken cancellationToken
    )
    {
        await lyricsRepository.GetByIdOrThrowAsync(id: query.LyricsId, cancellationToken: cancellationToken);

        IReadOnlyList<LyricsTranslationEntity> translations = await translationRepository.GetAllByLyricsIdAsync(
            lyricsId: query.LyricsId,
            cancellationToken: cancellationToken
        );

        List<TranslationDto> dtos = translations
            .Select(translation => new TranslationDto(
                Id: translation.Id,
                Language: translation.Language,
                Text: translation.Text,
                Source: translation.Source.ToString()
            ))
            .ToList();

        return new PublicGetLyricsTranslationsResult(Translations: dtos);
    }
}
