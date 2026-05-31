using _116.Content.Application.Shared.Cache;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag;

/// <summary>
/// Handles the <see cref="AdminDeleteTagCommand" /> to permanently delete a content tag.
/// </summary>
/// <param name="lookupRepository">Repository for lookup data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="cacheInvalidator">Invalidates the popular-tags cache after the tag graph changes.</param>
public class AdminDeleteTagHandler(
    ILookupRepository lookupRepository,
    IContentUnitOfWork unitOfWork,
    IPopularTagsCacheInvalidator cacheInvalidator
) : ICommandHandler<AdminDeleteTagCommand, AdminDeleteTagResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteTagResult> Handle(AdminDeleteTagCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        TagEntity tag = await lookupRepository.GetTagByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        lookupRepository.Remove(entity: tag);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        cacheInvalidator.Invalidate();

        return new AdminDeleteTagResult(IsSuccess: true);
    }
}
