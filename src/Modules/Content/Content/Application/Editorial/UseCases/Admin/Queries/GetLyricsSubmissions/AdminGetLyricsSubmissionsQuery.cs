using _116.Content.Domain.Enums;
using _116.Shared.Application.Pagination;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetLyricsSubmissions;

/// <summary>
/// Query for retrieving a paginated view of the community lyrics submission moderation queue.
/// Supports optional filtering by moderation status.
/// </summary>
/// <param name="PaginatedRequest">Pagination parameters (page index and page size).</param>
/// <param name="Status">Optional filter by moderation status.</param>
public record AdminGetLyricsSubmissionsQuery(PaginatedRequest PaginatedRequest, EnumSubmissionStatus? Status)
    : IQuery<AdminGetLyricsSubmissionsResult>;

/// <summary>
/// A single community lyrics submission in the moderation queue.
/// </summary>
/// <param name="Id">The unique identifier of the submission.</param>
/// <param name="SongTitle">The title of the submitted song.</param>
/// <param name="ArtistName">The performing artist name as entered by the submitter.</param>
/// <param name="LyricsText">The full submitted lyrics text.</param>
/// <param name="Language">ISO 639-1 language code of the submitted lyrics.</param>
/// <param name="SubmittedByUserId">The identity user UUID of the submitter.</param>
/// <param name="Status">The submission's current moderation status.</param>
/// <param name="ReviewedByUserId">The identity user UUID of the reviewing moderator, or null until reviewed.</param>
/// <param name="ReviewNote">The moderator's note explaining a rejection or revision request, or null until set.</param>
/// <param name="PublishedLyricsId">The lyrics record created from this submission once approved, or null until then.</param>
public record LyricsSubmissionDto(
    Guid Id,
    string SongTitle,
    string ArtistName,
    string LyricsText,
    string Language,
    Guid SubmittedByUserId,
    string Status,
    Guid? ReviewedByUserId,
    string? ReviewNote,
    Guid? PublishedLyricsId
);

/// <summary>
/// Result of the <see cref="AdminGetLyricsSubmissionsQuery" /> containing a paginated list of
/// submission DTOs.
/// </summary>
/// <param name="Submissions">The paginated result containing submission DTOs.</param>
public record AdminGetLyricsSubmissionsResult(PaginatedResult<LyricsSubmissionDto> Submissions);
