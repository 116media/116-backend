using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeLyrics;

/// <summary>
/// Handles the <see cref="PublicLikeLyricsCommand" /> to record a user's like on a lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicLikeLyricsHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicLikeLyricsCommand, PublicLikeLyricsResult>
{
    /// <inheritdoc />
    public async Task<PublicLikeLyricsResult> Handle(
        PublicLikeLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.LyricsId,
            cancellationToken: cancellationToken
        );

        bool alreadyLiked = await lyricsRepository.HasLikedAsync(
            userId: command.UserId,
            lyricsId: command.LyricsId,
            cancellationToken: cancellationToken
        );

        if (alreadyLiked)
        {
            throw i18n.LyricsInteraction.AlreadyLiked();
        }

        var like = LyricsLikeEntity.Create(id: Guid.NewGuid(), userId: command.UserId, lyricsId: command.LyricsId);

        await lyricsRepository.AddLikeAsync(like: like, cancellationToken: cancellationToken);

        lyrics.IncrementLikeCount();
        lyricsRepository.Update(lyrics: lyrics);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicLikeLyricsResult(IsSuccess: true);
    }
}
