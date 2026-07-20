using _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment.Contracts;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Commerce.UseCases.Admin.Commands.VerifyPayment;

/// <summary>
/// Factory implementation for the payment verification flow.
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="contentOrderRepository">Repository for content order data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="contentOrderErrors">Content order domain error factory.</param>
public class AdminVerifyPaymentFactory(
    IArticleRepository articleRepository,
    IVideoRepository videoRepository,
    ILyricsRepository lyricsRepository,
    ILookupRepository lookupRepository,
    IContentOrderRepository contentOrderRepository,
    IContentUnitOfWork unitOfWork,
    ContentOrderErrors contentOrderErrors
) : IVerifyPaymentFactory
{
    /// <inheritdoc />
    public async Task VerifyAsync(
        ContentOrderEntity order,
        ContentPaymentEntity payment,
        Guid adminUserId,
        string receiptUrl,
        CancellationToken cancellationToken
    )
    {
        payment.Verify(adminUserId: adminUserId, receiptUrl: receiptUrl, errors: contentOrderErrors);
        order.MarkPaid(contentOrderErrors);

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

            LyricsEntity? lyrics =
                article is null && video is null
                    ? await lyricsRepository.GetByOrderItemIdAsync(
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

                DateTimeOffset promotedUntil = DateTimeOffset.UtcNow.AddDays(promoLevel.DurationDays);
                article?.StampPromotion(promotionLevelId: promoLevel.Id, until: promotedUntil);
                video?.StampPromotion(promotionLevelId: promoLevel.Id, until: promotedUntil);
                lyrics?.StampPromotion(promotionLevelId: promoLevel.Id, until: promotedUntil);
            }

            article?.MarkPendingReview();
            video?.MarkPendingReview();
            lyrics?.MarkPendingReview();

            if (article is not null)
            {
                articleRepository.Update(article: article);
            }

            if (video is not null)
            {
                videoRepository.Update(video: video);
            }

            if (lyrics is not null)
            {
                lyricsRepository.Update(lyrics: lyrics);
            }
        }

        await contentOrderRepository.UpdatePaymentAsync(payment: payment, ct: cancellationToken);
        await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
