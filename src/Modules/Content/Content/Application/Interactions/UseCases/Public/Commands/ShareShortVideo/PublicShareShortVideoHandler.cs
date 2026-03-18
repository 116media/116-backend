using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareShortVideo;

/// <summary>
/// Handles the <see cref="PublicShareShortVideoCommand" /> to record a share event for a short video.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicShareShortVideoHandler(IShortVideoRepository shortVideoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicShareShortVideoCommand, PublicShareShortVideoResult>
{
    /// <inheritdoc />
    public async Task<PublicShareShortVideoResult> Handle(
        PublicShareShortVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        var share = ShortVideoShareEntity.Create(
            id: Guid.NewGuid(),
            userId: command.UserId,
            shortVideoId: command.ShortVideoId
        );

        await shortVideoRepository.AddShareAsync(share: share, cancellationToken: cancellationToken);

        shortVideo.IncrementShareCount();
        shortVideoRepository.Update(shortVideo: shortVideo);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicShareShortVideoResult(IsSuccess: true);
    }
}
