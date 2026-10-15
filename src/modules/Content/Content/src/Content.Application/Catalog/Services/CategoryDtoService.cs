using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Catalog.Services;

/// <summary>
/// Service implementation building category projections from a pre-resolved poster map.
/// </summary>
/// <param name="mapper">Injected IMapper instance.</param>
/// <param name="fileStorage">Storage's file contract.</param>
/// <param name="contentTypeRepository">Repository resolving the classifying content types.</param>
/// <param name="pricingTierRepository">Repository resolving the priced tiers.</param>
/// <param name="videoRepository">Repository counting a video category's published videos.</param>
/// <param name="articleRepository">Repository counting an article category's published articles.</param>
public class CategoryDtoService(
    IMapper mapper,
    IFileStorageService fileStorage,
    IContentTypeRepository contentTypeRepository,
    IPricingTierRepository pricingTierRepository,
    IVideoRepository videoRepository,
    IArticleRepository articleRepository
) : ICategoryDtoService
{
    /// <inheritdoc />
    public async Task<CategoryDto> CreateAsync(CategoryEntity category, CancellationToken ct = default)
    {
        CategoryLookups lookups = await ResolveLookupsAsync([category], ct);

        return category.ToCategoryDto(mapper, lookups);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryDto>> CreateManyAsync(
        IReadOnlyList<CategoryEntity> categories,
        CancellationToken ct = default
    )
    {
        CategoryLookups lookups = await ResolveLookupsAsync(categories, ct);

        return categories.Select(category => category.ToCategoryDto(mapper, lookups)).ToList();
    }

    /// <inheritdoc />
    public async Task<CategoryLookups> ResolveLookupsAsync(
        IReadOnlyList<CategoryEntity> categories,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> posters = await ResolvePostersAsync(categories, ct);

        IReadOnlyDictionary<Guid, ContentTypeEntity> contentTypes = await contentTypeRepository.GetByIdsAsync(
            ids: categories.Select(category => category.ContentTypeId).Distinct().ToList(),
            cancellationToken: ct
        );

        List<Guid> pricingTierIds =
        [
            .. categories.SelectMany(category => category.Pricing).Select(pricing => pricing.PricingTierId).Distinct(),
        ];

        IReadOnlyDictionary<Guid, PricingTierEntity> pricingTiers =
            pricingTierIds.Count == 0
                ? new Dictionary<Guid, PricingTierEntity>()
                : await pricingTierRepository.GetByIdsAsync(ids: pricingTierIds, cancellationToken: ct);

        IReadOnlyDictionary<Guid, int> publishedCounts = await ResolvePublishedCountsAsync(
            categories: categories,
            contentTypes: contentTypes,
            ct: ct
        );

        return new CategoryLookups(
            Posters: posters,
            ContentTypes: contentTypes,
            PricingTiers: pricingTiers,
            PublishedCounts: publishedCounts
        );
    }

    /// <summary>
    /// Resolves every distinct poster the supplied categories reference, in one query.
    /// </summary>
    /// <param name="categories">The categories whose posters to resolve.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The posters, keyed by file id.</returns>
    private Task<IReadOnlyDictionary<Guid, FileReferenceDto>> ResolvePostersAsync(
        IReadOnlyList<CategoryEntity> categories,
        CancellationToken ct
    )
    {
        return fileStorage.ResolveManyAsync(
            categories.Where(c => c.PosterFileId.HasValue).Select(c => c.PosterFileId!.Value).Distinct().ToList(),
            ct
        );
    }

    /// <summary>
    /// Counts the published items each category holds, asking only the repository that owns the
    /// category's content type. A lookup with no category of a given type runs no query for it.
    /// </summary>
    /// <param name="categories">The categories to count.</param>
    /// <param name="contentTypes">The already-resolved content types, keyed by id.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>Published counts keyed by category id.</returns>
    private async Task<IReadOnlyDictionary<Guid, int>> ResolvePublishedCountsAsync(
        IReadOnlyList<CategoryEntity> categories,
        IReadOnlyDictionary<Guid, ContentTypeEntity> contentTypes,
        CancellationToken ct
    )
    {
        List<Guid> videoCategoryIds = IdsOfType(categories, contentTypes, EnumCoreContentType.Video);
        List<Guid> articleCategoryIds = IdsOfType(categories, contentTypes, EnumCoreContentType.Article);

        IReadOnlyDictionary<Guid, int> videoCounts =
            videoCategoryIds.Count == 0
                ? new Dictionary<Guid, int>()
                : await videoRepository.CountPublishedByCategoriesAsync(
                    categoryIds: videoCategoryIds,
                    cancellationToken: ct
                );

        IReadOnlyDictionary<Guid, int> articleCounts =
            articleCategoryIds.Count == 0
                ? new Dictionary<Guid, int>()
                : await articleRepository.CountPublishedByCategoriesAsync(
                    categoryIds: articleCategoryIds,
                    cancellationToken: ct
                );

        return categories
            .Select(category => category.Id)
            .Distinct()
            .ToDictionary(id => id, id => videoCounts.GetValueOrDefault(id) + articleCounts.GetValueOrDefault(id));
    }

    /// <summary>
    /// The ids of the categories classified under the supplied core content type.
    /// </summary>
    /// <param name="categories">The categories to filter.</param>
    /// <param name="contentTypes">The already-resolved content types, keyed by id.</param>
    /// <param name="contentType">The core content type to match by name.</param>
    /// <returns>The matching category ids.</returns>
    private static List<Guid> IdsOfType(
        IReadOnlyList<CategoryEntity> categories,
        IReadOnlyDictionary<Guid, ContentTypeEntity> contentTypes,
        EnumCoreContentType contentType
    )
    {
        return
        [
            .. categories
                .Where(category =>
                    contentTypes.TryGetValue(category.ContentTypeId, out ContentTypeEntity? resolved)
                    && string.Equals(resolved.Name, contentType.ToString(), StringComparison.OrdinalIgnoreCase)
                )
                .Select(category => category.Id)
                .Distinct(),
        ];
    }
}
