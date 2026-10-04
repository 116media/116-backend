using _116.Content.Application.Shared.DTOs;
using _116.Content.Domain.Entities;

namespace _116.Content.Application.Catalog.Services;

/// <summary>
/// Assembles category pricing DTOs, resolving the pricing tier each row names.
/// </summary>
public interface ICategoryPricingDtoService
{
    /// <summary>
    /// Builds the DTO of one pricing row.
    /// </summary>
    /// <param name="pricing">The pricing row.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<CategoryPricingDto> CreateAsync(CategoryPricingEntity pricing, CancellationToken ct = default);

    /// <summary>
    /// Builds the DTOs of a category's pricing rows, ordered by tier.
    /// </summary>
    /// <param name="pricing">The pricing rows.</param>
    /// <param name="ct">Token to cancel the operation.</param>
    Task<IReadOnlyList<CategoryPricingDto>> CreateManyAsync(
        IEnumerable<CategoryPricingEntity> pricing,
        CancellationToken ct = default
    );
}
