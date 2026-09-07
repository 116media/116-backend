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
/// Implementation of <see cref="IArticleCommentRepository" /> for managing article comments,
/// replies and comment likes.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ArticleCommentRepository(ContentDbContext context)
    : ContentRepository<ArticleEntity>(context),
        IArticleCommentRepository
{
    /// <inheritdoc />
    public async Task AddCommentAsync(ArticleCommentEntity comment, CancellationToken cancellationToken = default)
    {
        await Context.ArticleComments.AddAsync(comment, cancellationToken);
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

        IQueryable<ArticleCommentEntity> query = Context
            .ArticleComments.IgnoreQueryFilters()
            .ApplySpecification(specification: specification);

        int totalCount = await query.CountAsync(cancellationToken);

        List<ArticleCommentEntity> comments = await query
            .OrderBy(c => c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (comments, totalCount);
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
        IQueryable<ArticleCommentEntity> query = Context.ArticleComments.ApplySpecification(
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

        return await Context
            .ArticleComments.Where(c =>
                c.ParentCommentId != null && distinctIds.Contains(c.ParentCommentId.Value) && !c.IsDeleted
            )
            .GroupBy(c => c.ParentCommentId!.Value)
            .Select(group => new { ParentCommentId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => row.ParentCommentId, row => row.Count, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArticleCommentEntity?> GetCommentByIdAsync(
        Guid commentId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleCommentByIdSpecification(commentId: commentId);
        return await Context
            .ArticleComments.AsTracking()
            .ApplySpecification(specification: specification)
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
        return await Context
            .ArticleComments.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void UpdateComment(ArticleCommentEntity comment)
    {
        Context.ArticleComments.Update(comment);
    }

    /// <inheritdoc />
    public async Task<bool> HasLikedCommentAsync(
        Guid userId,
        Guid commentId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleCommentLikeByUserAndCommentSpecification(userId: userId, commentId: commentId);
        return await Context
            .ArticleCommentLikes.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddCommentLikeAsync(ArticleCommentLikeEntity like, CancellationToken cancellationToken = default)
    {
        await Context.ArticleCommentLikes.AddAsync(like, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveCommentLikeAsync(Guid userId, Guid commentId, CancellationToken cancellationToken = default)
    {
        var specification = new ArticleCommentLikeByUserAndCommentSpecification(userId: userId, commentId: commentId);
        ArticleCommentLikeEntity? like = await Context
            .ArticleCommentLikes.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (like is not null)
        {
            like.MarkRemoved();
            Context.ArticleCommentLikes.Remove(like);
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

        List<Guid> likedIds = await Context
            .ArticleCommentLikes.Where(like => like.UserId == viewerUserId && commentIds.Contains(like.CommentId))
            .Select(like => like.CommentId)
            .ToListAsync(cancellationToken);

        return likedIds.ToHashSet();
    }

    /// <inheritdoc />
    public Task<int> ApplyCommentLikeDeltaAsync(
        Guid commentId,
        int delta,
        CancellationToken cancellationToken = default
    )
    {
        return Context
            .ArticleComments.Where(c => c.Id == commentId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(c => c.LikeCount, c => Math.Max(0, c.LikeCount + delta)),
                cancellationToken: cancellationToken
            );
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
        IQueryable<ArticleCommentEntity> query = Context
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
    public async Task<(List<CommentedArticleActivity> Activities, int TotalCount)> GetCommentedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var commentByUserSpecification = new ArticleCommentByUserIdSpecification(userId: userId);
        var groupedQuery = Context
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
        Dictionary<Guid, ArticleEntity> articles = await Context
            .Articles.Where(article => articleIds.Contains(article.Id))
            .Include(article => article.Category)
            .ToDictionaryAsync(article => article.Id, cancellationToken);

        List<ArticleCommentEntity> comments = await Context
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
}
