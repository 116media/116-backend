using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.ShareVideo;

/// <summary>
/// Handles the <see cref="PublicShareVideoCommand" /> to record a share event on a video.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicShareVideoHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<PublicShareVideoCommand, PublicShareVideoResult>
{
    /// <inheritdoc />
    public async Task<PublicShareVideoResult> Handle(
        PublicShareVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(
            id: command.VideoId,
            cancellationToken: cancellationToken
        );

        var share = VideoShareEntity.Create(id: Guid.NewGuid(), userId: command.UserId, videoId: command.VideoId);

        await videoRepository.AddShareAsync(share: share, cancellationToken: cancellationToken);

        video.IncrementShareCount();
        videoRepository.Update(video: video);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicShareVideoResult(IsSuccess: true);
    }
}
