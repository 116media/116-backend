using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar;

/// <summary>
/// Command for uploading or replacing an artist profile's avatar image.
/// If the artist profile already has an avatar, the old Cloudinary asset is deleted after
/// the new one is uploaded successfully.
/// </summary>
/// <param name="ArtistId">The unique identifier of the artist profile to upload the avatar for.</param>
/// <param name="File">The avatar image file to upload. Null when the file part is missing.</param>
public record AdminUploadArtistAvatarCommand(Guid ArtistId, IFormFile? File) : ICommand<AdminUploadArtistAvatarResult>;

/// <summary>
/// Result of the <see cref="AdminUploadArtistAvatarCommand" /> containing the uploaded avatar details.
/// </summary>
/// <param name="AvatarUrl">The publicly accessible URL of the uploaded avatar image.</param>
/// <param name="AvatarStorageKey">The provider-agnostic storage key for the avatar asset.</param>
public record AdminUploadArtistAvatarResult(string AvatarUrl, string AvatarStorageKey);
