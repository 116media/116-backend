using _116.Content.Application.Commerce.Services;
using _116.Content.Application.Editorial.Services;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RejectVideo;

/// <summary>
/// Handles the <see cref="AdminRejectVideoCommand" /> to reject a video during editorial review.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRejectVideoHandler(
    IVideoRepository videoRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n,
    ICommerceCustomerNotifier customerNotifier
) : ICommandHandler<AdminRejectVideoCommand, AdminRejectVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminRejectVideoResult> Handle(
        AdminRejectVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (video.Status == EnumContentStatus.Rejected)
        {
            throw i18n.Video.AlreadyRejected();
        }

        if (video.Status != EnumContentStatus.PendingReview)
        {
            throw i18n.Video.InvalidStatusTransition(
                from: video.Status.ToString(),
                to: nameof(EnumContentStatus.Rejected)
            );
        }

        video.Reject(reason: command.Reason);
        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        await customerNotifier.NotifyContentRejectedAsync(
            customerId: video.CustomerId,
            contentTitle: video.Title,
            reason: command.Reason,
            cancellationToken: cancellationToken
        );

        return new AdminRejectVideoResult(IsSuccess: true);
    }
}
