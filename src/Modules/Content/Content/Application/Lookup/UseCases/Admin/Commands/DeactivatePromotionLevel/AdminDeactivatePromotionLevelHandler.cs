using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivatePromotionLevel;

/// <summary>
/// Handles the <see cref="AdminDeactivatePromotionLevelCommand" /> to deactivate a promotion level.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminDeactivatePromotionLevelHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminDeactivatePromotionLevelCommand, AdminDeactivatePromotionLevelResult>
{
    /// <inheritdoc />
    public async Task<AdminDeactivatePromotionLevelResult> Handle(
        AdminDeactivatePromotionLevelCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        PromotionLevelEntity promotionLevel = await lookupRepository.GetPromotionLevelByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool deactivated = promotionLevel.Deactivate();

        if (!deactivated)
        {
            throw i18n.PromotionLevel.AlreadyInactive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = promotionLevel.ToPromotionLevelDto(mapper);
        return new AdminDeactivatePromotionLevelResult(PromotionLevel: dto);
    }
}
