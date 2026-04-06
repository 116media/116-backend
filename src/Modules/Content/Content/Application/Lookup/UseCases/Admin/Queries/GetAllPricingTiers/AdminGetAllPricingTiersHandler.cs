using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers;

/// <summary>
/// Handles the <see cref="AdminGetAllPricingTiersQuery" /> to retrieve all pricing tiers.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminGetAllPricingTiersHandler(ILookupRepository lookupRepository, IMapper mapper)
    : IQueryHandler<AdminGetAllPricingTiersQuery, AdminGetAllPricingTiersResult>
{
    /// <inheritdoc />
    public async Task<AdminGetAllPricingTiersResult> Handle(
        AdminGetAllPricingTiersQuery query,
        CancellationToken cancellationToken
    )
    {
        IReadOnlyList<PricingTierEntity> pricingTiers = await lookupRepository.GetAllPricingTiersAsync(
            search: query.Search,
            cancellationToken: cancellationToken
        );

        IReadOnlyList<PricingTierDto> dtoList = pricingTiers.ToPricingTierDtos(mapper);
        return new AdminGetAllPricingTiersResult(PricingTiers: dtoList);
    }
}
