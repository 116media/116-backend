using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Queries.GetAllPricingTiers;

/// <summary>
/// Query for retrieving all pricing tiers.
/// </summary>
public record GetAllPricingTiersQuery : IQuery<GetAllPricingTiersResult>;

/// <summary>
/// Result of the <see cref="GetAllPricingTiersQuery" /> containing all pricing tiers.
/// </summary>
/// <param name="PricingTiers">The list of all pricing tiers.</param>
public record GetAllPricingTiersResult(IReadOnlyList<PricingTierDto> PricingTiers);
