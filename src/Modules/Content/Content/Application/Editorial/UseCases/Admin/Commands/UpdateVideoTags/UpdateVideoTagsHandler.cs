using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;

/// <summary>
/// Handles the <see cref="UpdateVideoTagsCommand" /> to replace all tags on a video.
/// Validates all tag identifiers, removes existing tag associations, then adds the new set.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="lookupRepository">Repository for lookup entities including tags.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class UpdateVideoTagsHandler(
    IVideoRepository videoRepository,
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<UpdateVideoTagsCommand>
{
    /// <inheritdoc />
    public async Task Handle(UpdateVideoTagsCommand command, CancellationToken cancellationToken)
    {
        Guid videoId = Guid.Parse(command.VideoId);

        await videoRepository.GetByIdOrThrowAsync(id: videoId, cancellationToken: cancellationToken);

        foreach (Guid tagId in command.TagIds)
        {
            await lookupRepository.GetTagByIdOrThrowAsync(id: tagId, cancellationToken: cancellationToken);
        }

        IReadOnlyList<VideoTagEntity> existingTags = await videoRepository.GetTagsByVideoIdAsync(
            videoId: videoId,
            cancellationToken: cancellationToken
        );

        foreach (VideoTagEntity tag in existingTags)
        {
            videoRepository.RemoveTag(tag: tag);
        }

        foreach (Guid tagId in command.TagIds)
        {
            await videoRepository.AddTagAsync(
                tag: new VideoTagEntity { VideoId = videoId, TagId = tagId },
                cancellationToken: cancellationToken
            );
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
