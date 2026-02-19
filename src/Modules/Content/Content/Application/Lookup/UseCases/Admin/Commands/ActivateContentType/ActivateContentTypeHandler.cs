using _116.Content.Application.Shared.Errors;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.ActivateContentType;

/// <summary>
/// Handles the <see cref="ActivateContentTypeCommand" /> to activate a content type.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class ActivateContentTypeHandler(ILookupRepository lookupRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<ActivateContentTypeCommand>
{
    /// <inheritdoc />
    public async Task Handle(ActivateContentTypeCommand command, CancellationToken cancellationToken)
    {
        ContentTypeEntity contentType = await lookupRepository.GetContentTypeByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        bool activated = contentType.Activate();

        if (!activated)
        {
            throw ContentTypeErrors.AlreadyActive();
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);
    }
}
