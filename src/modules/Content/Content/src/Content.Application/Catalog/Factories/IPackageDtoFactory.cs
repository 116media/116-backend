using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.Factories;

/// <summary>
/// Builds <see cref="PackageDto" /> projections, resolving the categories the slots reference —
/// and their pricing, which the calculated package price sums — in a single batch.
/// </summary>
public interface IPackageDtoFactory
{
    /// <summary>
    /// Builds the projection for one package.
    /// </summary>
    /// <param name="package">The package to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projection.</returns>
    Task<PackageDto> CreateAsync(PackageEntity package, CancellationToken ct = default);

    /// <summary>
    /// Builds the projections for a list of packages, resolving every slot category in one query.
    /// </summary>
    /// <param name="packages">The packages to project.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The projections, in the order supplied.</returns>
    Task<IReadOnlyList<PackageDto>> CreateManyAsync(
        IReadOnlyList<PackageEntity> packages,
        CancellationToken ct = default
    );

    /// <summary>
    /// Resolves the categories a set of packages' slots reference, in one query, each with its
    /// pricing loaded.
    /// </summary>
    /// <param name="packages">The packages whose slot categories to resolve.</param>
    /// <param name="ct">Token to observe for cancellation requests.</param>
    /// <returns>The categories, keyed by id.</returns>
    Task<IReadOnlyDictionary<Guid, CategoryEntity>> ResolveSlotCategoriesAsync(
        IReadOnlyList<PackageEntity> packages,
        CancellationToken ct = default
    );
}
