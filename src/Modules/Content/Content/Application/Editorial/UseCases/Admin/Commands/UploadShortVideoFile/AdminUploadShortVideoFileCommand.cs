using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadShortVideoFile;

/// <summary>
/// Command for uploading or replacing the video file of a short video.
/// </summary>
/// <param name="ShortVideoId">The unique identifier of the short video to update.</param>
/// <param name="File">The video file to upload. Null when the file part is missing.</param>
public record AdminUploadShortVideoFileCommand(string ShortVideoId, IFormFile? File)
    : ICommand<AdminUploadShortVideoFileResult>;

/// <summary>
/// Result of the <see cref="AdminUploadShortVideoFileCommand" /> containing the new video file details.
/// </summary>
/// <param name="VideoUrl">The publicly accessible URL of the uploaded video file.</param>
/// <param name="VideoStorageKey">The provider-agnostic storage key for the uploaded video file.</param>
public record AdminUploadShortVideoFileResult(string VideoUrl, string VideoStorageKey);
