using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.UnlikeShortVideo;

/// <summary>
/// Handles the <see cref="PublicUnlikeShortVideoCommand" /> to remove a user's like from a short video.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicUnlikeShortVideoHandler(IShortVideoRepository shortVideoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicUnlikeShortVideoCommand, PublicUnlikeShortVideoResult>
{
    /// <inheritdoc />
    public async Task<PublicUnlikeShortVideoResult> Handle(
        PublicUnlikeShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        bool hasLiked = await shortVideoRepository.HasLikedAsync(
            userId: command.UserId,
            shortVideoId: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        if (!hasLiked)
        {
            throw ShortVideoInteractionErrors.LikeNotFound();
        }

        await shortVideoRepository.RemoveLikeAsync(
            userId: command.UserId,
            shortVideoId: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        shortVideo.DecrementLikeCount();
        shortVideoRepository.Update(shortVideo: shortVideo);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicUnlikeShortVideoResult(IsSuccess: true);
    }
}
