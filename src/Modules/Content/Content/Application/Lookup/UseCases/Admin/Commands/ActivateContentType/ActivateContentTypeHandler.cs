using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;

/// <summary>
/// Handles the <see cref="ActivateContentTypeCommand" /> to activate a content type.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class ActivateContentTypeHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<ActivateContentTypeCommand, ActivateContentTypeResult>
{
    /// <inheritdoc />
    public async Task<ActivateContentTypeResult> Handle(
        ActivateContentTypeCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ContentTypeEntity contentType = await lookupRepository.GetContentTypeByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool activated = contentType.Activate();

        if (!activated)
        {
            throw ContentTypeErrors.AlreadyActive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = contentType.ToContentTypeDto(mapper);
        return new ActivateContentTypeResult(ContentType: dto);
    }
}
