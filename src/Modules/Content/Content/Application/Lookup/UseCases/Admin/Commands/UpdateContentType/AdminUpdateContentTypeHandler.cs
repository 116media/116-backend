using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Mappers;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using MapsterMapper;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.UpdateContentType;

/// <summary>
/// Handles the <see cref="AdminUpdateContentTypeCommand" /> to update an existing content type.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="mapper">Mapster mapper for entity-to-DTO transformations.</param>
public class AdminUpdateContentTypeHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IMapper mapper
) : ICommandHandler<AdminUpdateContentTypeCommand, AdminUpdateContentTypeResult>
{
    /// <inheritdoc />
    public async Task<AdminUpdateContentTypeResult> Handle(
        AdminUpdateContentTypeCommand command,
        CancellationToken cancellationToken
    )
    {
        Guid id = Guid.Parse(command.Id);

        ContentTypeEntity contentType = await lookupRepository.GetContentTypeByIdOrThrowAsync(
            id: id,
            cancellationToken: cancellationToken
        );

        bool nameConflict = await lookupRepository.ContentTypeExistsByNameAsync(
            name: command.Name,
            cancellationToken: cancellationToken
        );

        if (nameConflict && !string.Equals(contentType.Name, command.Name, StringComparison.OrdinalIgnoreCase))
        {
            throw ContentTypeErrors.AlreadyExists(name: command.Name);
        }

        contentType.Update(name: command.Name);

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = contentType.ToContentTypeDto(mapper);
        return new AdminUpdateContentTypeResult(ContentType: dto);
    }
}
