using _116.Content.Application.Shared.Persistence;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Shared.Contracts.Application.CQRS;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.UploadArtistAvatar;

/// <summary>
/// Handles the <see cref="AdminUploadArtistAvatarCommand" /> to upload or replace an artist
/// profile's avatar image. The avatar file is tracked via <see cref="FileReferenceDto" /> in
/// the Core module.
/// </summary>
/// <param name="artistRepository">Repository for artist profile data access operations.</param>
/// <param name="unitOfWork">Unit of Work for managing database transactions.</param>
public class AdminUploadArtistAvatarHandler(
    IArtistRepository artistRepository,
    IFileStorageService fileStorage,
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

        IFormFile file = command.File!;

        StoredFile uploaded = await fileStorage.UploadAsync(
            file: file,
            publicId: command.ArtistId.ToString(),
            folder: "content/artist-avatars",
            kind: EnumStoredFileKind.Image,
            cancellationToken: cancellationToken
        );

        await unitOfWork.ExecuteInTransactionAsync(
            async ct =>
            {
                await fileStorage.RecordAsync(
                    file: uploaded,
                    supersededFileId: artist.AvatarFileId,
                    cancellationToken: ct
                );

                artist.SetAvatarFileId(avatarFileId: uploaded.Reference.Id);
            },
            cancellationToken: cancellationToken
        );

        return new AdminUploadArtistAvatarResult(
            AvatarUrl: uploaded.Reference.StorageUrl,
            AvatarStorageKey: uploaded.Reference.StorageKey!
        );
    }
}
