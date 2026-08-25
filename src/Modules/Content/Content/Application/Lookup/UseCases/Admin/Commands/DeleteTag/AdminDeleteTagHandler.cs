using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Lookup.UseCases.Admin.Commands.DeleteTag;

/// <summary>
/// Handles the <see cref="AdminDeleteTagCommand" /> to permanently delete a content tag.
/// </summary>
/// <param name="tagRepository">Repository for tag data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminDeleteTagHandler(ITagRepository tagRepository, IContentUnitOfWork unitOfWork)
    : ICommandHandler<AdminDeleteTagCommand, AdminDeleteTagResult>
{
    /// <inheritdoc />
    public async Task<AdminDeleteTagResult> Handle(AdminDeleteTagCommand command, CancellationToken cancellationToken)
    {
        Guid id = Guid.Parse(command.Id);

        TagEntity tag = await tagRepository.GetByIdOrThrowAsync(id: id, cancellationToken: cancellationToken);

        tag.MarkDeleted();
        tagRepository.Remove(entity: tag);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDeleteTagResult(IsSuccess: true);
    }
}
