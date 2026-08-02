using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Commands.ProposeTranslationRevision;

/// <summary>
/// Handles the <see cref="PublicProposeTranslationRevisionCommand" /> to propose a community
/// correction to a published translation.
/// </summary>
/// <param name="translationRepository">Repository for lyrics translation data access operations.</param>
/// <param name="revisionRepository">Repository for translation revision data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class PublicProposeTranslationRevisionHandler(
    ITranslationRepository translationRepository,
    ITranslationRevisionRepository revisionRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<PublicProposeTranslationRevisionCommand, PublicProposeTranslationRevisionResult>
{
    /// <inheritdoc />
    public async Task<PublicProposeTranslationRevisionResult> Handle(
        PublicProposeTranslationRevisionCommand command,
        CancellationToken cancellationToken
    )
    {
        await translationRepository.GetByIdOrThrowAsync(
            id: command.TranslationId,
            cancellationToken: cancellationToken
        );

        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionEntity.Propose(
            id: Guid.NewGuid(),
            translationId: command.TranslationId,
            proposedText: command.ProposedText,
            editSummary: command.EditSummary,
            userId: command.UserId
        );

        await revisionRepository.AddAsync(revision: revision, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new PublicProposeTranslationRevisionResult(RevisionId: revision.Id);
    }
}
