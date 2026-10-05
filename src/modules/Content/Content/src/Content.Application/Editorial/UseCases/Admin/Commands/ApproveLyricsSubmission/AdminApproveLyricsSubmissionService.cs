using _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ApproveLyricsSubmission;

/// <summary>
/// Resolves the approval for the admin approve-submission use case.
/// </summary>
/// <param name="submissionRepository">Repository loading the submission.</param>
/// <param name="lyricsRepository">Repository checking the slug.</param>
/// <param name="categoryRepository">Repository resolving the default lyrics category.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminApproveLyricsSubmissionService(
    ILyricsSubmissionRepository submissionRepository,
    ILyricsRepository lyricsRepository,
    ICategoryRepository categoryRepository,
    ContentI18n i18n
) : IAdminApproveLyricsSubmissionService
{
    /// <inheritdoc />
    public async Task<ApprovedSubmissionData> PrepareAsync(
        Guid submissionId,
        string slug,
        Guid reviewerId,
        CancellationToken cancellationToken
    )
    {
        LyricsSubmissionEntity submission = await submissionRepository.GetByIdOrThrowAsync(
            id: submissionId,
            cancellationToken: cancellationToken
        );

        if (submission.Status != EnumSubmissionStatus.Pending)
        {
            throw i18n.Submission.NotPending();
        }

        LyricsEntity? existing = await lyricsRepository.GetBySlugAsync(
            slug: slug,
            cancellationToken: cancellationToken
        );

        if (existing is not null)
        {
            throw i18n.Lyrics.SlugAlreadyExists(slug: slug);
        }

        // Community submissions never carry a customer or order, so approval always files under the default free category.
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
            slug: slug,
            authorId: reviewerId
        );

        return new ApprovedSubmissionData(Submission: submission, Lyrics: lyrics);
    }
}
