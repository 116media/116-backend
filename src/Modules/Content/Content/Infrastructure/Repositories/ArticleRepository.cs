using _116.Content.Application.Editorial.Builders;
using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Application.Specifications;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IArticleRepository" /> for managing article and article image entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ArticleRepository(ContentDbContext context) : IArticleRepository
{
    /// <inheritdoc />
    public async Task<(List<ArticleEntity> Articles, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        string? search,
        EnumContentStatus? status,
        Guid? categoryId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ArticleEntity> query = context.Articles.Include(a => a.Category);

        Specification<ArticleEntity>? spec = new ArticleQueryBuilder()
            .WithSearch(search: search)
            .WithStatus(status: status)
            .WithCategory(categoryId: categoryId)
            .Build();

        if (spec is not null)
        {
            query = query.ApplySpecification(specification: spec);
        }

        int totalCount = await query.CountAsync(cancellationToken);

        List<ArticleEntity> articles = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (articles, totalCount);
    }

    /// <inheritdoc />
    public async Task<ArticleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleByIdSpecification(id: id);
        return await context
            .Articles.ApplySpecification(specification: specification)
            .Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Tags)
                .ThenInclude(t => t.Tag)
            .Include(a => a.Customer)
            .Include(a => a.PromotionLevel)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArticleEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleByIdSpecification(id: id);
        return await context
            .Articles.ApplySpecification(specification: specification)
            .Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Tags)
                .ThenInclude(t => t.Tag)
            .Include(a => a.Customer)
            .Include(a => a.PromotionLevel)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArticleEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleBySlugSpecification(slug: slug);
        return await context
            .Articles.ApplySpecification(specification: specification)
            .Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Tags)
                .ThenInclude(t => t.Tag)
            .Include(a => a.PromotionLevel)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleEntity>> GetPromotedAsync(CancellationToken cancellationToken = default)
    {
        var specification = new PromotedArticleSpecification();
        return await context
            .Articles.ApplySpecification(specification: specification)
            .Include(a => a.Category)
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleEntity>> GetPopularArticlesAsync(
        int limit,
        Guid? categoryId,
        Guid? excludeId,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<ArticleEntity> query = new PopularArticlesQueryBuilder()
            .WithCategory(categoryId: categoryId)
            .WithExcludeId(excludeId: excludeId)
            .WithLimit(limit: limit)
            .Build(context: context);

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleEntity>> GetAbandonedDraftsAsync(
        DateTime cutoff,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new AbandonedDraftSpecification(cutoff: cutoff);
        return await context
            .Articles.ApplySpecification(specification: specification)
            .Include(a => a.Images)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArticleEntity?> GetByOrderItemIdAsync(
        Guid orderItemId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleByOrderItemIdSpecification(orderItemId: orderItemId);
        return await context
            .Articles.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(ArticleEntity article, CancellationToken cancellationToken = default)
    {
        await context.Articles.AddAsync(article, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(ArticleEntity article)
    {
        context.Articles.Update(article);
    }

    /// <inheritdoc />
    public void Remove(ArticleEntity article)
    {
        context.Articles.Remove(article);
    }

    /// <inheritdoc />
    public async Task AddImageAsync(ArticleImageEntity image, CancellationToken cancellationToken = default)
    {
        await context.ArticleImages.AddAsync(image, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleImageEntity>> GetImagesByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleImageByArticleIdSpecification(articleId: articleId);
        return await context
            .ArticleImages.ApplySpecification(specification: specification)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveImages(IEnumerable<ArticleImageEntity> images)
    {
        context.ArticleImages.RemoveRange(images);
    }

    /// <inheritdoc />
    public async Task AddTagAsync(ArticleTagEntity tag, CancellationToken cancellationToken = default)
    {
        await context.ArticleTags.AddAsync(tag, cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveTag(ArticleTagEntity tag)
    {
        tag.MarkRemoved();
        context.ArticleTags.Remove(tag);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleTagEntity>> GetTagsByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleTagByArticleIdSpecification(articleId: articleId);
        return await context
            .ArticleTags.ApplySpecification(specification: specification)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleArtistEntity>> GetArtistsByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default
    )
    {
        return await context
            .ArticleArtists.Where(aa => aa.ArticleId == articleId)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task ReplaceArticleArtistsAsync(
        Guid articleId,
        IReadOnlyList<Guid> artistIds,
        CancellationToken cancellationToken = default
    )
    {
        List<ArticleArtistEntity> current = await context
            .ArticleArtists.Where(aa => aa.ArticleId == articleId)
            .ToListAsync(cancellationToken: cancellationToken);

        var desired = artistIds.ToHashSet();

        context.ArticleArtists.RemoveRange(current.Where(aa => !desired.Contains(aa.ArtistId)));

        var existing = current.Select(aa => aa.ArtistId).ToHashSet();

        foreach (Guid artistId in desired.Where(id => !existing.Contains(id)))
        {
            await context.ArticleArtists.AddAsync(
                ArticleArtistEntity.Create(id: Guid.NewGuid(), articleId: articleId, artistId: artistId),
                cancellationToken
            );
        }
    }

    /// <inheritdoc />
    public async Task<(List<ArticleEntity> Articles, int TotalCount)> GetPublishedByArtistAsync(
        Guid artistId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleByArtistSpecification(
            artistId: artistId,
            articleArtists: context.ArticleArtists
        );

        IQueryable<ArticleEntity> query = context.Articles.ApplySpecification(specification: specification);

        int totalCount = await query.CountAsync(cancellationToken: cancellationToken);

        List<ArticleEntity> articles = await query
            .OrderByDescending(a => a.PublishedAt)
            .ThenBy(a => a.Id)
            .Skip(count: (page - 1) * pageSize)
            .Take(count: pageSize)
            .ToListAsync(cancellationToken: cancellationToken);

        return (articles, totalCount);
    }

    /// <inheritdoc />
    public async Task<bool> HasLikedAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleLikeByUserAndArticleSpecification(userId: userId, articleId: articleId);
        return await context.ArticleLikes.ApplySpecification(specification: specification).AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddLikeAsync(ArticleLikeEntity like, CancellationToken cancellationToken = default)
    {
        await context.ArticleLikes.AddAsync(like, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveLikeAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleLikeByUserAndArticleSpecification(userId: userId, articleId: articleId);
        ArticleLikeEntity? like = await context
            .ArticleLikes.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (like is not null)
        {
            like.MarkRemoved();
            context.ArticleLikes.Remove(like);
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
        return await context
            .ArticleBookmarks.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddBookmarkAsync(ArticleBookmarkEntity bookmark, CancellationToken cancellationToken = default)
    {
        await context.ArticleBookmarks.AddAsync(bookmark, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveBookmarkAsync(Guid userId, Guid articleId, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleBookmarkByUserAndArticleSpecification(userId: userId, articleId: articleId);
        ArticleBookmarkEntity? bookmark = await context
            .ArticleBookmarks.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (bookmark is not null)
        {
            bookmark.MarkRemoved();
            context.ArticleBookmarks.Remove(bookmark);
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

        List<Guid> likedIds = await context
            .ArticleLikes.Where(like => like.UserId == userId && articleIds.Contains(like.ArticleId))
            .Select(like => like.ArticleId)
            .ToListAsync(cancellationToken);

        List<Guid> bookmarkedIds = await context
            .ArticleBookmarks.Where(bookmark => bookmark.UserId == userId && articleIds.Contains(bookmark.ArticleId))
            .Select(bookmark => bookmark.ArticleId)
            .ToListAsync(cancellationToken);

        return (likedIds.ToHashSet(), bookmarkedIds.ToHashSet());
    }

    /// <inheritdoc />
    public async Task AddShareAsync(ArticleShareEntity share, CancellationToken cancellationToken = default)
    {
        await context.ArticleShares.AddAsync(share, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddCommentAsync(ArticleCommentEntity comment, CancellationToken cancellationToken = default)
    {
        await context.ArticleComments.AddAsync(comment, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<ArticleCommentEntity> Comments, int TotalCount)> GetCommentsAsync(
        Guid articleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleCommentByArticleIdSpecification(articleId: articleId);
        IQueryable<ArticleCommentEntity> query = context.ArticleComments.ApplySpecification(
            specification: specification
        );

        int totalCount = await query.CountAsync(cancellationToken);

        List<ArticleCommentEntity> comments = await query
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (comments, totalCount);
    }

    /// <inheritdoc />
    public async Task<ArticleCommentEntity?> GetCommentByIdAsync(
        Guid commentId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleCommentByIdSpecification(commentId: commentId);
        return await context
            .ArticleComments.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArticleCommentEntity?> GetCommentByIdAsync(
        Guid commentId,
        Guid articleId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleCommentByIdInArticleSpecification(commentId: commentId, articleId: articleId);
        return await context
            .ArticleComments.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<(List<ArticleCommentEntity> Replies, int TotalCount)> GetRepliesAsync(
        Guid parentCommentId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleCommentReplyByParentSpecification(parentCommentId: parentCommentId);
        IQueryable<ArticleCommentEntity> query = context.ArticleComments.ApplySpecification(
            specification: specification
        );

        int totalCount = await query.CountAsync(cancellationToken);

        List<ArticleCommentEntity> replies = await query
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (replies, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, int>> GetReplyCountsAsync(
        IReadOnlyCollection<Guid> parentCommentIds,
        CancellationToken cancellationToken = default
    )
    {
        if (parentCommentIds.Count == 0)
        {
            return new Dictionary<Guid, int>();
        }

        Guid[] distinctIds = parentCommentIds.Distinct().ToArray();

        return await context
            .ArticleComments.Where(c =>
                c.ParentCommentId != null && distinctIds.Contains(c.ParentCommentId.Value) && !c.IsDeleted
            )
            .GroupBy(c => c.ParentCommentId!.Value)
            .Select(group => new { ParentCommentId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.ParentCommentId, row => row.Count, cancellationToken);
    }

    /// <inheritdoc />
    public void UpdateComment(ArticleCommentEntity comment)
    {
        context.ArticleComments.Update(comment);
    }

    /// <inheritdoc />
    public async Task<bool> HasLikedCommentAsync(
        Guid userId,
        Guid commentId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleCommentLikeByUserAndCommentSpecification(userId: userId, commentId: commentId);
        return await context
            .ArticleCommentLikes.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddCommentLikeAsync(ArticleCommentLikeEntity like, CancellationToken cancellationToken = default)
    {
        await context.ArticleCommentLikes.AddAsync(like, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveCommentLikeAsync(Guid userId, Guid commentId, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleCommentLikeByUserAndCommentSpecification(userId: userId, commentId: commentId);
        ArticleCommentLikeEntity? like = await context
            .ArticleCommentLikes.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (like is not null)
        {
            like.MarkRemoved();
            context.ArticleCommentLikes.Remove(like);
        }
    }

    /// <inheritdoc />
    public async Task<IReadOnlySet<Guid>> GetLikedCommentIdsAsync(
        Guid viewerUserId,
        IReadOnlyCollection<Guid> commentIds,
        CancellationToken cancellationToken = default
    )
    {
        if (commentIds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        List<Guid> likedIds = await context
            .ArticleCommentLikes.Where(like => like.UserId == viewerUserId && commentIds.Contains(like.CommentId))
            .Select(like => like.CommentId)
            .ToListAsync(cancellationToken);

        return likedIds.ToHashSet();
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
        IQueryable<ArticleBookmarkEntity> bookmarkQuery = context
            .ArticleBookmarks.ApplySpecification(specification: specification)
            .Where(b => b.Article.Status == EnumContentStatus.Published)
            .Include(b => b.Article)
                .ThenInclude(a => a.Category)
            .OrderByDescending(b => b.CreatedAt)
            .ThenBy(b => b.ArticleId);

        int totalCount = await bookmarkQuery.CountAsync(cancellationToken);

        List<BookmarkedArticleActivity> activities = await bookmarkQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BookmarkedArticleActivity(b.Article, b.CreatedAt ?? DateTime.MinValue))
            .ToListAsync(cancellationToken);

        return (activities, totalCount);
    }

    /// <inheritdoc />
    public async Task<(List<CommentedArticleActivity> Activities, int TotalCount)> GetCommentedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var commentByUserSpecification = new ArticleCommentByUserIdSpecification(userId: userId);
        var groupedQuery = context
            .ArticleComments.ApplySpecification(specification: commentByUserSpecification)
            .Where(comment => !comment.IsDeleted && comment.Article.Status == EnumContentStatus.Published)
            .GroupBy(comment => comment.ArticleId)
            .Select(group => new
            {
                ArticleId = group.Key,
                CommentCount = group.Count(),
                LastCommentedAt = group.Max(comment => comment.CreatedAt),
            });

        int totalCount = await groupedQuery.CountAsync(cancellationToken);
        var pageRows = await groupedQuery
            .OrderByDescending(row => row.LastCommentedAt)
            .ThenBy(row => row.ArticleId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (pageRows.Count == 0)
        {
            return ([], totalCount);
        }

        Guid[] articleIds = pageRows.Select(row => row.ArticleId).ToArray();
        Dictionary<Guid, ArticleEntity> articles = await context
            .Articles.Where(article => articleIds.Contains(article.Id))
            .Include(article => article.Category)
            .ToDictionaryAsync(article => article.Id, cancellationToken);

        List<ArticleCommentEntity> comments = await context
            .ArticleComments.ApplySpecification(specification: commentByUserSpecification)
            .Where(comment => !comment.IsDeleted && articleIds.Contains(comment.ArticleId))
            .OrderByDescending(comment => comment.CreatedAt)
            .ThenBy(comment => comment.Id)
            .ToListAsync(cancellationToken);

        Dictionary<Guid, ArticleCommentEntity> latestByArticle = comments
            .GroupBy(comment => comment.ArticleId)
            .ToDictionary(group => group.Key, group => group.First());

        List<CommentedArticleActivity> activities = pageRows
            .Select(row => new CommentedArticleActivity(
                articles[row.ArticleId],
                latestByArticle[row.ArticleId],
                row.CommentCount,
                row.LastCommentedAt ?? DateTime.MinValue
            ))
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
        IQueryable<ArticleLikeEntity> query = context
            .ArticleLikes.ApplySpecification(specification: specification)
            .Where(like => like.Article.Status == EnumContentStatus.Published)
            .Include(like => like.Article)
                .ThenInclude(article => article.Category)
            .OrderByDescending(like => like.CreatedAt)
            .ThenBy(like => like.ArticleId);

        int totalCount = await query.CountAsync(cancellationToken);
        List<ArticleActivity> activities = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(like => new ArticleActivity(like.Article, like.CreatedAt ?? DateTime.MinValue, 1, null))
            .ToListAsync(cancellationToken);

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
        var groupedQuery = context
            .ArticleShares.ApplySpecification(specification: specification)
            .Where(share => share.Article.Status == EnumContentStatus.Published)
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

        Guid[] articleIds = pageRows.Select(row => row.ArticleId).ToArray();
        Dictionary<Guid, ArticleEntity> articles = await context
            .Articles.Where(article => articleIds.Contains(article.Id))
            .Include(article => article.Category)
            .ToDictionaryAsync(article => article.Id, cancellationToken);

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
    public async Task<(List<ArticleCommentEntity> Comments, int TotalCount)> GetOwnCommentsForArticleAsync(
        Guid userId,
        Guid articleId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleCommentByUserAndArticleSpecification(userId: userId, articleId: articleId);
        IQueryable<ArticleCommentEntity> query = context
            .ArticleComments.ApplySpecification(specification: specification)
            .Where(comment => !comment.IsDeleted);

        int totalCount = await query.CountAsync(cancellationToken);
        List<ArticleCommentEntity> comments = await query
            .OrderByDescending(comment => comment.CreatedAt)
            .ThenBy(comment => comment.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (comments, totalCount);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleEntity>> GetActivePromotedBySpotAsync(
        int spotPriority,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleBySpotPrioritySpecification(spotPriority: spotPriority);
        return await context
            .Articles.ApplySpecification(specification: specification)
            .Include(a => a.Category)
            .Include(a => a.PromotionLevel)
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleEntity>> GetGossipFallbackAsync(
        Guid gossipCategoryId,
        int limit,
        IEnumerable<Guid> excludeIds,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new GossipArticleSpecification(gossipCategoryId: gossipCategoryId);
        return await context
            .Articles.ApplySpecification(specification: specification)
            .Where(a => !excludeIds.Contains(a.Id))
            .Include(a => a.Category)
            .OrderByDescending(a => a.PublishedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
