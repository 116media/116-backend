using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.PublishVideo;

/// <summary>
/// Handles the <see cref="AdminPublishVideoCommand" /> to publish an approved video.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminPublishVideoHandler(IVideoRepository videoRepository, IContentUnitOfWork unitOfWork, ContentI18n i18n)
    : ICommandHandler<AdminPublishVideoCommand, AdminPublishVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminPublishVideoResult> Handle(
        AdminPublishVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (video.Status == EnumContentStatus.Published)
        {
            throw i18n.Video.AlreadyPublished();
        }

        video.Publish();
        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminPublishVideoResult(IsSuccess: true);
    }
}
