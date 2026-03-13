using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoBySlug;

/// <summary>
/// Query for retrieving the full details of a published video by its URL slug.
/// </summary>
/// <param name="Slug">The URL-safe slug of the video to retrieve.</param>
public record GetVideoBySlugQuery(string Slug) : IQuery<GetVideoBySlugResult>;

/// <summary>
/// Result of the <see cref="GetVideoBySlugQuery" /> containing the full video details.
/// </summary>
/// <param name="Video">The detailed video information.</param>
public record GetVideoBySlugResult(VideoDetailDto Video);
