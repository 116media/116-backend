using _116.Content.Domain.Enums;
using _116.Content.Domain.Exceptions;

namespace _116.Content.Domain.StateMachines;

/// <summary>
/// The publication state machine shared by articles, videos and lyrics: one transition table,
/// consulted by every entity transition method, so no handler carries its own copy of the rules.
/// </summary>
public static class ContentPublicationState
{
    /// <summary>
    /// The legal target states for each source state. A source missing from the table allows
    /// nothing.
    /// </summary>
    private static readonly Dictionary<EnumContentStatus, EnumContentStatus[]> Allowed = new()
    {
        [EnumContentStatus.Draft] = [EnumContentStatus.PendingPayment, EnumContentStatus.PendingReview],
        [EnumContentStatus.PendingPayment] = [EnumContentStatus.PendingReview, EnumContentStatus.Rejected],
        [EnumContentStatus.PendingReview] = [EnumContentStatus.Approved, EnumContentStatus.Rejected],
        [EnumContentStatus.Approved] =
        [
            EnumContentStatus.Published,
            EnumContentStatus.Rejected,
            EnumContentStatus.Archived,
        ],
        [EnumContentStatus.Published] = [EnumContentStatus.Archived, EnumContentStatus.Rejected],
        [EnumContentStatus.Rejected] = [EnumContentStatus.PendingReview, EnumContentStatus.Archived],
        [EnumContentStatus.Archived] = [EnumContentStatus.PendingReview],
    };

    /// <summary>
    /// The states in which content is still editable; past review, edits are refused.
    /// </summary>
    private static readonly EnumContentStatus[] Editable =
    [
        EnumContentStatus.Draft,
        EnumContentStatus.PendingPayment,
        EnumContentStatus.PendingReview,
        EnumContentStatus.Rejected,
    ];

    /// <summary>
    /// Reports whether the table allows moving from one publication state to another.
    /// </summary>
    /// <param name="from">The current state.</param>
    /// <param name="to">The requested state.</param>
    /// <returns>True when the move is legal.</returns>
    public static bool CanMove(EnumContentStatus from, EnumContentStatus to)
    {
        return Allowed.TryGetValue(from, out EnumContentStatus[]? targets) && Array.IndexOf(targets, to) >= 0;
    }

    /// <summary>
    /// Throws <see cref="ContentRuleException" /> when the requested move is not in the
    /// transition table.
    /// </summary>
    /// <param name="from">The current state.</param>
    /// <param name="to">The requested state.</param>
    /// <param name="contentType">The content type performing the move.</param>
    public static void EnsureCanMove(EnumContentStatus from, EnumContentStatus to, EnumCoreContentType contentType)
    {
        if (!CanMove(from: from, to: to))
        {
            throw new ContentRuleException(
                ContentRuleCodes.InvalidStatusTransition,
                contentType.ToString(),
                from.ToString(),
                to.ToString()
            );
        }
    }

    /// <summary>
    /// Throws <see cref="ContentRuleException" /> when the content has moved past review
    /// and can no longer be edited.
    /// </summary>
    /// <param name="status">The current state.</param>
    /// <param name="contentType">The content type being edited.</param>
    public static void EnsureEditable(EnumContentStatus status, EnumCoreContentType contentType)
    {
        if (Array.IndexOf(Editable, status) < 0)
        {
            throw new ContentRuleException(ContentRuleCodes.NotEditable, contentType.ToString(), status.ToString());
        }
    }
}
