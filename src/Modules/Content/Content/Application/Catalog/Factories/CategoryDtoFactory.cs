using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
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
public class CategoryDtoFactory(IMapper mapper, IFileStorageService fileStorage) : ICategoryDtoFactory
{
    /// <inheritdoc />
    public async Task<CategoryDto> CreateAsync(CategoryEntity category, CancellationToken ct = default)
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> posters = await ResolvePostersAsync([category], ct);

        return category.ToCategoryDto(mapper, posters);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<CategoryDto>> CreateManyAsync(
        IReadOnlyList<CategoryEntity> categories,
        CancellationToken ct = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> posters = await ResolvePostersAsync(categories, ct);

        return categories.Select(category => category.ToCategoryDto(mapper, posters)).ToList();
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
