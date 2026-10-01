using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.Contracts;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;

/// <summary>
/// Handles the <see cref="AdminApproveLyricsSubmissionCommand" /> to publish a community
/// submission as a lyrics page and mark it approved in one transaction.
/// </summary>
/// <param name="approveService">Service resolving the submission and building the page.</param>
/// <param name="lyricsRepository">Repository staging the new page.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminApproveLyricsSubmissionHandler(
    IAdminApproveLyricsSubmissionService approveService,
    ILyricsRepository lyricsRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminApproveLyricsSubmissionCommand, AdminApproveLyricsSubmissionResult>
{
    /// <inheritdoc />
    public async Task<AdminApproveLyricsSubmissionResult> Handle(
        AdminApproveLyricsSubmissionCommand command,
        CancellationToken cancellationToken
    )
    {
        ApprovedSubmissionData approval = await approveService.PrepareAsync(
            submissionId: command.Id,
            slug: command.Slug,
            reviewerId: command.ReviewerId,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await lyricsRepository.AddAsync(lyrics: approval.Lyrics, cancellationToken: ct);
                approval.Submission.Approve(
                    reviewedByUserId: command.ReviewerId,
                    publishedLyricsId: approval.Lyrics.Id
                );
            },
            cancellationToken: cancellationToken
        );

        return new AdminApproveLyricsSubmissionResult(IsSuccess: true, LyricsId: approval.Lyrics.Id);
    }
}
