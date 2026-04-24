using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;

/// <summary>
/// Command for updating a short video's metadata and optionally replacing the video file.
/// </summary>
/// <param name="Id">The unique identifier of the short video to update.</param>
/// <param name="Title">The new display title.</param>
/// <param name="Slug">The new URL-safe slug.</param>
/// <param name="VideoId">Optional parent full video ID. <c>null</c> for standalone.</param>
/// <param name="VideoFile">Optional new video file. When provided, overwrites the existing file in storage.</param>
public record AdminUpdateShortVideoCommand(string Id, string Title, string Slug, Guid? VideoId, IFormFile? VideoFile)
    : ICommand<AdminUpdateShortVideoResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateShortVideoCommand" /> containing the updated short video.
/// </summary>
/// <param name="ShortVideo">The updated short video information.</param>
public record AdminUpdateShortVideoResult(ShortVideoDto ShortVideo);
