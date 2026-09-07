using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers;

/// <summary>
/// Query for retrieving all pricing tiers.
/// </summary>
/// <param name="Search">
/// Optional search term to filter pricing tiers by name or description (case-insensitive, partial match).
/// </param>
public record AdminGetAllPricingTiersQuery(string? Search = null)
    : IQuery<AdminGetAllPricingTiersResult>,
        IConditionallyCacheableRequest
{
    /// <inheritdoc />
    /// <remarks>
    /// Free-text search produces an unbounded key space, so those results are never stored.
    /// </remarks>
    public bool IsCacheable => string.IsNullOrWhiteSpace(Search);

    /// <inheritdoc />
    public string CacheKey => "lookup:pricing_tiers:admin";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Lookups];
}

/// <summary>
/// Result of the <see cref="AdminGetAllPricingTiersQuery" /> containing all pricing tiers.
/// </summary>
/// <param name="PricingTiers">The list of all pricing tiers.</param>
public record AdminGetAllPricingTiersResult(IReadOnlyList<PricingTierDto> PricingTiers);
