using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels;

/// <summary>
/// Query for retrieving all active promotion levels visible to the public.
/// </summary>
public record PublicGetActivePromotionLevelsQuery : IQuery<PublicGetActivePromotionLevelsResult>, ICacheableRequest
{
    /// <inheritdoc />
    public string CacheKey => "lookup:promotion_levels:active";

    /// <inheritdoc />
    public TimeSpan Ttl => TimeSpan.FromMinutes(30);

    /// <inheritdoc />
    public IReadOnlyList<string> CacheTags => [ContentCacheTags.Lookups];
}

/// <summary>
/// Result of the <see cref="PublicGetActivePromotionLevelsQuery" /> containing all active promotion levels.
/// </summary>
/// <param name="PromotionLevels">The list of active promotion levels.</param>
public record PublicGetActivePromotionLevelsResult(IReadOnlyList<PromotionLevelDto> PromotionLevels);
