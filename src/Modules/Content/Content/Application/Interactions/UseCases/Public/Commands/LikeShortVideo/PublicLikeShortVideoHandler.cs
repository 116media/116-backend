using _116.Content.Application.Shared.Errors;
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
public class PublicLikeShortVideoHandler(IShortVideoRepository shortVideoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicLikeShortVideoCommand, PublicLikeShortVideoResult>
{
    /// <inheritdoc />
    public async Task<PublicLikeShortVideoResult> Handle(
        PublicLikeShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        bool alreadyLiked = await shortVideoRepository.HasLikedAsync(
            userId: command.UserId,
            shortVideoId: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        if (alreadyLiked)
        {
            throw ShortVideoInteractionErrors.AlreadyLiked();
        }

        var like = ShortVideoLikeEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            shortVideoId: command.ShortVideoId
        );

        await shortVideoRepository.AddLikeAsync(like: like, cancellationToken: cancellationToken);

        shortVideo.IncrementLikeCount();
        shortVideoRepository.Update(shortVideo: shortVideo);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicLikeShortVideoResult(IsSuccess: true);
    }
}
