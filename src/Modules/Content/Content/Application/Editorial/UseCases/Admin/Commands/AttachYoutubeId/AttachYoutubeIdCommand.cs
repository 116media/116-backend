using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.AttachYoutubeId;

/// <summary>
/// Command for attaching a YouTube video ID to a video and auto-downloading its thumbnail.
/// </summary>
/// <param name="VideoId">The unique identifier of the video to attach the YouTube ID to.</param>
/// <param name="YoutubeVideoId">The YouTube video ID (e.g., "dQw4w9WgXcQ").</param>
public record AttachYoutubeIdCommand(string VideoId, string YoutubeVideoId) : ICommand<AttachYoutubeIdResult>;

/// <summary>
/// Result of the <see cref="AttachYoutubeIdCommand" /> containing the updated video details.
/// </summary>
/// <param name="Video">The updated video detail information.</param>
public record AttachYoutubeIdResult(VideoDetailDto Video);
