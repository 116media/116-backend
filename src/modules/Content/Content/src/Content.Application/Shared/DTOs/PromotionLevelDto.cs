namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object representing a promotion level.
/// </summary>
/// <param name="Id">The unique identifier of the promotion level.</param>
/// <param name="Name">The display name of the promotion level.</param>
/// <param name="DurationDays">The homepage placement duration in days.</param>
/// <param name="PriceUsd">The price of this promotion level in USD.</param>
/// <param name="IsActive">Whether the promotion level is currently active.</param>
/// <param name="SpotPriority">The homepage grid spot this promotion level maps to (1, 2, or 3). Null means no specific spot.</param>
public record PromotionLevelDto(
    Guid Id,
    string Name,
    int DurationDays,
    decimal PriceUsd,
    bool IsActive,
    int? SpotPriority
);
