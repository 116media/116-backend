using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using MapsterMapper;

namespace _116.Content.Application.Catalog.Factories;

/// <summary>
/// Factory implementation building package projections from a pre-resolved category map.
/// </summary>
/// <param name="mapper">Injected IMapper instance.</param>
/// <param name="categoryRepository">Repository resolving the slot categories and their pricing.</param>
public class PackageDtoFactory(IMapper mapper, ICategoryRepository categoryRepository) : IPackageDtoFactory
{
    /// <inheritdoc />
    public async Task<PackageDto> CreateAsync(PackageEntity package, CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, CategoryEntity> categories = await ResolveSlotCategoriesAsync([package], ct);

        return package.ToPackageDto(mapper, categories);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PackageDto>> CreateManyAsync(
        IReadOnlyList<PackageEntity> packages,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, CategoryEntity> categories = await ResolveSlotCategoriesAsync(packages, ct);

        return packages.ToPackageDtos(mapper, categories);
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<Guid, CategoryEntity>> ResolveSlotCategoriesAsync(
        IReadOnlyList<PackageEntity> packages,
        CancellationToken ct = default
    )
    {
        return categoryRepository.GetByIdsAsync(
            ids:
            [
                .. packages
                    .SelectMany(package => package.Slots)
                    .Where(slot => slot.CategoryId.HasValue)
                    .Select(slot => slot.CategoryId!.Value)
                    .Distinct(),
            ],
            cancellationToken: ct
        );
    }
}
