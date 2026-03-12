using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;

/// <summary>
/// Command to update an existing promotion level's name, duration, and price.
/// </summary>
/// <param name="Id">The unique identifier of the promotion level to update.</param>
/// <param name="Name">The new display name for the promotion level.</param>
/// <param name="DurationDays">The new promotion duration in days.</param>
/// <param name="PriceUsd">The new price in US dollars.</param>
public record AdminUpdatePromotionLevelCommand(string Id, string Name, int DurationDays, decimal PriceUsd)
    : ICommand<AdminUpdatePromotionLevelResult>;

/// <summary>
/// Result returned after successfully updating a promotion level.
/// </summary>
/// <param name="PromotionLevel">The updated promotion level data.</param>
public record AdminUpdatePromotionLevelResult(PromotionLevelDto PromotionLevel);
