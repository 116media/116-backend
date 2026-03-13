using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateVideo;

/// <summary>
/// Command for updating a video's editable metadata fields.
/// Only permitted when the video is in <c>Draft</c> or <c>Rejected</c> status.
/// </summary>
/// <param name="Id">The unique identifier of the video to update.</param>
/// <param name="Title">The video display title.</param>
/// <param name="Slug">The URL-safe slug for this video.</param>
/// <param name="Description">Optional description shown below the video player.</param>
public record UpdateVideoCommand(string Id, string Title, string Slug, string? Description)
    : ICommand<UpdateVideoResult>;

/// <summary>
/// Result of the <see cref="UpdateVideoCommand" /> containing the updated video details.
/// </summary>
/// <param name="Video">The updated video detail information.</param>
public record UpdateVideoResult(VideoDetailDto Video);
