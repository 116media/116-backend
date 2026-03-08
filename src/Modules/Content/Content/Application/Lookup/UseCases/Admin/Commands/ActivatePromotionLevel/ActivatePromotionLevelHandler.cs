using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Handles the <see cref="ActivatePromotionLevelCommand" /> to activate a promotion level.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class ActivatePromotionLevelHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<ActivatePromotionLevelCommand, ActivatePromotionLevelResult>
{
    /// <inheritdoc />
    public async Task<ActivatePromotionLevelResult> Handle(
        ActivatePromotionLevelCommand command,
        CancellationToken cancellationToken
    )
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

        var dto = promotionLevel.ToPromotionLevelDto(mapper);
        return new ActivatePromotionLevelResult(PromotionLevel: dto);
    }
}
