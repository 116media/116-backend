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
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleEntity>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        var specification = new FeaturedArticleSpecification();
        return await context
            .Articles.ApplySpecification(specification: specification)
            .Include(a => a.Category)
            .OrderByDescending(a => a.PublishedAt)
            .ToListAsync(cancellationToken);
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
            context.ArticleBookmarks.Remove(bookmark);
        }
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
    public void UpdateComment(ArticleCommentEntity comment)
    {
        context.ArticleComments.Update(comment);
    }

    /// <inheritdoc />
    public async Task<(List<ArticleEntity> Articles, int TotalCount)> GetBookmarkedArticlesAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new ArticleBookmarkByUserIdSpecification(userId: userId);
        IQueryable<ArticleEntity> query = context
            .ArticleBookmarks.ApplySpecification(specification: specification)
            .OrderByDescending(b => b.CreatedAt)
            .Select(b => b.Article)
            .Include(a => a.Category);

        int totalCount = await query.CountAsync(cancellationToken);

        List<ArticleEntity> articles = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (articles, totalCount);
    }
}
