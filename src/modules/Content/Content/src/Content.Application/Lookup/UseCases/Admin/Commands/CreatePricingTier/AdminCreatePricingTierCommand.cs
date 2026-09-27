using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePricingTier;

/// <summary>
/// Command for creating a new pricing tier.
/// </summary>
/// <param name="Name">The name of the pricing tier (e.g., "base_upload", "social_boost").</param>
/// <param name="Description">A description of what this tier covers.</param>
public record AdminCreatePricingTierCommand(string Name, string Description) : ICommand<AdminCreatePricingTierResult>;

/// <summary>
/// Result of the <see cref="AdminCreatePricingTierCommand" /> containing the created pricing tier details.
/// </summary>
/// <param name="PricingTier">The created pricing tier information.</param>
public record AdminCreatePricingTierResult(PricingTierDto PricingTier);
