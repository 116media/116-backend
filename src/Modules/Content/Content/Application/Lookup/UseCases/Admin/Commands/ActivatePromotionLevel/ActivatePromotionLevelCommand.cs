using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Command to activate a promotion level, making it available for use.
/// </summary>
/// <param name="Id">The unique identifier of the promotion level to activate.</param>
public record ActivatePromotionLevelCommand(string Id) : ICommand<ActivatePromotionLevelResult>;

/// <summary>
/// Result returned after successfully activating a promotion level.
/// </summary>
/// <param name="PromotionLevel">The updated promotion level information.</param>
public record ActivatePromotionLevelResult(PromotionLevelDto PromotionLevel);
