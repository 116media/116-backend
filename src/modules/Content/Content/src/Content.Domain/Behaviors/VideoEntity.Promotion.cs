using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Promotion behaviour of <see cref="VideoEntity" />. Its state lives in <c>Entities/VideoEntity.cs</c>.
/// </summary>
public sealed partial class VideoEntity
{
    /// <summary>
    /// Flags for manual social media promotion. Called by Commerce only.
    /// </summary>
    public void StampSocialBoost() => SocialBoost = true;

    /// <summary>
    /// Activates the video's paid promotion placement until the given date.
    /// Called by the Commerce payment verification flow only.
    /// </summary>
    /// <param name="promotionLevelId">
    /// The promotion level purchased, used to determine the homepage grid spot.
    /// </param>
    /// <param name="until">
    /// When the promotion expires (<c>payment.verified_at + promotion_level.duration_days</c>,
    /// the verification instant truncated to whole milliseconds).
    /// </param>
    public void StampPromotion(Guid promotionLevelId, DateTimeOffset until)
    {
        IsPromoted = true;
        PromotionLevelId = promotionLevelId;
        PromotedUntil = until;
    }

    /// <summary>
    /// Force-removes the active paid promotion. SuperAdmin only.
    /// Clears the purchased level alongside the window so no stale placement
    /// data outlives the promotion, and records the audit trail needed for
    /// future pro-rata refund calculation.
    /// </summary>
    /// <param name="unpromotedBy">
    /// Identity of the SuperAdmin performing the force-unpromote, read from JWT claims.
    /// </param>
    /// <param name="reason">
    /// Mandatory reason for the force-unpromote (e.g. "government request", "policy violation").
    /// </param>
    /// <exception cref="ContentRuleException">
    /// Thrown when the video does not have an active promotion.
    /// </exception>
    public void ForceUnpromote(string unpromotedBy, string reason, DateTimeOffset now)
    {
        if (!IsPromoted)
        {
            throw new ContentRuleException(ContentRuleCodes.VideoNotPromoted);
        }

        IsPromoted = false;
        PromotedUntil = null;
        PromotionLevelId = null;
        UnpromotedAt = now;
        UnpromotedBy = unpromotedBy;
        UnpromotedReason = reason;

        AddDomainEvent(
            new ContentPromotionRemovedEvent(
                ContentId: Id,
                ContentType: EnumCoreContentType.Video,
                CustomerId: CustomerId,
                Title: Title,
                Reason: reason
            )
        );
    }
}
