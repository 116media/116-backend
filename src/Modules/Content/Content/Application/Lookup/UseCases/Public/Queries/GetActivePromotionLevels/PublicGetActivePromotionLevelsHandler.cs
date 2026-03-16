using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Public.Queries.GetActivePromotionLevels;

/// <summary>
/// Handles the <see cref="PublicGetActivePromotionLevelsQuery" /> to retrieve all active promotion levels.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class PublicGetActivePromotionLevelsHandler(ILookupRepository lookupRepository, IMapper mapper)
    : IQueryHandler<PublicGetActivePromotionLevelsQuery, PublicGetActivePromotionLevelsResult>
{
    /// <inheritdoc />
    public async Task<PublicGetActivePromotionLevelsResult> Handle(
        PublicGetActivePromotionLevelsQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<PromotionLevelEntity> promotionLevels = await lookupRepository.GetActivePromotionLevelsAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PromotionLevelDto> dtoList = promotionLevels.ToPromotionLevelDtos(mapper);
        return new PublicGetActivePromotionLevelsResult(PromotionLevels: dtoList);
    }
}
