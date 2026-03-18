using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Handles the <see cref="AdminVerifyPaymentCommand" /> to verify an order payment and stamp content promotions.
/// </summary>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="lookupRepository">Repository for promotion level data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminVerifyPaymentHandler(
    IContentOrderRepository contentOrderRepository,
    IArticleRepository articleRepository,
    IVideoRepository videoRepository,
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminVerifyPaymentCommand, AdminVerifyPaymentResult>
{
    /// <inheritdoc />
    public async Task<AdminVerifyPaymentResult> Handle(
        AdminVerifyPaymentCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid orderId = Guid.Parse(command.OrderId);

        ContentOrderEntity? order = await contentOrderRepository.GetByIdWithItemsAsync(
            id: orderId,
            ct: cancellationToken
        );

        if (order is null)
        {
            throw ContentOrderErrors.NotFound(id: orderId);
        }

        ContentPaymentEntity? payment = await contentOrderRepository.GetPaymentByOrderIdAsync(
            orderId: orderId,
            ct: cancellationToken
        );

        if (payment is null)
        {
            throw ContentOrderErrors.PaymentNotFound(orderId: orderId);
        }

        payment.Verify(adminUserId: command.AdminUserId, receiptUrl: command.ReceiptUrl);
        order.MarkPaid();

        foreach (ContentOrderItemEntity item in order.Items)
        {
            ArticleEntity? article = await articleRepository.GetByOrderItemIdAsync(
                orderItemId: item.Id,
                cancellationToken: cancellationToken
            );

            VideoEntity? video = article is null
                ? await videoRepository.GetByOrderItemIdAsync(
                    orderItemId: item.Id,
                    cancellationToken: cancellationToken
                )
                : null;

            if (item.SocialBoost)
            {
                article?.StampSocialBoost();
                video?.StampSocialBoost();
            }

            if (item.PromotionLevelId.HasValue)
            {
                PromotionLevelEntity promoLevel = await lookupRepository.GetPromotionLevelByIdOrThrowAsync(
                    id: item.PromotionLevelId.Value,
                    cancellationToken: cancellationToken
                );

                DateTimeOffset featuredUntil = DateTimeOffset.UtcNow.AddDays(promoLevel.DurationDays);
                article?.StampFeatured(until: featuredUntil);
                video?.StampFeatured(until: featuredUntil);
            }

            if (article is not null)
            {
                articleRepository.Update(article: article);
            }

            if (video is not null)
            {
                videoRepository.Update(video: video);
            }
        }

        await contentOrderRepository.UpdatePaymentAsync(payment: payment, ct: cancellationToken);
        await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminVerifyPaymentResult(IsSuccess: true);
    }
}
