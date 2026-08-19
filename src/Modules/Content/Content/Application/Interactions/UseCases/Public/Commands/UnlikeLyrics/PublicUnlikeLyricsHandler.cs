using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeLyrics;

/// <summary>
/// Handles the <see cref="PublicUnlikeLyricsCommand" /> to remove a user's like from a lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicUnlikeLyricsHandler(
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicUnlikeLyricsCommand, PublicUnlikeLyricsResult>
{
    /// <inheritdoc />
    public async Task<PublicUnlikeLyricsResult> Handle(
        PublicUnlikeLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        await lyricsRepository.GetByIdOrThrowAsync(id: command.LyricsId, cancellationToken: cancellationToken);

        bool hasLiked = await lyricsRepository.HasLikedAsync(
            userId: command.UserId,
            lyricsId: command.LyricsId,
            cancellationToken: cancellationToken
        );

        if (!hasLiked)
        {
            throw i18n.LyricsInteraction.LikeNotFound();
        }

        await lyricsRepository.RemoveLikeAsync(
            userId: command.UserId,
            lyricsId: command.LyricsId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicUnlikeLyricsResult(IsSuccess: true);
    }
}
