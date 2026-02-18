namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a pricing tier.
/// </summary>
/// <param name="Id">The unique identifier of the pricing tier.</param>
/// <param name="Name">The name of the pricing tier.</param>
/// <param name="Description">An optional description of what this tier covers.</param>
/// <param name="IsActive">Whether the pricing tier is currently active.</param>
public record PricingTierDto(Guid Id, string Name, string? Description, bool IsActive);
