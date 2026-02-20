using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Handles the <see cref="DeactivatePromotionLevelCommand" /> to deactivate a promotion level.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class DeactivatePromotionLevelHandler(ILookupRepository lookupRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<DeactivatePromotionLevelCommand>
{
    /// <inheritdoc />
    public async Task Handle(DeactivatePromotionLevelCommand command, CancellationToken cancellationToken)
    {
        PromotionLevelEntity promotionLevel = await lookupRepository.GetPromotionLevelByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        bool deactivated = promotionLevel.Deactivate();

        if (!deactivated)
        {
            throw PromotionLevelErrors.AlreadyInactive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
