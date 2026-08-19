using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.LikeShortVideo;

/// <summary>
/// Handles the <see cref="PublicLikeShortVideoCommand" /> to record a user's like on a short video.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class PublicLikeShortVideoHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<PublicLikeShortVideoCommand, PublicLikeShortVideoResult>
{
    /// <inheritdoc />
    public async Task<PublicLikeShortVideoResult> Handle(
        PublicLikeShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        await shortVideoRepository.GetByIdOrThrowAsync(id: command.ShortVideoId, cancellationToken: cancellationToken);

        bool alreadyLiked = await shortVideoRepository.HasLikedAsync(
            userId: command.UserId,
            shortVideoId: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        if (alreadyLiked)
        {
            throw i18n.ShortVideoInteraction.AlreadyLiked();
        }

        var like = ShortVideoLikeEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            shortVideoId: command.ShortVideoId
        );

        await shortVideoRepository.AddLikeAsync(like: like, cancellationToken: cancellationToken);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicLikeShortVideoResult(IsSuccess: true);
    }
}
