namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a single video within a playlist.
/// </summary>
/// <param name="VideoId">The unique identifier of the video.</param>
/// <param name="Title">The display title of the video.</param>
/// <param name="ThumbnailUrl">The URL of the video thumbnail, or null if not set.</param>
/// <param name="RatingAverage">The cached average star rating of the video.</param>
/// <param name="RatingCount">The total number of ratings received by the video.</param>
/// <param name="SortOrder">The display order of this video within the playlist.</param>
public record VideoInPlaylistDto(
    Guid VideoId,
    string Title,
    string? ThumbnailUrl,
    decimal RatingAverage,
    int RatingCount,
    int SortOrder
);

/// <summary>
/// Data transfer object for a user playlist detail view, including all videos.
/// </summary>
/// <param name="Id">The unique identifier of the playlist.</param>
/// <param name="Name">The display name of the playlist.</param>
/// <param name="Videos">The ordered list of videos in the playlist.</param>
public record PlaylistDetailDto(Guid Id, string Name, IReadOnlyList<VideoInPlaylistDto> Videos);
