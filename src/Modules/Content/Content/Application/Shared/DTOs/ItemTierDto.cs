namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a pricing tier snapshot attached to an order item.
/// The price is frozen at item creation time and is immutable.
/// </summary>
/// <param name="TierName">The display name of the pricing tier (e.g., "base_upload", "social_boost").</param>
/// <param name="PriceSnapshotUsd">The price in USD frozen from category pricing at the time the tier was added.</param>
public record ItemTierDto(string TierName, decimal PriceSnapshotUsd);
