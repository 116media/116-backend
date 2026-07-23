using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetLyricsSubmissions;

/// <summary>
/// Handles the <see cref="AdminGetLyricsSubmissionsQuery" /> to retrieve a paginated view of
/// the community lyrics submission moderation queue.
/// </summary>
/// <param name="submissionRepository">Repository for community lyrics submission data access operations.</param>
public class AdminGetLyricsSubmissionsHandler(ILyricsSubmissionRepository submissionRepository)
    : IQueryHandler<AdminGetLyricsSubmissionsQuery, AdminGetLyricsSubmissionsResult>
{
    /// <inheritdoc />
    public async Task<AdminGetLyricsSubmissionsResult> Handle(
        AdminGetLyricsSubmissionsQuery query,
        CancellationToken cancellationToken
    )
    {
        int pageSize = query.PaginatedRequest.PageSize;
        int pageIndex = query.PaginatedRequest.PageIndex;

        (List<LyricsSubmissionEntity> submissions, int totalCount) = await submissionRepository.GetAllAsync(
            page: pageIndex + 1,
            pageSize: pageSize,
            status: query.Status,
            cancellationToken: cancellationToken
        );

        List<LyricsSubmissionDto> dtos = submissions
            .Select(submission => new LyricsSubmissionDto(
                Id: submission.Id,
                SongTitle: submission.SongTitle,
                ArtistName: submission.ArtistName,
                LyricsText: submission.LyricsText,
                Language: submission.Language,
                SubmittedByUserId: submission.SubmittedByUserId,
                Status: submission.Status.ToString(),
                ReviewedByUserId: submission.ReviewedByUserId,
                ReviewNote: submission.ReviewNote,
                PublishedLyricsId: submission.PublishedLyricsId
            ))
            .ToList();

        var paginatedResult = new PaginatedResult<LyricsSubmissionDto>(
            pageIndex: pageIndex,
            pageSize: pageSize,
            count: totalCount,
            items: dtos
        );

        return new AdminGetLyricsSubmissionsResult(Submissions: paginatedResult);
    }
}
