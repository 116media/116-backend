using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePricingTier;

/// <summary>
/// Command to activate a pricing tier, making it available for use.
/// </summary>
/// <param name="Id">The unique identifier of the pricing tier to activate.</param>
public record ActivatePricingTierCommand(Guid Id) : ICommand;
