using _116.Content.Application.Shared.Search;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Content.Infrastructure.Persistence.Projections;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Search.Projectors;

/// <summary>
/// Projects articles into the search documents. An article's searchable text is frozen once
/// it is published, so in practice only publication state and the SEO and cover fields move
/// after the first projection.
/// </summary>
/// <param name="context">The Content database context.</param>
/// <param name="writer">The shared document writer.</param>
public class ArticleSearchProjector(ContentDbContext context, SearchDocumentWriter writer) : ISearchDocumentProjector
{
    /// <inheritdoc />
    public EnumCoreContentType Type => EnumCoreContentType.Article;

    /// <inheritdoc />
    public async Task ProjectAsync(Guid entityId, CancellationToken cancellationToken = default)
    {
        ArticleEntity? article = await context
            .Articles.AsNoTracking()
            .FirstOrDefaultAsync(candidate => candidate.Id == entityId, cancellationToken);

        if (article is null)
        {
            await writer.RemoveAsync(Type, entityId, cancellationToken);
            return;
        }

        await writer.UpsertAsync(ToDocument(article), cancellationToken);
    }

    /// <inheritdoc />
    public Task RemoveAsync(Guid entityId, CancellationToken cancellationToken = default) =>
        writer.RemoveAsync(Type, entityId, cancellationToken);

    /// <inheritdoc />
    public async Task<int> RebuildAsync(CancellationToken cancellationToken = default)
    {
        List<ArticleEntity> articles = await context.Articles.AsNoTracking().ToListAsync(cancellationToken);

        return await writer.ReplaceAllAsync([.. articles.Select(ToDocument)], cancellationToken);
    }

    private static SearchDocument ToDocument(ArticleEntity article) =>
        new()
        {
            Type = EnumCoreContentType.Article,
            EntityId = article.Id,
            Slug = article.Slug.Value,
            IsVisible = article.Status == EnumContentStatus.Published,
            Title = article.Title,
            Summary = article.Headline,
            Keywords = $"{article.MetaTitle} {article.MetaDescription}".Trim(),
            Body = article.Body,
            ImageFileId = article.CoverImageFileId,
            CategoryId = article.CategoryId,
            PublishedAt = article.PublishedAt,
        };
}
