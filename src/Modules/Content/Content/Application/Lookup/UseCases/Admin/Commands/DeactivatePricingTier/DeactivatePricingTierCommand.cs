using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePricingTier;

/// <summary>
/// Command to deactivate a pricing tier, preventing it from being assigned to content.
/// </summary>
/// <param name="Id">The unique identifier of the pricing tier to deactivate.</param>
public record DeactivatePricingTierCommand(Guid Id) : ICommand;
