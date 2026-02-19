using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;

/// <summary>
/// Command for creating a new promotion level.
/// </summary>
/// <param name="Name">The display name of the promotion level (e.g., "Featured — 7 days").</param>
/// <param name="DurationDays">The homepage placement duration in days (must be greater than zero).</param>
/// <param name="PriceUsd">The price of this promotion level in USD (must be zero or greater).</param>
public record CreatePromotionLevelCommand(string Name, int DurationDays, decimal PriceUsd)
    : ICommand<CreatePromotionLevelResult>;

/// <summary>
/// Result of the <see cref="CreatePromotionLevelCommand" /> containing the created promotion level details.
/// </summary>
/// <param name="PromotionLevel">The created promotion level information.</param>
public record CreatePromotionLevelResult(PromotionLevelDto PromotionLevel);
