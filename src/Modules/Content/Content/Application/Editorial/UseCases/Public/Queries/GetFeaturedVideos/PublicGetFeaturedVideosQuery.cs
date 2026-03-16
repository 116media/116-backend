using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetFeaturedVideos;

/// <summary>
/// Query for retrieving the list of currently featured published videos.
/// </summary>
public record PublicGetFeaturedVideosQuery() : IQuery<PublicGetFeaturedVideosResult>;

/// <summary>
/// Result of the <see cref="PublicGetFeaturedVideosQuery" /> containing featured video summaries.
/// </summary>
/// <param name="Videos">The list of featured video summary DTOs.</param>
public record PublicGetFeaturedVideosResult(IReadOnlyList<VideoSummaryDto> Videos);
