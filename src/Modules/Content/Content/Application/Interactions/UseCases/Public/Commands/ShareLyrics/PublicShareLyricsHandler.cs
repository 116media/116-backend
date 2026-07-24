using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareLyrics;

/// <summary>
/// Handles the <see cref="PublicShareLyricsCommand" /> to record a share event for a lyrics page.
/// </summary>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicShareLyricsHandler(ILyricsRepository lyricsRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicShareLyricsCommand, PublicShareLyricsResult>
{
    /// <inheritdoc />
    public async Task<PublicShareLyricsResult> Handle(
        PublicShareLyricsCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: command.LyricsId,
            cancellationToken: cancellationToken
        );

        var share = LyricsShareEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            lyricsId: command.LyricsId,
            shareChannel: command.ShareChannel
        );

        await lyricsRepository.AddShareAsync(share: share, cancellationToken: cancellationToken);

        lyrics.IncrementShareCount();
        lyricsRepository.Update(lyrics: lyrics);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicShareLyricsResult(IsSuccess: true);
    }
}
