using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="ShortVideoBookmarkEntity" />. Its state lives in <c>Entities/ShortVideoBookmarkEntity.cs</c>.
/// </summary>
public partial class ShortVideoBookmarkEntity
{
    /// <summary>
    /// Declares this bookmark's removal so the post-commit engagement consumer
    /// can decrement the short video's cached bookmark count.
    /// Called by the removal path immediately before the row is removed.
    /// </summary>
    public void MarkRemoved()
    {
        AddDomainEvent(
            new ShortVideoEngagedEvent(ShortVideoId: ShortVideoId, Kind: EnumEngagementKind.Bookmark, Delta: -1)
        );
    }
}
