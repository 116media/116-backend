using _116.Content.Application.Editorial.Builders;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IArticleRepository" /> for managing article and article image entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class ArticleRepository(ContentDbContext context) : ContentRepository<ArticleEntity>(context), IArticleRepository
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
        IQueryable<ArticleEntity> query = Context.Articles.Include(a => a.Category);

        if (!string.IsNullOrWhiteSpace(search))
        {
            string pattern = $"%{search}%";
            query = query.Where(article =>
                EF.Functions.ILike(article.Title, pattern)
                || EF.Functions.ILike(article.Headline, pattern)
                || EF.Functions.ILike(article.Body, pattern)
                || (article.MetaTitle != null && EF.Functions.ILike(article.MetaTitle, pattern))
                || (article.MetaDescription != null && EF.Functions.ILike(article.MetaDescription, pattern))
            );
        }

        if (status.HasValue)
        {
            query = query.Where(article => article.Status == status.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(article => article.CategoryId == categoryId.Value);
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
    public override async Task<ArticleEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await Context
            .Articles.Where(article => article.Id == id)
            .Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Tags)
                .ThenInclude(t => t.Tag)
            .Include(a => a.Customer)
            .Include(a => a.PromotionLevel)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<ArticleEntity> GetByIdOrThrowAsync(
        Guid id,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .Articles.AsTracking()
            .Where(article => article.Id == id)
            .Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Tags)
                .ThenInclude(t => t.Tag)
            .Include(a => a.Customer)
            .Include(a => a.PromotionLevel)
            .AsSplitQuery()
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArticleEntity?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await Context
            .Articles.AsTracking()
            .Where(article => EF.Functions.ILike(article.Slug, slug))
            .Include(a => a.Category)
            .Include(a => a.Images)
            .Include(a => a.Tags)
                .ThenInclude(t => t.Tag)
            .Include(a => a.PromotionLevel)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleEntity>> GetPromotedAsync(CancellationToken cancellationToken = default)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return await Context
            .Articles.Where(article =>
                article.IsPromoted
                && article.Status == EnumContentStatus.Published
                && (article.PromotedUntil == null || article.PromotedUntil > now)
            )
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
            .Build(source: Context.Articles.Include(a => a.Category));

        return await query.ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleEntity>> GetAbandonedDraftsAsync(
        DateTime cutoff,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .Articles.Where(article =>
                article.Status == EnumContentStatus.Draft
                && article.Body == string.Empty
                && article.Headline == string.Empty
                && article.CreatedAt < cutoff
            )
            .Include(a => a.Images)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ArticleEntity?> GetByOrderItemIdAsync(
        Guid orderItemId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .Articles.AsTracking()
            .FirstOrDefaultAsync(article => article.OrderItemId == orderItemId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddImageAsync(ArticleImageEntity image, CancellationToken cancellationToken = default)
    {
        await Context.ArticleImages.AddAsync(image, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleImageEntity>> GetImagesByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .ArticleImages.AsTracking()
            .Where(image => image.ArticleId == articleId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveImages(IEnumerable<ArticleImageEntity> images)
    {
        Context.ArticleImages.RemoveRange(images);
    }

    /// <inheritdoc />
    public async Task AddTagAsync(ArticleTagEntity tag, CancellationToken cancellationToken = default)
    {
        await Context.ArticleTags.AddAsync(tag, cancellationToken);
    }

    /// <inheritdoc />
    public void RemoveTag(ArticleTagEntity tag)
    {
        tag.MarkRemoved();
        Context.ArticleTags.Remove(tag);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleTagEntity>> GetTagsByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .ArticleTags.AsTracking()
            .Where(tag => tag.ArticleId == articleId)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ArticleArtistEntity>> GetArtistsByArticleIdAsync(
        Guid articleId,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
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
        List<ArticleArtistEntity> current = await Context
            .ArticleArtists.AsTracking()
            .Where(aa => aa.ArticleId == articleId)
            .ToListAsync(cancellationToken: cancellationToken);

        var desired = artistIds.ToHashSet();

        Context.ArticleArtists.RemoveRange(current.Where(aa => !desired.Contains(aa.ArtistId)));

        var existing = current.Select(aa => aa.ArtistId).ToHashSet();

        foreach (Guid artistId in desired.Where(id => !existing.Contains(id)))
        {
            await Context.ArticleArtists.AddAsync(
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
        IQueryable<ArticleEntity> query = Context.Articles.Where(article =>
            article.Status == EnumContentStatus.Published
            && Context.ArticleArtists.Any(aa => aa.ArticleId == article.Id && aa.ArtistId == artistId)
        );

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
    public async Task<IReadOnlyList<ArticleEntity>> GetActivePromotedBySpotAsync(
        int spotPriority,
        CancellationToken cancellationToken = default
    )
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return await Context
            .Articles.Where(article =>
                article.IsPromoted
                && article.Status == EnumContentStatus.Published
                && (article.PromotedUntil == null || article.PromotedUntil > now)
                && article.PromotionLevel != null
                && article.PromotionLevel.SpotPriority == spotPriority
            )
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
        return await Context
            .Articles.Where(article =>
                article.Status == EnumContentStatus.Published && article.CategoryId == gossipCategoryId
            )
            .Where(a => !excludeIds.Contains(a.Id))
            .Include(a => a.Category)
            .OrderByDescending(a => a.PublishedAt)
            .Take(limit)
            .ToListAsync(cancellationToken);
    }
}
