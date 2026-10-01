using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Lifecycle transitions of <see cref="ArticleLikeEntity" />. Its state lives in <c>Entities/ArticleLikeEntity.cs</c>.
/// </summary>
public partial class ArticleLikeEntity
{
    /// <summary>
    /// Declares this like's removal so the post-commit engagement consumer
    /// can decrement the article's cached like count.
    /// Called by the removal path immediately before the row is removed.
    /// </summary>
    public void MarkRemoved()
    {
        AddDomainEvent(new ArticleEngagedEvent(ArticleId: ArticleId, Kind: EnumEngagementKind.Like, Delta: -1));
    }
}
