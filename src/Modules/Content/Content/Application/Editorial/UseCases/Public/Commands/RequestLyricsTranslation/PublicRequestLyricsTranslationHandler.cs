using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Application.Shared.Services;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.RequestLyricsTranslation;

/// <summary>
/// Handles the <see cref="PublicRequestLyricsTranslationCommand" /> to request or retrieve an
/// AI-generated translation of a lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="translationRepository">Repository for lyrics translation data access operations.</param>
/// <param name="translationService">Port used to generate a translation on first request.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicRequestLyricsTranslationHandler(
    ILyricsRepository lyricsRepository,
    ITranslationRepository translationRepository,
    ITranslationService translationService,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicRequestLyricsTranslationCommand, PublicRequestLyricsTranslationResult>
{
    /// <inheritdoc />
    public async Task<PublicRequestLyricsTranslationResult> Handle(
        PublicRequestLyricsTranslationCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsTranslationEntity? existing = await translationRepository.GetByLyricsAndLanguageAsync(
            lyricsId: command.LyricsId,
            language: command.Language,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            return new PublicRequestLyricsTranslationResult(Text: existing.Text, Source: existing.Source.ToString());
        }

        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.LyricsId,
            cancellationToken: cancellationToken
        );

        string translatedText = await translationService.TranslateAsync(
            text: lyrics.LyricsText,
            targetLanguage: command.Language,
            cancellationToken: cancellationToken
        );

        var translation = LyricsTranslationEntity.CreateAi(
            id: Guid.NewGuid(),
            lyricsId: command.LyricsId,
            language: command.Language,
            text: translatedText
        );

        await translationRepository.AddAsync(translation: translation, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicRequestLyricsTranslationResult(Text: translatedText, Source: nameof(EnumTranslationSource.Ai));
    }
}
