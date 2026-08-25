using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for article likes, bookmarks, shares and engagement counters.
/// </summary>
public interface IArticleInteractionRepository
{
    /// <summary>
    /// Throws when no article row exists, without materializing the aggregate.
    /// </summary>
    /// <param name="articleId">The article identifier.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task ExistsOrThrowAsync(Guid articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the user has already liked the given article.
    /// </summary>
    Task<bool> HasLikedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a like record to the repository.
    /// </summary>
    Task AddLikeAsync(ArticleLikeEntity like, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the like record for the given user and article.
    /// </summary>
    Task RemoveLikeAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the user has already bookmarked the given article.
    /// </summary>
    Task<bool> HasBookmarkedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a bookmark record to the repository.
    /// </summary>
    Task AddBookmarkAsync(ArticleBookmarkEntity bookmark, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the bookmark record for the given user and article.
    /// </summary>
    Task RemoveBookmarkAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the subsets of the given article ids that the specified user has liked and
    /// bookmarked, used to stamp the per-user interaction flags on article DTOs. Executes
    /// one query per interaction type; returns empty sets for an anonymous caller
    /// (<paramref name="currentUserId" /> null) or an empty id list, running no queries
    /// in that case.
    /// </summary>
    /// <param name="currentUserId">The authenticated caller's id, or null when anonymous.</param>
    /// <param name="articleIds">The candidate article ids, typically one page of a feed.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The liked and bookmarked id sets for the current user.</returns>
    Task<(IReadOnlySet<Guid> Liked, IReadOnlySet<Guid> Bookmarked)> GetLikedAndBookmarkedIdsAsync(
        Guid? currentUserId,
        IReadOnlyCollection<Guid> articleIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a share record to the repository.
    /// </summary>
    Task AddShareAsync(ArticleShareEntity share, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of articles bookmarked by the given user, along with total count.
    /// </summary>
    Task<(List<BookmarkedArticleActivity> Activities, int TotalCount)> GetBookmarkedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns published articles currently liked by the user.
    /// </summary>
    Task<(List<ArticleActivity> Activities, int TotalCount)> GetLikedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns published articles shared by the user, grouped by article.
    /// </summary>
    Task<(List<ArticleActivity> Activities, int TotalCount)> GetSharedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Applies a signed delta to one engagement counter in a single statement, clamped at zero.
    /// Set-based by design: two concurrent likes cannot both read the same value and write the
    /// same increment, and the change tracker never sees the row, so the audit columns keep
    /// whatever the last editorial write set them to.
    /// </summary>
    /// <param name="articleId">The article whose counter moves.</param>
    /// <param name="kind">The engagement whose counter to move.</param>
    /// <param name="delta">The signed amount to apply.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>
    /// Rows updated, <c>0</c> when the row no longer exists, or <c>null</c> when this entity
    /// carries no counter for the kind — a normal event, not a missing row.
    /// </returns>
    Task<int?> ApplyEngagementDeltaAsync(
        Guid articleId,
        EnumEngagementKind kind,
        int delta,
        CancellationToken cancellationToken = default
    );
}
