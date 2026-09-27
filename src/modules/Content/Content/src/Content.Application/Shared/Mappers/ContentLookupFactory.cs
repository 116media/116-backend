using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Shared.Mappers;

/// <summary>
/// Resolver implementation: one query per kind of row, never one per projected entity.
/// </summary>
/// <param name="categoryRepository">Repository resolving the filing categories.</param>
/// <param name="customerRepository">Repository resolving the commissioning customers.</param>
/// <param name="promotionLevelRepository">Repository resolving the purchased promotion levels.</param>
/// <param name="tagRepository">Repository resolving the applied tags.</param>
public class ContentLookupFactory(
    ICategoryRepository categoryRepository,
    ICustomerRepository customerRepository,
    IPromotionLevelRepository promotionLevelRepository,
    ITagRepository tagRepository
) : IContentLookupFactory
{
    /// <inheritdoc />
    public Task<ContentLookups> ResolveForArticlesAsync(
        IReadOnlyList<ArticleEntity> articles,
        CancellationToken ct = default
    )
    {
        return ResolveAsync(
            categoryIds: [.. articles.Select(article => article.CategoryId)],
            customerIds: [.. articles.Select(article => article.CustomerId)],
            promotionLevelIds: [.. articles.Select(article => article.PromotionLevelId)],
            tagIds: [.. articles.SelectMany(article => article.Tags).Select(tag => tag.TagId)],
            ct: ct
        );
    }

    /// <inheritdoc />
    public Task<ContentLookups> ResolveForVideosAsync(IReadOnlyList<VideoEntity> videos, CancellationToken ct = default)
    {
        return ResolveAsync(
            categoryIds: [.. videos.Select(video => video.CategoryId)],
            customerIds: [.. videos.Select(video => video.CustomerId)],
            promotionLevelIds: [.. videos.Select(video => video.PromotionLevelId)],
            tagIds: [.. videos.SelectMany(video => video.Tags).Select(tag => tag.TagId)],
            ct: ct
        );
    }

    /// <inheritdoc />
    public Task<ContentLookups> ResolveForLyricsAsync(
        IReadOnlyList<LyricsEntity> lyrics,
        CancellationToken ct = default
    )
    {
        return ResolveAsync(
            categoryIds: [.. lyrics.Select(page => page.CategoryId)],
            customerIds: [.. lyrics.Select(page => page.CustomerId)],
            promotionLevelIds: [],
            tagIds: [.. lyrics.SelectMany(page => page.Tags).Select(tag => tag.TagId)],
            ct: ct
        );
    }

    /// <summary>
    /// Runs the four batch lookups, skipping any whose id set is empty.
    /// </summary>
    /// <param name="categoryIds">The category ids to resolve.</param>
    /// <param name="customerIds">The customer ids to resolve; nulls are dropped.</param>
    /// <param name="promotionLevelIds">The promotion level ids to resolve; nulls are dropped.</param>
    /// <param name="tagIds">The tag ids to resolve.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The resolved lookups.</returns>
    private async Task<ContentLookups> ResolveAsync(
        IReadOnlyList<Guid> categoryIds,
        IReadOnlyList<Guid?> customerIds,
        IReadOnlyList<Guid?> promotionLevelIds,
        IReadOnlyList<Guid> tagIds,
        CancellationToken ct
    )
    {
        IReadOnlyDictionary<Guid, CategoryEntity> categories = await categoryRepository.GetByIdsAsync(
            ids: [.. categoryIds.Distinct()],
            cancellationToken: ct
        );

        IReadOnlyDictionary<Guid, CustomerEntity> customers = await customerRepository.GetByIdsAsync(
            ids: [.. customerIds.OfType<Guid>().Distinct()],
            cancellationToken: ct
        );

        IReadOnlyDictionary<Guid, PromotionLevelEntity> promotionLevels = await promotionLevelRepository.GetByIdsAsync(
            ids: [.. promotionLevelIds.OfType<Guid>().Distinct()],
            cancellationToken: ct
        );

        IReadOnlyDictionary<Guid, TagEntity> tags = await tagRepository.GetByIdsAsync(
            ids: [.. tagIds.Distinct()],
            cancellationToken: ct
        );

        return new ContentLookups(
            Categories: categories,
            Customers: customers,
            PromotionLevels: promotionLevels,
            Tags: tags
        );
    }
}
