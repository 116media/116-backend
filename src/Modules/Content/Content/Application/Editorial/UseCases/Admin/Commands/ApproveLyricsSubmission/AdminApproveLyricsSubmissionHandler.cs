using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;

/// <summary>
/// Handles the <see cref="AdminApproveLyricsSubmissionCommand" /> to approve a pending
/// community lyrics submission. Runs as two separate, individually-safe commits: creating the
/// lyrics record first, then marking the submission approved and linked to it — the lyrics
/// record's own commit succeeding on its own is a safe, retryable state, since a reconciliation
/// query can detect and repair a submission left behind in <c>Pending</c> if the second commit
/// never runs.
/// </summary>
/// <param name="submissionRepository">Repository for community lyrics submission data access operations.</param>
/// <param name="lyricsRepository">Repository for lyrics data access operations.</param>
/// <param name="categoryRepository">Repository for category data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminApproveLyricsSubmissionHandler(
    ILyricsSubmissionRepository submissionRepository,
    ILyricsRepository lyricsRepository,
    ICategoryRepository categoryRepository,
    IContentUnitOfWork unitOfWork,
    ContentI18n i18n
) : ICommandHandler<AdminApproveLyricsSubmissionCommand, AdminApproveLyricsSubmissionResult>
{
    /// <inheritdoc />
    public async Task<AdminApproveLyricsSubmissionResult> Handle(
        AdminApproveLyricsSubmissionCommand command,
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

        LyricsEntity? existing = await lyricsRepository.GetBySlugAsync(
            slug: command.Slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.Lyrics.SlugAlreadyExists(slug: command.Slug);
        }

        // Community submissions never carry a customer/order — the submitter never picks a
        // category — so approval always assigns the seeded default free category, via
        // LyricsEntity.CreateFree, never CreatePaid.
        CategoryEntity? category = await categoryRepository.GetDefaultLyricsCategoryAsync(
            cancellationToken: cancellationToken
        );

        if (category is null)
        {
            throw i18n.Category.DefaultLyricsCategoryNotConfigured();
        }

        LyricsEntity lyrics = LyricsEntity.CreateFree(
            id: Guid.NewGuid(),
            categoryId: category.Id,
            videoId: null,
            songTitle: submission.SongTitle,
            artistName: submission.ArtistName,
            lyricsText: submission.LyricsText,
            language: submission.Language,
            slug: command.Slug,
            authorId: command.ReviewerId
        );

        await lyricsRepository.AddAsync(lyrics: lyrics, cancellationToken: cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken); // step 1 — safe to retry alone

        submission.Approve(reviewedByUserId: command.ReviewerId, publishedLyricsId: lyrics.Id);
        submissionRepository.Update(submission: submission);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken); // step 2

        return new AdminApproveLyricsSubmissionResult(IsSuccess: true, LyricsId: lyrics.Id);
    }
}
