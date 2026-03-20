namespace _116.Content.Application.Shared.DTOs;

/// <summary>
/// Data transfer object for a user playlist summary (list view).
/// </summary>
/// <param name="Id">The unique identifier of the playlist.</param>
/// <param name="Name">The display name of the playlist.</param>
/// <param name="VideoCount">The number of videos in the playlist.</param>
public record PlaylistDto(Guid Id, string Name, int VideoCount);
