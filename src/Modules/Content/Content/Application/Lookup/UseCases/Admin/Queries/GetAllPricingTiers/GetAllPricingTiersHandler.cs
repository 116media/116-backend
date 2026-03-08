using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers;

/// <summary>
/// Handles the <see cref="GetAllPricingTiersQuery" /> to retrieve all pricing tiers.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class GetAllPricingTiersHandler(ILookupRepository lookupRepository, IMapper mapper)
    : IQueryHandler<GetAllPricingTiersQuery, GetAllPricingTiersResult>
{
    /// <inheritdoc />
    public async Task<GetAllPricingTiersResult> Handle(
        GetAllPricingTiersQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<PricingTierEntity> pricingTiers = await lookupRepository.GetAllPricingTiersAsync(
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PricingTierDto> dtoList = pricingTiers.ToPricingTierDtos(mapper);
        return new GetAllPricingTiersResult(PricingTiers: dtoList);
    }
}
