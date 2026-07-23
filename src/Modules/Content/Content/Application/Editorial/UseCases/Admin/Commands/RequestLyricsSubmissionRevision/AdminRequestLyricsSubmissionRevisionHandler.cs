using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.RequestLyricsSubmissionRevision;

/// <summary>
/// Handles the <see cref="AdminRequestLyricsSubmissionRevisionCommand" /> to ask a submitter to
/// revise and resubmit their pending community lyrics submission.
/// </summary>
/// <param name="submissionRepository">Repository for community lyrics submission data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminRequestLyricsSubmissionRevisionHandler(
    ILyricsSubmissionRepository submissionRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminRequestLyricsSubmissionRevisionCommand, AdminRequestLyricsSubmissionRevisionResult>
{
    /// <inheritdoc />
    public async Task<AdminRequestLyricsSubmissionRevisionResult> Handle(
        AdminRequestLyricsSubmissionRevisionCommand command,
        CancellationToken cancellationToken
    )
    {
        LyricsSubmissionEntity submission = await submissionRepository.GetByIdOrThrowAsync(
            id: command.Id,
            cancellationToken: cancellationToken
        );

        if (submission.Status != EnumSubmissionStatus.Pending)
        {
            throw i18n.Submission.NotPending();
        }

        submission.RequestRevision(reviewedByUserId: command.ReviewerId, note: command.Note);
        submissionRepository.Update(submission: submission);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminRequestLyricsSubmissionRevisionResult(IsSuccess: true);
    }
}
