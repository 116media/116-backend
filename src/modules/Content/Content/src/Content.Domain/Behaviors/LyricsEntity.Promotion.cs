using System.ComponentModel.DataAnnotations;
using _116.Content.Domain.Constants;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
using _116.Content.Domain.ValueObjects;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Promotion behaviour of <see cref="LyricsEntity" />. Its state lives in <c>Entities/LyricsEntity.cs</c>.
/// </summary>
public sealed partial class LyricsEntity
{
    /// <summary>
    /// Activates the lyrics page's paid promotion placement until the given date.
    /// Called by the Commerce payment verification flow only.
    /// </summary>
    /// <param name="promotionLevelId">
    /// The promotion level purchased. Accepted for signature parity with the Commerce
    /// verification call site — a lyrics page carries no promotion level column, unlike
    /// <see cref="ArticleEntity" /> and <see cref="VideoEntity" />, which persist it; the
    /// purchased level lives on <see cref="ContentOrderItemEntity" /> and is consulted at
    /// verification time only.
    /// </param>
    /// <param name="until">
    /// When the promotion expires (<c>payment.verified_at + promotion_level.duration_days</c>,
    /// the verification instant truncated to whole milliseconds).
    /// </param>
    public void StampPromotion(Guid promotionLevelId, DateTimeOffset until)
    {
        IsPromoted = true;
        PromotedUntil = until;
    }

    /// <summary>
    /// Force-removes the active paid promotion. SuperAdmin only.
    /// Records the audit trail needed for future pro-rata refund calculation.
    /// </summary>
    /// <param name="unpromotedBy">
    /// Identity of the SuperAdmin performing the force-unpromote, read from JWT claims.
    /// </param>
    /// <param name="reason">
    /// Mandatory reason for the force-unpromote (e.g. "government request", "policy violation").
    /// </param>
    /// <exception cref="ContentRuleException">
    /// Thrown when the lyrics page does not have an active promotion.
    /// </exception>
    public void ForceUnpromote(string unpromotedBy, string reason, DateTimeOffset now)
    {
        if (!IsPromoted)
        {
            throw new ContentRuleException(ContentRuleCodes.LyricsNotPromoted);
        }

        IsPromoted = false;
        PromotedUntil = null;
        UnpromotedAt = now;
        UnpromotedBy = unpromotedBy;
        UnpromotedReason = reason;

        AddDomainEvent(
            new ContentPromotionRemovedEvent(
                ContentId: Id,
                ContentType: EnumCoreContentType.Lyrics,
                CustomerId: CustomerId,
                Title: SongTitle,
                Reason: reason
            )
        );
    }
}
