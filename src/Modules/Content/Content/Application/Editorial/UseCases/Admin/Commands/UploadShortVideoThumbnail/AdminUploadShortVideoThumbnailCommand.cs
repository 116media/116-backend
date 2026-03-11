using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoThumbnail;

/// <summary>
/// Command for uploading or replacing the thumbnail image of a short video.
/// </summary>
/// <param name="ShortVideoId">The unique identifier of the short video to update.</param>
/// <param name="File">The thumbnail image file to upload.</param>
public record AdminUploadShortVideoThumbnailCommand(string ShortVideoId, IFormFile File)
    : ICommand<AdminUploadShortVideoThumbnailResult>;

/// <summary>
/// Result of the <see cref="AdminUploadShortVideoThumbnailCommand" /> containing the new thumbnail details.
/// </summary>
/// <param name="ThumbnailUrl">The publicly accessible URL of the uploaded thumbnail.</param>
/// <param name="ThumbnailStorageKey">The provider-agnostic storage key for the uploaded thumbnail.</param>
public record AdminUploadShortVideoThumbnailResult(string ThumbnailUrl, string ThumbnailStorageKey);
