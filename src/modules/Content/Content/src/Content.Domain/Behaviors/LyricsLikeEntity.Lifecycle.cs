using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="LyricsLikeEntity" />. Its state lives in <c>Entities/LyricsLikeEntity.cs</c>.
/// </summary>
public partial class LyricsLikeEntity
{
    /// <summary>
    /// Declares this like's removal so the post-commit engagement consumer
    /// can decrement the lyrics page's cached like count.
    /// Called by the removal path immediately before the row is removed.
    /// </summary>
    public void MarkRemoved()
    {
        AddDomainEvent(new LyricsEngagedEvent(LyricsId: LyricsId, Kind: EnumEngagementKind.Like, Delta: -1));
    }
}
