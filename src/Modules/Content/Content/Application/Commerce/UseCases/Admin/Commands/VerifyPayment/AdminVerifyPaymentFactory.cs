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
            await ApplyPaidEffectsAsync(item: item, cancellationToken: cancellationToken);
        }

        await contentOrderRepository.UpdatePaymentAsync(payment: payment, ct: cancellationToken);
        await contentOrderRepository.UpdateAsync(order: order, ct: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Applies the post-payment effects to whichever content record fulfils this order item.
    /// An order item is fulfilled by exactly one content type, so the first match wins and the
    /// remaining lookups are skipped.
    /// </summary>
    /// <param name="item">The paid order item to apply effects for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    private async Task ApplyPaidEffectsAsync(ContentOrderItemEntity item, CancellationToken cancellationToken)
    {
        Promotion? promotion = await ResolvePromotionAsync(item: item, cancellationToken: cancellationToken);

        ArticleEntity? article = await articleRepository.GetByOrderItemIdAsync(
            orderItemId: item.Id,
            cancellationToken: cancellationToken
        );

        if (article is not null)
        {
            if (item.SocialBoost)
            {
                article.StampSocialBoost();
            }

            if (promotion is not null)
            {
                article.StampPromotion(promotionLevelId: promotion.LevelId, until: promotion.Until);
            }

            article.MarkPendingReview();
            articleRepository.Update(article: article);
            return;
        }

        VideoEntity? video = await videoRepository.GetByOrderItemIdAsync(
            orderItemId: item.Id,
            cancellationToken: cancellationToken
        );

        if (video is not null)
        {
            if (item.SocialBoost)
            {
                video.StampSocialBoost();
            }

            if (promotion is not null)
            {
                video.StampPromotion(promotionLevelId: promotion.LevelId, until: promotion.Until);
            }

            video.MarkPendingReview();
            videoRepository.Update(video: video);
            return;
        }

        LyricsEntity? lyrics = await lyricsRepository.GetByOrderItemIdAsync(
            orderItemId: item.Id,
            cancellationToken: cancellationToken
        );

        if (lyrics is not null)
        {
            // No social boost here: a lyrics page has no social boost concept, unlike articles
            // and videos. This omission is intentional, not a missing case.
            if (promotion is not null)
            {
                lyrics.StampPromotion(promotionLevelId: promotion.LevelId, until: promotion.Until);
            }

            lyrics.MarkPendingReview();
            lyricsRepository.Update(lyrics: lyrics);
        }
    }

    /// <summary>
    /// Resolves the promotion level an order item was bought with and the moment that promotion
    /// expires, or <c>null</c> when the item carries no promotion.
    /// </summary>
    /// <param name="item">The order item to resolve the promotion for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The resolved promotion, or <c>null</c> when the item is not promoted.</returns>
    private async Task<Promotion?> ResolvePromotionAsync(
        ContentOrderItemEntity item,
        CancellationToken cancellationToken
    )
    {
        if (!item.PromotionLevelId.HasValue)
        {
            return null;
        }

        PromotionLevelEntity promoLevel = await lookupRepository.GetPromotionLevelByIdOrThrowAsync(
            id: item.PromotionLevelId.Value,
            cancellationToken: cancellationToken
        );

        return new Promotion(LevelId: promoLevel.Id, Until: DateTimeOffset.UtcNow.AddDays(promoLevel.DurationDays));
    }

    /// <summary>
    /// A resolved promotion purchase, applied identically to every content type that supports it.
    /// </summary>
    /// <param name="LevelId">The purchased promotion level.</param>
    /// <param name="Until">The moment the promotion expires.</param>
    private sealed record Promotion(Guid LevelId, DateTimeOffset Until);
}
