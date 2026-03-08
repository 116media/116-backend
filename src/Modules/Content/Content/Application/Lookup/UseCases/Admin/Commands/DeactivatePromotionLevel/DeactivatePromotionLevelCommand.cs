using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Command to deactivate a promotion level, preventing it from being assigned to content.
/// </summary>
/// <param name="Id">The unique identifier of the promotion level to deactivate.</param>
public record DeactivatePromotionLevelCommand(Guid Id) : ICommand<DeactivatePromotionLevelResult>;

/// <summary>
/// Result returned after successfully deactivating a promotion level.
/// </summary>
/// <param name="PromotionLevel">The updated promotion level information.</param>
public record DeactivatePromotionLevelResult(PromotionLevelDto PromotionLevel);
