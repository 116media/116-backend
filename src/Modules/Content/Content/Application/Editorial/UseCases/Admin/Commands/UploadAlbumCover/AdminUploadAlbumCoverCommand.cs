using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadAlbumCover;

/// <summary>
/// Command for uploading or replacing an album's cover art image.
/// If the album already has a cover image, the old Cloudinary asset is deleted after
/// the new one is uploaded successfully.
/// </summary>
/// <param name="AlbumId">The unique identifier of the album to upload the cover for.</param>
/// <param name="File">The cover image file to upload.</param>
public record AdminUploadAlbumCoverCommand(Guid AlbumId, IFormFile File) : ICommand<AdminUploadAlbumCoverResult>;

/// <summary>
/// Result of the <see cref="AdminUploadAlbumCoverCommand" /> containing the uploaded cover details.
/// </summary>
/// <param name="CoverImageUrl">The publicly accessible URL of the uploaded cover image.</param>
/// <param name="CoverImageStorageKey">The provider-agnostic storage key for the cover asset.</param>
public record AdminUploadAlbumCoverResult(string CoverImageUrl, string CoverImageStorageKey);
