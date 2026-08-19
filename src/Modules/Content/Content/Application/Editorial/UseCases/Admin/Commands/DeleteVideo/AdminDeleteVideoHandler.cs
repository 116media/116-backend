using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DeleteVideo;

/// <summary>
/// Handles the <see cref="AdminDeleteVideoCommand" /> to permanently delete a draft or rejected video.
/// Captures the thumbnail file id on the aggregate before removal so the post-commit cleanup
/// handler can soft-delete the file row and purge the remote asset after the business commit;
/// a storage failure can no longer block or outlive the deletion.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminDeleteVideoHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork, ContentI18n i18n)
    : ICommandHandler<AdminDeleteVideoCommand, AdminDeleteVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteVideoResult> Handle(
        AdminDeleteVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (video.Status != EnumContentStatus.Draft && video.Status != EnumContentStatus.Rejected)
        {
            throw i18n.Video.CannotDeletePublishedVideo();
        }

        video.MarkDeleted();
        videoRepository.Remove(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeleteVideoResult(IsSuccess: true);
    }
}
