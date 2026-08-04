using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadLyricsCover;

/// <summary>
/// Command for uploading or replacing a lyrics page's cover/album art image.
/// If the lyrics page already has a cover image, the old Cloudinary asset is deleted after
/// the new one is uploaded successfully.
/// </summary>
/// <param name="LyricsId">The unique identifier of the lyrics page to upload the cover for.</param>
/// <param name="File">The cover image file to upload. Null when the file part is missing.</param>
public record AdminUploadLyricsCoverCommand(Guid LyricsId, IFormFile? File) : ICommand<AdminUploadLyricsCoverResult>;

/// <summary>
/// Result of the <see cref="AdminUploadLyricsCoverCommand" /> containing the uploaded cover details.
/// </summary>
/// <param name="CoverImageUrl">The publicly accessible URL of the uploaded cover image.</param>
/// <param name="CoverImageStorageKey">The provider-agnostic storage key for the cover asset.</param>
public record AdminUploadLyricsCoverResult(string CoverImageUrl, string CoverImageStorageKey);
