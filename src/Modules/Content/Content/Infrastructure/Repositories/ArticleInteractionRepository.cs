using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Exceptions;
using _116.Shared.Application.Specifications;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IArticleInteractionRepository" /> for managing article likes,
/// bookmarks, shares and engagement counters.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ArticleInteractionRepository(ContentDbContext context)
    : ContentRepository<ArticleEntity>(context),
        IArticleInteractionRepository
{
    /// <inheritdoc />
    public async Task<bool> HasLikedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleLikeByUserAndArticleSpecification(userId: userId, articleId: articleId);
        return await Context.ArticleLikes.ApplySpecification(specification: specification).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddLikeAsync(ArticleLikeEntity like, CancellationToken cancellationToken = default)
    {
        await Context.ArticleLikes.AddAsync(like, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveLikeAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleLikeByUserAndArticleSpecification(userId: userId, articleId: articleId);
        ArticleLikeEntity? like = await Context
            .ArticleLikes.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (like is not null)
        {
            like.MarkRemoved();
            Context.ArticleLikes.Remove(like);
        }
    }

    /// <inheritdoc />
    public async Task<bool> HasBookmarkedAsync(
        Guid userId,
        Guid articleId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleBookmarkByUserAndArticleSpecification(userId: userId, articleId: articleId);
        return await Context
            .ArticleBookmarks.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddBookmarkAsync(ArticleBookmarkEntity bookmark, CancellationToken cancellationToken = default)
    {
        await Context.ArticleBookmarks.AddAsync(bookmark, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveBookmarkAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleBookmarkByUserAndArticleSpecification(userId: userId, articleId: articleId);
        ArticleBookmarkEntity? bookmark = await Context
            .ArticleBookmarks.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (bookmark is not null)
        {
            bookmark.MarkRemoved();
            Context.ArticleBookmarks.Remove(bookmark);
        }
    }

    /// <inheritdoc />
    public async Task<(IReadOnlySet<Guid> Liked, IReadOnlySet<Guid> Bookmarked)> GetLikedAndBookmarkedIdsAsync(
        Guid? currentUserId,
        IReadOnlyCollection<Guid> articleIds,
        CancellationToken cancellationToken = default
    )
    {
        if (currentUserId is not Guid userId || articleIds.Count == 0)
        {
            return (new HashSet<Guid>(), new HashSet<Guid>());
        }

        List<Guid> likedIds = await Context
            .ArticleLikes.ApplySpecification(specification: new ArticleLikeByUserIdSpecification(userId: userId))
            .Where(like => articleIds.Contains(like.ArticleId))
            .Select(like => like.ArticleId)
            .ToListAsync(cancellationToken);

        List<Guid> bookmarkedIds = await Context
            .ArticleBookmarks.ApplySpecification(
                specification: new ArticleBookmarkByUserIdSpecification(userId: userId)
            )
            .Where(bookmark => articleIds.Contains(bookmark.ArticleId))
            .Select(bookmark => bookmark.ArticleId)
            .ToListAsync(cancellationToken);

        return (likedIds.ToHashSet(), bookmarkedIds.ToHashSet());
    }

    /// <inheritdoc />
    public async Task AddShareAsync(ArticleShareEntity share, CancellationToken cancellationToken = default)
    {
        await Context.ArticleShares.AddAsync(share, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<BookmarkedArticleActivity> Activities, int TotalCount)> GetBookmarkedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleBookmarkByUserIdSpecification(userId: userId);
        IQueryable<ArticleBookmarkEntity> bookmarkQuery = Context
            .ArticleBookmarks.ApplySpecification(specification: specification)
            .Where(bookmark =>
                Context.Articles.Any(article =>
                    article.Id == bookmark.ArticleId && article.Status == EnumContentStatus.Published
                )
            )
            .OrderByDescending(bookmark => bookmark.CreatedAt)
            .ThenBy(bookmark => bookmark.ArticleId);

        int totalCount = await bookmarkQuery.CountAsync(cancellationToken);

        var pageRows = await bookmarkQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(bookmark => new { bookmark.ArticleId, bookmark.CreatedAt })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, ArticleEntity> articles = await LoadArticlesAsync(
            [.. pageRows.Select(row => row.ArticleId)],
            cancellationToken
        );

        List<BookmarkedArticleActivity> activities = pageRows
            .Select(row => new BookmarkedArticleActivity(articles[row.ArticleId], row.CreatedAt ?? DateTime.MinValue))
            .ToList();

        return (activities, totalCount);
    }

    /// <inheritdoc />
    public async Task<(List<ArticleActivity> Activities, int TotalCount)> GetLikedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleLikeByUserIdSpecification(userId: userId);
        IQueryable<ArticleLikeEntity> query = Context
            .ArticleLikes.ApplySpecification(specification: specification)
            .Where(like =>
                Context.Articles.Any(article =>
                    article.Id == like.ArticleId && article.Status == EnumContentStatus.Published
                )
            )
            .OrderByDescending(like => like.CreatedAt)
            .ThenBy(like => like.ArticleId);

        int totalCount = await query.CountAsync(cancellationToken);
        var pageRows = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(like => new { like.ArticleId, like.CreatedAt })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, ArticleEntity> articles = await LoadArticlesAsync(
            [.. pageRows.Select(row => row.ArticleId)],
            cancellationToken
        );

        List<ArticleActivity> activities = pageRows
            .Select(row => new ArticleActivity(articles[row.ArticleId], row.CreatedAt ?? DateTime.MinValue, 1, null))
            .ToList();

        return (activities, totalCount);
    }

    /// <inheritdoc />
    public async Task<(List<ArticleActivity> Activities, int TotalCount)> GetSharedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleShareByUserIdSpecification(userId: userId);
        var groupedQuery = Context
            .ArticleShares.ApplySpecification(specification: specification)
            .Where(share =>
                Context.Articles.Any(article =>
                    article.Id == share.ArticleId && article.Status == EnumContentStatus.Published
                )
            )
            .GroupBy(share => share.ArticleId)
            .Select(group => new
            {
                ArticleId = group.Key,
                InteractionCount = group.Count(),
                LastInteractedAt = group.Max(share => share.CreatedAt),
                LastShareChannel = group
                    .OrderByDescending(share => share.CreatedAt)
                    .ThenBy(share => share.Id)
                    .Select(share => share.ShareChannel)
                    .FirstOrDefault(),
            });

        int totalCount = await groupedQuery.CountAsync(cancellationToken);
        var pageRows = await groupedQuery
            .OrderByDescending(row => row.LastInteractedAt)
            .ThenBy(row => row.ArticleId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (pageRows.Count == 0)
        {
            return ([], totalCount);
        }

        Dictionary<Guid, ArticleEntity> articles = await LoadArticlesAsync(
            [.. pageRows.Select(row => row.ArticleId)],
            cancellationToken
        );

        List<ArticleActivity> activities = pageRows
            .Select(row => new ArticleActivity(
                articles[row.ArticleId],
                row.LastInteractedAt ?? DateTime.MinValue,
                row.InteractionCount,
                row.LastShareChannel
            ))
            .ToList();

        return (activities, totalCount);
    }

    /// <inheritdoc />
    public async Task<int?> ApplyEngagementDeltaAsync(
        Guid articleId,
        EnumEngagementKind kind,
        int delta,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ArticleEntity> row = Context.Articles.ApplySpecification(
            specification: new ArticleByIdSpecification(id: articleId)
        );

        // Math.Max reaches PostgreSQL as GREATEST, so a racing unlike cannot go negative.
        return kind switch
        {
            EnumEngagementKind.Like => await row.ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.LikeCount, e => Math.Max(0, e.LikeCount + delta)),
                cancellationToken: cancellationToken
            ),
            EnumEngagementKind.Bookmark => await row.ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.BookmarkCount, e => Math.Max(0, e.BookmarkCount + delta)),
                cancellationToken: cancellationToken
            ),
            EnumEngagementKind.Comment => await row.ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.CommentCount, e => Math.Max(0, e.CommentCount + delta)),
                cancellationToken: cancellationToken
            ),
            EnumEngagementKind.Share => await row.ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.ShareCount, e => Math.Max(0, e.ShareCount + delta)),
                cancellationToken: cancellationToken
            ),
            _ => null,
        };
    }

    /// <summary>
    /// Loads the articles a page of interaction rows points at, keyed by id.
    /// </summary>
    /// <param name="articleIds">The article ids on the page.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The articles by id.</returns>
    private async Task<Dictionary<Guid, ArticleEntity>> LoadArticlesAsync(
        Guid[] articleIds,
        CancellationToken cancellationToken
    )
    {
        if (articleIds.Length == 0)
        {
            return [];
        }

        return await Context
            .Articles.Where(article => articleIds.Contains(article.Id))
            .ToDictionaryAsync(article => article.Id, cancellationToken);
    }
}
