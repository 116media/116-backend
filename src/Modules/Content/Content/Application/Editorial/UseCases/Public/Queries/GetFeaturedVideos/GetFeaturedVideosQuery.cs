using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedVideos;

/// <summary>
/// Query for retrieving the list of currently featured published videos.
/// </summary>
public record GetFeaturedVideosQuery() : IQuery<GetFeaturedVideosResult>;

/// <summary>
/// Result of the <see cref="GetFeaturedVideosQuery" /> containing featured video summaries.
/// </summary>
/// <param name="Videos">The list of featured video summary DTOs.</param>
public record GetFeaturedVideosResult(IReadOnlyList<VideoSummaryDto> Videos);
