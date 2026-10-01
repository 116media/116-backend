using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="ArticleBookmarkEntity" />. Its state lives in <c>Entities/ArticleBookmarkEntity.cs</c>.
/// </summary>
public partial class ArticleBookmarkEntity
{
    /// <summary>
    /// Declares this bookmark's removal so the post-commit engagement consumer
    /// can decrement the article's cached bookmark count.
    /// Called by the removal path immediately before the row is removed.
    /// </summary>
    public void MarkRemoved()
    {
        AddDomainEvent(new ArticleEngagedEvent(ArticleId: ArticleId, Kind: EnumEngagementKind.Bookmark, Delta: -1));
    }
}
