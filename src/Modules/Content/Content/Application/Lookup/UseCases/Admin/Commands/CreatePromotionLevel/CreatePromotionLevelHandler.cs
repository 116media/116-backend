using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreatePromotionLevel;

/// <summary>
/// Handles the <see cref="CreatePromotionLevelCommand" /> to create a new promotion level.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class CreatePromotionLevelHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<CreatePromotionLevelCommand, CreatePromotionLevelResult>
{
    /// <inheritdoc />
    public async Task<CreatePromotionLevelResult> Handle(
        CreatePromotionLevelCommand command,
        CancellationToken cancellationToken
    )
    {
        bool exists = await lookupRepository.PromotionLevelExistsByNameAsync(
            name: command.Name,
            cancellationToken: cancellationToken
        );

        if (exists)
        {
            throw PromotionLevelErrors.AlreadyExists(name: command.Name);
        }

        var promotionLevel = PromotionLevelEntity.Create(
            id: Guid.NewGuid(),
            name: command.Name,
            durationDays: command.DurationDays,
            priceUsd: command.PriceUsd
        );

        await lookupRepository.AddPromotionLevelAsync(
            promotionLevel: promotionLevel,
            cancellationToken: cancellationToken
        );
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = promotionLevel.ToPromotionLevelDto(mapper);
        return new CreatePromotionLevelResult(PromotionLevel: dto);
    }
}
