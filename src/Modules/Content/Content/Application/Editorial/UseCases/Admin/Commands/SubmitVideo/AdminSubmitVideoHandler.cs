using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.SubmitVideo;

/// <summary>
/// Handles the <see cref="AdminSubmitVideoCommand" /> to submit a video for review or payment.
/// </summary>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminSubmitVideoHandler(
    IVideoRepository videoRepository,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminSubmitVideoCommand, AdminSubmitVideoResult>
{
    /// <inheritdoc />
    public async Task<AdminSubmitVideoResult> Handle(
        AdminSubmitVideoCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        VideoEntity video = await videoRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        if (video.CustomerId.HasValue && video.OrderItemId.HasValue)
        {
            ContentOrderEntity? order = await contentOrderRepository.GetOrderByItemIdAsync(
                orderItemId: video.OrderItemId.Value,
                ct: cancellationToken
            );

            bool orderAlreadyPaid = order?.Status == EnumOrderStatus.Paid;

            if (orderAlreadyPaid && !video.MarkPendingReview())
            {
                throw i18n.Video.AlreadyPendingReview();
            }

            if (!orderAlreadyPaid && !video.Submit())
            {
                throw i18n.Video.AlreadySubmitted();
            }
        }
        else if (!video.CustomerId.HasValue && !video.MarkPendingReview())
        {
            throw i18n.Video.AlreadyPendingReview();
        }

        videoRepository.Update(video: video);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminSubmitVideoResult(IsSuccess: true);
    }
}
