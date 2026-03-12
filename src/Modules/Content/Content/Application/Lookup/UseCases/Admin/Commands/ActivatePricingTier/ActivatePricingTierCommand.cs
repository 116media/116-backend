using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;

/// <summary>
/// Command to activate a pricing tier, making it available for use.
/// </summary>
/// <param name="Id">The unique identifier of the pricing tier to activate.</param>
public record ActivatePricingTierCommand(string Id) : ICommand<ActivatePricingTierResult>;

/// <summary>
/// Result returned after successfully activating a pricing tier.
/// </summary>
/// <param name="PricingTier">The updated pricing tier information.</param>
public record ActivatePricingTierResult(PricingTierDto PricingTier);
