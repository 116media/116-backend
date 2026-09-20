using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using MapsterMapper;

namespace _116.Content.Application.Catalog.Factories;

/// <summary>
/// Factory implementation building category projections from a pre-resolved poster map.
/// </summary>
/// <param name="mapper">Injected IMapper instance.</param>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="contentTypeRepository">Repository resolving the classifying content types.</param>
/// <param name="pricingTierRepository">Repository resolving the priced tiers.</param>
public class CategoryDtoFactory(
    IMapper mapper,
    IFileStorageService fileStorage,
    IContentTypeRepository contentTypeRepository,
    IPricingTierRepository pricingTierRepository
) : ICategoryDtoFactory
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

        IReadOnlyDictionary<Guid, PricingTierEntity> pricingTiers = await pricingTierRepository.GetByIdsAsync(
            ids:
            [
                .. categories
                    .SelectMany(category => category.Pricing)
                    .Select(pricing => pricing.PricingTierId)
                    .Distinct(),
            ],
            cancellationToken: ct
        );

        return new CategoryLookups(Posters: posters, ContentTypes: contentTypes, PricingTiers: pricingTiers);
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
}
