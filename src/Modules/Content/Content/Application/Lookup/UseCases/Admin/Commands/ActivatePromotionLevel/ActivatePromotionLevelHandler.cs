using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Handles the <see cref="ActivatePromotionLevelCommand" /> to activate a promotion level.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class ActivatePromotionLevelHandler(ILookupRepository lookupRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<ActivatePromotionLevelCommand>
{
    /// <inheritdoc />
    public async Task Handle(ActivatePromotionLevelCommand command, CancellationToken cancellationToken)
    {
        PromotionLevelEntity promotionLevel = await lookupRepository.GetPromotionLevelByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        bool activated = promotionLevel.Activate();

        if (!activated)
        {
            throw PromotionLevelErrors.AlreadyActive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
