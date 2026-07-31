namespace _116.Content.Domain.Enums;

/// <summary>
/// The kind of engagement a user performed against a content surface.
/// Carried on the per-surface engagement events so a single consumer can
/// apply the matching denormalized-counter mutation.
/// </summary>
public enum EnumEngagementKind
{
    /// <summary>
    /// A like was added or removed.
    /// </summary>
    Like,

    /// <summary>
    /// A bookmark was added or removed.
    /// </summary>
    Bookmark,

    /// <summary>
    /// A share was recorded. Shares are append-only, so the delta is always positive.
    /// </summary>
    Share,

    /// <summary>
    /// A comment (or reply) was added or soft-deleted.
    /// </summary>
    Comment,

    /// <summary>
    /// A view passed the counting gate. Views are append-only, so the delta is always positive.
    /// </summary>
    View,

    /// <summary>
    /// A star rating was created or restarred. The rating aggregates are
    /// recomputed from the rating rows, so the delta is informational only.
    /// </summary>
    Rating,
}
