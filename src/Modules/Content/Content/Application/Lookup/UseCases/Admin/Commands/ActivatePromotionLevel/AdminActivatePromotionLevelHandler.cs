using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivatePromotionLevel;

/// <summary>
/// Handles the <see cref="AdminActivatePromotionLevelCommand" /> to activate a promotion level.
/// </summary>
/// <param name="promotionLevelRepository">Repository for promotion level data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminActivatePromotionLevelHandler(
    IPromotionLevelRepository promotionLevelRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper,
    ContentI18n i18n
) : ICommandHandler<AdminActivatePromotionLevelCommand, AdminActivatePromotionLevelResult>
{
    /// <inheritdoc />
    public async Task<AdminActivatePromotionLevelResult> Handle(
        AdminActivatePromotionLevelCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        PromotionLevelEntity promotionLevel = await promotionLevelRepository.GetByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool activated = promotionLevel.Activate();

        if (!activated)
        {
            throw i18n.PromotionLevel.AlreadyActive();
        }

        promotionLevelRepository.Update(promotionLevel: promotionLevel);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = promotionLevel.ToPromotionLevelDto(mapper);
        return new AdminActivatePromotionLevelResult(PromotionLevel: dto);
    }
}
