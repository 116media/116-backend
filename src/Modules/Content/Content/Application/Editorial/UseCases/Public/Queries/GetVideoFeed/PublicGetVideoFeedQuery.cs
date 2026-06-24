using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Public.Queries.GetVideoFeed;

/// <summary>
/// Query for retrieving the public video feed: pinned video categories, each with
/// its latest published videos.
/// </summary>
public record PublicGetVideoFeedQuery : IQuery<PublicGetVideoFeedResult>;

/// <summary>
/// A single feed section: one pinned category and its latest published videos.
/// </summary>
/// <param name="Category">The pinned category metadata.</param>
/// <param name="Videos">Up to the section limit of latest published videos in the category, newest first.</param>
public record VideoFeedSectionDto(CategoryDto Category, IReadOnlyList<VideoSummaryDto> Videos);

/// <summary>
/// Result of the <see cref="PublicGetVideoFeedQuery" />.
/// </summary>
/// <param name="Sections">
/// Ordered feed sections (most recently pinned category first). Sections whose
/// category has no published videos are omitted.
/// </param>
public record PublicGetVideoFeedResult(IReadOnlyList<VideoFeedSectionDto> Sections);
