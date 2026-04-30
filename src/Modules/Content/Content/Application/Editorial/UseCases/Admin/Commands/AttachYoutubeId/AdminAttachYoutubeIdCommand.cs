using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId;

/// <summary>
/// Command for attaching a YouTube video URL to a video and auto-downloading its thumbnail.
/// </summary>
/// <param name="VideoId">
/// The unique identifier of the video to attach the YouTube URL to.
/// </param>
/// <param name="YoutubeVideoUrl">
/// The full YouTube video URL (e.g., "https://www.youtube.com/watch?v=dQw4w9WgXcQ").
/// </param>
public record AdminAttachYoutubeIdCommand(string VideoId, string YoutubeVideoUrl)
    : ICommand<AdminAttachYoutubeIdResult>;

/// <summary>
/// Result of the <see cref="AdminAttachYoutubeIdCommand" /> containing the updated video details.
/// </summary>
/// <param name="Video">The updated video detail information.</param>
public record AdminAttachYoutubeIdResult(VideoDetailDto Video);
