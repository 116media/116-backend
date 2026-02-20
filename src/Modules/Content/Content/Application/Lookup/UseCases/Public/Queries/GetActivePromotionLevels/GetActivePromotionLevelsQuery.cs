using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels;

/// <summary>
/// Query for retrieving all active promotion levels visible to the public.
/// </summary>
public record GetActivePromotionLevelsQuery : IQuery<GetActivePromotionLevelsResult>;

/// <summary>
/// Result of the <see cref="GetActivePromotionLevelsQuery" /> containing all active promotion levels.
/// </summary>
/// <param name="PromotionLevels">The list of active promotion levels.</param>
public record GetActivePromotionLevelsResult(IReadOnlyList<PromotionLevelDto> PromotionLevels);
