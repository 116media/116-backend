using _116.Content.Application.Shared.DTOs;
using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;
using Mapster;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.CreateContentType;

/// <summary>
/// Handles the <see cref="CreateContentTypeCommand" /> to create a new content type.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class CreateContentTypeHandler(ILookupRepository lookupRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<CreateContentTypeCommand, CreateContentTypeResult>
{
    /// <inheritdoc />
    public async Task<CreateContentTypeResult> Handle(
        CreateContentTypeCommand command,
        CancellationToken cancellationToken
    )
    {
        bool exists = await lookupRepository.ContentTypeExistsByNameAsync(
            name: command.Name,
            cancellationToken: cancellationToken
        );

        if (exists)
        {
            throw ContentTypeErrors.AlreadyExists(name: command.Name);
        }

        var contentType = ContentTypeEntity.Create(id: Guid.NewGuid(), name: command.Name);

        await lookupRepository.AddContentTypeAsync(contentType: contentType, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        var dto = contentType.Adapt<ContentTypeDto>();

        return new CreateContentTypeResult(ContentType: dto);
    }
}
