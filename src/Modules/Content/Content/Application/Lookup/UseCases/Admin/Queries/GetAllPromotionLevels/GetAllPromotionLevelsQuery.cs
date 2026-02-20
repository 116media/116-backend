using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels;

/// <summary>
/// Query for retrieving all promotion levels.
/// </summary>
public record GetAllPromotionLevelsQuery : IQuery<GetAllPromotionLevelsResult>;

/// <summary>
/// Result of the <see cref="GetAllPromotionLevelsQuery" /> containing all promotion levels.
/// </summary>
/// <param name="PromotionLevels">The list of all promotion levels.</param>
public record GetAllPromotionLevelsResult(IReadOnlyList<PromotionLevelDto> PromotionLevels);
