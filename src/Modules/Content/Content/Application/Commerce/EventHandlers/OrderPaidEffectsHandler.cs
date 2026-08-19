using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Shared.Application.Services;
using Microsoft.Extensions.Logging;

namespace _116.Content.Application.Commerce.EventHandlers;

/// <summary>
/// Applies the paid-for effects (promotion stamp, social boost, pending
/// review) carried on <see cref="OrderPaidEvent" /> to the content record
/// fulfilling each order item. An order item is fulfilled by exactly one
/// content type, so the first match wins and the remaining lookups are
/// skipped. Each item commits on its own so one failing item never rolls
/// back the effects already applied for its siblings.
/// <para>
/// Redispatching the event is safe. Every effect is either already present
/// or has since been settled by a later decision, and neither case writes:
/// the promotion window is applied verbatim from the payload (computed when
/// the order was marked paid, never recomputed) and is compared for
/// equality against the persisted value, which the payload's
/// millisecond-truncated instants make exact; social boost is a one-way
/// flag; the review status advances only from
/// <c>Draft</c>/<c>PendingPayment</c>/<c>Rejected</c>; and a promotion a
/// SuperAdmin force-removed after the order was paid is treated as settled
/// rather than re-activated.
/// </para>
/// </summary>
/// <param name="articleRepository">Repository for article data access operations.</param>
/// <param name="videoRepository">Repository for video data access operations.</param>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="unitOfWork">Unit of Work committing each item's effects.</param>
/// <param name="logger">Logger recording items no content record fulfils.</param>
public class OrderPaidEffectsHandler(
    IArticleRepository articleRepository,
    IVideoRepository videoRepository,
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork,
    ILogger<OrderPaidEffectsHandler> logger
) : IDomainEventHandler<OrderPaidEvent>
{
    /// <inheritdoc />
    public async Task Handle(OrderPaidEvent domainEvent, CancellationToken cancellationToken = default)
    {
        foreach (PaidItemEffect effect in domainEvent.Items)
        {
            bool applied =
                await TryApplyToArticleAsync(
                    effect: effect,
                    paidAt: domainEvent.PaidAt,
                    cancellationToken: cancellationToken
                )
                || await TryApplyToVideoAsync(
                    effect: effect,
                    paidAt: domainEvent.PaidAt,
                    cancellationToken: cancellationToken
                )
                || await TryApplyToLyricsAsync(
                    effect: effect,
                    paidAt: domainEvent.PaidAt,
                    cancellationToken: cancellationToken
                );

            if (!applied)
            {
                logger.LogDebug(
                    "Paid effects skipped for order item {OrderItemId}: no content record fulfils it.",
                    effect.OrderItemId
                );
            }
        }
    }

    /// <summary>
    /// Applies the effect to the article fulfilling the order item, when one
    /// exists. Commits only when the article state actually changed.
    /// </summary>
    /// <param name="effect">The paid item effect to apply.</param>
    /// <param name="paidAt">The instant the order was marked paid.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns><c>true</c> when an article fulfils the item; otherwise <c>false</c>.</returns>
    private async Task<bool> TryApplyToArticleAsync(
        PaidItemEffect effect,
        DateTimeOffset paidAt,
        CancellationToken cancellationToken
    )
    {
        ArticleEntity? article = await articleRepository.GetByOrderItemIdAsync(
            orderItemId: effect.OrderItemId,
            cancellationToken: cancellationToken
        );

        if (article is null)
        {
            return false;
        }

        bool changed = false;

        if (effect.SocialBoost && !article.SocialBoost)
        {
            article.StampSocialBoost();
            changed = true;
        }

        bool promotionStamped =
            article.IsPromoted
            && article.PromotionLevelId == effect.PromotionLevelId
            && article.PromotedUntil == effect.PromotionUntil;

        if (
            effect.PromotionLevelId.HasValue
            && effect.PromotionUntil.HasValue
            && !promotionStamped
            && !IsPromotionSettled(unpromotedAt: article.UnpromotedAt, paidAt: paidAt)
        )
        {
            article.StampPromotion(promotionLevelId: effect.PromotionLevelId.Value, until: effect.PromotionUntil.Value);
            changed = true;
        }

        changed |= article.MarkPendingReview();

        if (changed)
        {
            articleRepository.Update(article: article);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        return true;
    }

    /// <summary>
    /// Applies the effect to the video fulfilling the order item, when one
    /// exists. Commits only when the video state actually changed.
    /// </summary>
    /// <param name="effect">The paid item effect to apply.</param>
    /// <param name="paidAt">The instant the order was marked paid.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns><c>true</c> when a video fulfils the item; otherwise <c>false</c>.</returns>
    private async Task<bool> TryApplyToVideoAsync(
        PaidItemEffect effect,
        DateTimeOffset paidAt,
        CancellationToken cancellationToken
    )
    {
        VideoEntity? video = await videoRepository.GetByOrderItemIdAsync(
            orderItemId: effect.OrderItemId,
            cancellationToken: cancellationToken
        );

        if (video is null)
        {
            return false;
        }

        bool changed = false;

        if (effect.SocialBoost && !video.SocialBoost)
        {
            video.StampSocialBoost();
            changed = true;
        }

        bool promotionStamped =
            video.IsPromoted
            && video.PromotionLevelId == effect.PromotionLevelId
            && video.PromotedUntil == effect.PromotionUntil;

        if (
            effect.PromotionLevelId.HasValue
            && effect.PromotionUntil.HasValue
            && !promotionStamped
            && !IsPromotionSettled(unpromotedAt: video.UnpromotedAt, paidAt: paidAt)
        )
        {
            video.StampPromotion(promotionLevelId: effect.PromotionLevelId.Value, until: effect.PromotionUntil.Value);
            changed = true;
        }

        changed |= video.MarkPendingReview();

        if (changed)
        {
            videoRepository.Update(video: video);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        return true;
    }

    /// <summary>
    /// Applies the effect to the lyrics page fulfilling the order item, when
    /// one exists. A lyrics page has no social boost concept, so that flag is
    /// intentionally ignored here. Commits only when the lyrics state
    /// actually changed.
    /// </summary>
    /// <param name="effect">The paid item effect to apply.</param>
    /// <param name="paidAt">The instant the order was marked paid.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns><c>true</c> when a lyrics page fulfils the item; otherwise <c>false</c>.</returns>
    private async Task<bool> TryApplyToLyricsAsync(
        PaidItemEffect effect,
        DateTimeOffset paidAt,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity? lyrics = await lyricsRepository.GetByOrderItemIdAsync(
            orderItemId: effect.OrderItemId,
            cancellationToken: cancellationToken
        );

        if (lyrics is null)
        {
            return false;
        }

        bool changed = false;

        bool promotionStamped = lyrics.IsPromoted && lyrics.PromotedUntil == effect.PromotionUntil;

        if (
            effect.PromotionLevelId.HasValue
            && effect.PromotionUntil.HasValue
            && !promotionStamped
            && !IsPromotionSettled(unpromotedAt: lyrics.UnpromotedAt, paidAt: paidAt)
        )
        {
            lyrics.StampPromotion(promotionLevelId: effect.PromotionLevelId.Value, until: effect.PromotionUntil.Value);
            changed = true;
        }

        changed |= lyrics.MarkPendingReview();

        if (changed)
        {
            lyricsRepository.Update(lyrics: lyrics);
            await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
        }

        return true;
    }

    /// <summary>
    /// Reports whether the paid-for promotion has already been settled by a
    /// force-unpromote that happened after the order was paid. Such content
    /// carries the removal audit trail and must keep it: re-stamping would
    /// revive a placement a SuperAdmin deliberately pulled.
    /// </summary>
    /// <param name="unpromotedAt">When the content was last force-unpromoted, if ever.</param>
    /// <param name="paidAt">The instant the order was marked paid.</param>
    /// <returns><c>true</c> when the promotion was removed after payment; otherwise <c>false</c>.</returns>
    private static bool IsPromotionSettled(DateTimeOffset? unpromotedAt, DateTimeOffset paidAt)
    {
        return unpromotedAt.HasValue && unpromotedAt.Value >= paidAt;
    }
}
