using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPromotionLevels;

/// <summary>
/// Handles the <see cref="GetAllPromotionLevelsQuery" /> to retrieve all promotion levels.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetAllPromotionLevelsHandler(ILookupRepository lookupRepository, IMapper mapper)
    : IQueryHandler<GetAllPromotionLevelsQuery, GetAllPromotionLevelsResult>
{
    /// <inheritdoc />
    public async Task<GetAllPromotionLevelsResult> Handle(
        GetAllPromotionLevelsQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<PromotionLevelEntity> promotionLevels = await lookupRepository.GetAllPromotionLevelsAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PromotionLevelDto> dtoList = promotionLevels.ToPromotionLevelDtos(mapper);
        return new GetAllPromotionLevelsResult(PromotionLevels: dtoList);
    }
}
