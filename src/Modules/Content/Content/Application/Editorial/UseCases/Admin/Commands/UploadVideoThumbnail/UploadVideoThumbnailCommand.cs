using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadVideoThumbnail;

/// <summary>
/// Command for uploading or replacing a video thumbnail image.
/// If the video already has a thumbnail, the old Cloudinary asset is deleted after the
/// new one is uploaded successfully.
/// </summary>
/// <param name="VideoId">The unique identifier of the video to upload the thumbnail for.</param>
/// <param name="File">The thumbnail image file to upload.</param>
public record UploadVideoThumbnailCommand(string VideoId, IFormFile File) : ICommand<UploadVideoThumbnailResult>;

/// <summary>
/// Result of the <see cref="UploadVideoThumbnailCommand" /> containing the uploaded thumbnail details.
/// </summary>
/// <param name="ThumbnailUrl">The publicly accessible URL of the uploaded thumbnail.</param>
/// <param name="ThumbnailStorageKey">The provider-agnostic storage key for the thumbnail asset.</param>
public record UploadVideoThumbnailResult(string ThumbnailUrl, string ThumbnailStorageKey);
