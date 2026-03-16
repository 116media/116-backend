using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdatePromotionLevel;

/// <summary>
/// Handles the <see cref="AdminUpdatePromotionLevelCommand" /> to update an existing promotion level.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdatePromotionLevelHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminUpdatePromotionLevelCommand, AdminUpdatePromotionLevelResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdatePromotionLevelResult> Handle(
        AdminUpdatePromotionLevelCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        PromotionLevelEntity promotionLevel = await lookupRepository.GetPromotionLevelByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool nameConflict = await lookupRepository.PromotionLevelExistsByNameAsync(
            name: command.Name,
            cancellationToken: cancellationToken
        );

        if (nameConflict && !string.Equals(promotionLevel.Name, command.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw PromotionLevelErrors.AlreadyExists(name: command.Name);
        }

        promotionLevel.Update(name: command.Name, durationDays: command.DurationDays, priceUsd: command.PriceUsd);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = promotionLevel.ToPromotionLevelDto(mapper);
        return new AdminUpdatePromotionLevelResult(PromotionLevel: dto);
    }
}
