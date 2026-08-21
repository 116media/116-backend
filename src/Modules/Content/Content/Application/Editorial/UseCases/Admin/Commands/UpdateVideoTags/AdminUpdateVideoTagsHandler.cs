using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Helpers;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideoTags;

/// <summary>
/// Handles the <see cref="AdminUpdateVideoTagsCommand" /> to replace all tags on a video.
/// For each tag name, the handler looks up an existing tag by slug or creates a new one,
/// then removes existing video tag associations and adds the resolved set.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="lookupRepository">Repository for lookup entities including tags.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminUpdateVideoTagsHandler(
    IVideoRepository videoRepository,
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminUpdateVideoTagsCommand, AdminUpdateVideoTagsResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateVideoTagsResult> Handle(
        AdminUpdateVideoTagsCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid videoId = Guid.Parse(command.VideoId);

        await videoRepository.GetByIdOrThrowAsync(id: videoId, cancellationToken: cancellationToken);

        var resolvedTagIds = new List<Guid>();

        foreach (string name in command.TagNames)
        {
            TagEntity? existing = await lookupRepository.GetTagByNameAsync(
                name: name,
                cancellationToken: cancellationToken
            );

            if (existing is null)
            {
                string uniqueSlug = SlugHelper.ToUniqueSlug(name);
                existing = TagEntity.Create(id: Guid.NewGuid(), name: name, slug: uniqueSlug);
                await lookupRepository.AddTagAsync(tag: existing, cancellationToken: cancellationToken);
            }

            resolvedTagIds.Add(existing.Id);
        }

        IReadOnlyList<VideoTagEntity> existingTags = await videoRepository.GetTagsByVideoIdAsync(
            videoId: videoId,
            cancellationToken: cancellationToken
        );

        foreach (VideoTagEntity tag in existingTags)
        {
            videoRepository.RemoveTag(tag: tag);
        }

        foreach (Guid tagId in resolvedTagIds)
        {
            var tag = VideoTagEntity.Create(id: Guid.NewGuid(), videoId: videoId, tagId: tagId);
            await videoRepository.AddTagAsync(tag: tag, cancellationToken: cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUpdateVideoTagsResult(IsSuccess: true);
    }
}
