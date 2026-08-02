using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Domain.Entities;
using _116.Shared.Contracts.Application.CQRS;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar;

/// <summary>
/// Handles the <see cref="AdminUploadArtistAvatarCommand" /> to upload or replace an artist
/// profile's avatar image. The avatar file is tracked via <see cref="FileEntity" /> in
/// the Core module.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="fileRepository">Repository for centralized file entity management.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadArtistAvatarHandler(
    IArtistRepository artistRepository,
    IFileRepository fileRepository,
    IContentUnitOfWork unitOfWork
) : ICommandHandler<AdminUploadArtistAvatarCommand, AdminUploadArtistAvatarResult>
{
    /// <inheritdoc />
    public async Task<AdminUploadArtistAvatarResult> Handle(
        AdminUploadArtistAvatarCommand command,
        CancellationToken cancellationToken
    )
    {
        ArtistEntity artist = await artistRepository.GetByIdOrThrowAsync(
            id: command.ArtistId,
            cancellationToken: cancellationToken
        );

        FileEntity fileEntity = await fileRepository.ReplaceImageFileAsync(
            currentFileId: artist.AvatarFileId,
            file: command.File,
            publicId: command.ArtistId.ToString(),
            folder: "content/artist-avatars",
            originalFileName: command.File.FileName,
            mimeType: command.File.ContentType,
            cancellationToken: cancellationToken
        );

        artist.SetAvatarFileId(avatarFileId: fileEntity.Id);

        artistRepository.Update(artist: artist);
        await unitOfWork.CommitAsync(cancellationToken: cancellationToken);

        return new AdminUploadArtistAvatarResult(
            AvatarUrl: fileEntity.StorageUrl,
            AvatarStorageKey: fileEntity.StorageKey!
        );
    }
}
