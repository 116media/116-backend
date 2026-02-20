using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Command to deactivate a promotion level, preventing it from being assigned to content.
/// </summary>
/// <param name="Id">The unique identifier of the promotion level to deactivate.</param>
public record DeactivatePromotionLevelCommand(Guid Id) : ICommand;
