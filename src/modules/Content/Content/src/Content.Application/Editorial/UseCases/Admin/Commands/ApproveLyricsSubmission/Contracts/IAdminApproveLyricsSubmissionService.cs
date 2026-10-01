using _116.Content.Domain.Entities;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.Contracts;

/// <summary>
/// The pending submission and the lyrics page built from it, ready to be persisted together.
/// </summary>
/// <param name="Submission">The pending submission.</param>
/// <param name="Lyrics">The lyrics page built from it, not yet staged.</param>
public record ApprovedSubmissionData(LyricsSubmissionEntity Submission, LyricsEntity Lyrics);

/// <summary>
/// Resolves an approval: loads the submission, gates its status and the slug, resolves the default
/// free category and builds the lyrics page.
/// </summary>
public interface IAdminApproveLyricsSubmissionService
{
    /// <summary>
    /// Prepares the approval, throwing the localized error when the submission is not pending, the
    /// slug is taken or no default category is configured. The caller persists and commits.
    /// </summary>
    /// <param name="submissionId">The submission being approved.</param>
    /// <param name="slug">The slug the page publishes under.</param>
    /// <param name="reviewerId">The approving reviewer.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<ApprovedSubmissionData> PrepareAsync(
        Guid submissionId,
        string slug,
        Guid reviewerId,
        CancellationToken cancellationToken
    );
}
