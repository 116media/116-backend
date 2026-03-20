using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Interactions.UseCases.Public.Commands.RecordShortVideoView;

/// <summary>
/// Handles the <see cref="PublicRecordShortVideoViewCommand" /> to record a view event for a short video.
/// </summary>
/// <param name="shortVideoRepository">Repository for short video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicRecordShortVideoViewHandler(
    IShortVideoRepository shortVideoRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicRecordShortVideoViewCommand, PublicRecordShortVideoViewResult>
{
    /// <inheritdoc />
    public async Task<PublicRecordShortVideoViewResult> Handle(
        PublicRecordShortVideoViewCommand command,
        CancellationToken cancellationToken
    )
    {
        ShortVideoEntity shortVideo = await shortVideoRepository.GetByIdOrThrowAsync(
            id: command.ShortVideoId,
            cancellationToken: cancellationToken
        );

        shortVideo.IncrementViewCount();
        shortVideoRepository.Update(shortVideo: shortVideo);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicRecordShortVideoViewResult(IsSuccess: true);
    }
}
