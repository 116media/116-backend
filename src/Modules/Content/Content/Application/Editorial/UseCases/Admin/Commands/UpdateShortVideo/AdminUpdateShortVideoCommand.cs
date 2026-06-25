using _116.Content.Application.Shared.DTOs;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UpdateShortVideo;

/// <summary>
/// Command for updating a short video's metadata. The video file is replaced separately via the
/// dedicated upload endpoint. Slug is immutable after creation to preserve public URLs.
/// </summary>
/// <param name="Id">The unique identifier of the short video to update.</param>
/// <param name="Title">The new display title.</param>
/// <param name="VideoId">Optional parent full video ID. <c>null</c> for standalone.</param>
public record AdminUpdateShortVideoCommand(string Id, string Title, Guid? VideoId)
    : ICommand<AdminUpdateShortVideoResult>;

/// <summary>
/// Result of the <see cref="AdminUpdateShortVideoCommand" /> containing the updated short video.
/// </summary>
/// <param name="ShortVideo">The updated short video information.</param>
public record AdminUpdateShortVideoResult(ShortVideoDto ShortVideo);
