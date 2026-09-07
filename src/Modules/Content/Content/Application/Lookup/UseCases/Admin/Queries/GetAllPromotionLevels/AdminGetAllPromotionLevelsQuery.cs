using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels;

/// <summary>
/// Query for retrieving all promotion levels.
/// </summary>
/// <param name="Search">
/// Optional search term to filter promotion levels by name (case-insensitive, partial match).
/// </param>
public record AdminGetAllPromotionLevelsQuery(string? Search = null)
    : IQuery<AdminGetAllPromotionLevelsResult>,
        IConditionallyCacheableRequest
{
    /// <inheritdoc />
    /// <remarks>
    /// Free-text search produces an unbounded key space, so those results are never stored.
    /// </remarks>
    public bool IsCacheable => string.IsNullOrWhiteSpace(Search);

    /// <inheritdoc />
    public string CacheKey => "lookup:promotion_levels:admin";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Lookups];
}

/// <summary>
/// Result of the <see cref="AdminGetAllPromotionLevelsQuery" /> containing all promotion levels.
/// </summary>
/// <param name="PromotionLevels">The list of all promotion levels.</param>
public record AdminGetAllPromotionLevelsResult(IReadOnlyList<PromotionLevelDto> PromotionLevels);
