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
        return await context.ArticleImages.Where(i => i.ArticleId == articleId).ToListAsync(cancellationToken);
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
        return await context.ArticleTags.Where(t => t.ArticleId == articleId).ToListAsync(cancellationToken);
    }
}
