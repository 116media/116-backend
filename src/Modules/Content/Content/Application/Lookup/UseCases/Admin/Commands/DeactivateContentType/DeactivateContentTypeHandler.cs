using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeactivateContentType;

/// <summary>
/// Handles the <see cref="DeactivateContentTypeCommand" /> to deactivate a content type.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class DeactivateContentTypeHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<DeactivateContentTypeCommand, DeactivateContentTypeResult>
{
    /// <inheritdoc />
    public async Task<DeactivateContentTypeResult> Handle(
        DeactivateContentTypeCommand command,
        CancellationToken cancellationToken
    )
    {
        ContentTypeEntity contentType = await lookupRepository.GetContentTypeByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        bool deactivated = contentType.Deactivate();

        if (!deactivated)
        {
            throw ContentTypeErrors.AlreadyInactive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = contentType.ToContentTypeDto(mapper);
        return new DeactivateContentTypeResult(ContentType: dto);
    }
}
