using _116.Content.Application.Shared.Errors.Facade;
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
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminDecideTranslationRevisionHandler(
    ITranslationRevisionRepository revisionRepository,
    ITranslationRepository translationRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
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

        bool decided = command.Accept
            ? revision.Accept(decidedByUserId: command.DecidedByUserId)
            : revision.Reject(decidedByUserId: command.DecidedByUserId);

        if (!decided)
        {
            throw i18n.Translation.AlreadyDecided();
        }

        if (command.Accept)
        {
            LyricsTranslationEntity translation = await translationRepository.GetByIdOrThrowAsync(
                id: revision.TranslationId,
                cancellationToken: cancellationToken
            );
            translation.ApplyAcceptedRevision(newText: revision.ProposedText);
        }

        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminDecideTranslationRevisionResult(IsSuccess: true);
    }
}
