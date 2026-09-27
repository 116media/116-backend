using _116.BuildingBlocks.Application.CQRS;
using _116.Content.Application.Shared.DTOs;

namespace _116.Content.Application.Editorial.UseCases.Admin.Queries.GetActiveVideos;

/// <summary>
/// Query for retrieving all active videos (excludes Archived and Rejected).
/// </summary>
/// <remarks>
/// Returns an unpaginated list for use in dropdowns and selection fields.
/// </remarks>
public record AdminGetActiveVideosQuery() : IQuery<AdminGetActiveVideosResult>;

/// <summary>
/// Result of the <see cref="AdminGetActiveVideosQuery" /> containing the list of active video summaries.
/// </summary>
/// <param name="Videos">
/// The list of active video summary DTOs.
/// </param>
public record AdminGetActiveVideosResult(IReadOnlyList<VideoSummaryDto> Videos);
