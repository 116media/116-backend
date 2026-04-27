using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePricingTier;

/// <summary>
/// Command for updating an existing pricing tier.
/// </summary>
/// <param name="Id">The unique identifier of the pricing tier to update.</param>
/// <param name="Name">The new name for the pricing tier.</param>
/// <param name="Description">The new description for the pricing tier.</param>
public record AdminUpdatePricingTierCommand(string Id, string Name, string Description)
    : ICommand<AdminUpdatePricingTierResult>;

/// <summary>
/// Result of the <see cref="AdminUpdatePricingTierCommand" /> containing the updated pricing tier details.
/// </summary>
/// <param name="PricingTier">The updated pricing tier information.</param>
public record AdminUpdatePricingTierResult(PricingTierDto PricingTier);
