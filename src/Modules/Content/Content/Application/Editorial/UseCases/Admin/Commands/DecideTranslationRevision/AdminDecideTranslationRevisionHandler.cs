using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.DecideTranslationRevision;

/// <summary>
/// Handles the <see cref="AdminDecideTranslationRevisionCommand" /> to let a moderator accept or
/// reject a pending translation revision directly, bypassing the community vote tally.
/// </summary>
/// <param name="revisionRepository">Repository for translation revision data access operations.</param>
/// <param name="translationRepository">Repository for lyrics translation data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminDecideTranslationRevisionHandler(
    ITranslationRevisionRepository revisionRepository,
    ITranslationRepository translationRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminDecideTranslationRevisionCommand, AdminDecideTranslationRevisionResult>
{
    /// <inheritdoc />
    public async Task<AdminDecideTranslationRevisionResult> Handle(
        AdminDecideTranslationRevisionCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsTranslationRevisionEntity revision = await revisionRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        if (command.Accept)
        {
            revision.Accept(decidedByUserId: command.DecidedByUserId);
            revisionRepository.Update(revision: revision);

            LyricsTranslationEntity translation = await translationRepository.GetByIdOrThrowAsync(
                id: revision.TranslationId,
                cancellationToken: cancellationToken
            );
            translation.ApplyAcceptedRevision(newText: revision.ProposedText);
            translationRepository.Update(translation: translation);
        }
        else
        {
            revision.Reject(decidedByUserId: command.DecidedByUserId);
            revisionRepository.Update(revision: revision);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDecideTranslationRevisionResult(IsSuccess: true);
    }
}
