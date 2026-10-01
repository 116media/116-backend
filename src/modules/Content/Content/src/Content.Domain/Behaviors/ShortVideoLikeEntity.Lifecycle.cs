using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="ShortVideoLikeEntity" />. Its state lives in <c>Entities/ShortVideoLikeEntity.cs</c>.
/// </summary>
public partial class ShortVideoLikeEntity
{
    /// <summary>
    /// Declares this like's removal so the post-commit engagement consumer
    /// can decrement the short video's cached like count.
    /// Called by the removal path immediately before the row is removed.
    /// </summary>
    public void MarkRemoved()
    {
        AddDomainEvent(
            new ShortVideoEngagedEvent(ShortVideoId: ShortVideoId, Kind: EnumEngagementKind.Like, Delta: -1)
        );
    }
}
