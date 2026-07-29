using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Cache;
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
/// <param name="cacheInvalidator">Invalidates the popular-videos cache after the published set changes.</param>
public class AdminPublishVideoHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n,
    IPopularVideosCacheInvalidator cacheInvalidator,
    ICommerceCustomerNotifier customerNotifier
) : ICommandHandler<AdminPublishVideoCommand, AdminPublishVideoResult>
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

        if (video.Status != EnumContentStatus.Approved)
        {
            throw i18n.Video.InvalidStatusTransition(
                from: video.Status.ToString(),
                to: nameof(EnumContentStatus.Published)
            );
        }

        video.Publish(i18n.Video);
        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await customerNotifier.NotifyContentPublishedAsync(
            customerId: video.CustomerId,
            contentTitle: video.Title,
            publicUrl: ContentPublicLinks.Video(video.Slug),
            cancellationToken: cancellationToken
        );

        cacheInvalidator.Invalidate();

        return new AdminPublishVideoResult(IsSuccess: true);
    }
}
