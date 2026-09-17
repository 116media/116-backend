using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Identity.Application.User.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace _116.Identity.Infrastructure.Services;

/// <summary>
/// Implements Identity's avatar workflow over Core's storage contract.
/// </summary>
/// <param name="fileStorage">Core's storage contract.</param>
/// <param name="mapper">Injected IMapper instance</param>
public class AvatarService(IFileStorageService fileStorage, IMapper mapper) : IAvatarService
{
    /// <summary>
    /// The storage folder every avatar is written to.
    /// </summary>
    private const string AvatarFolder = "avatars";

    /// <inheritdoc />
    public async Task<FileDto?> GetAvatarAsync(Guid? avatarFileId, CancellationToken cancellationToken = default)
    {
        FileReferenceDto? avatar = await fileStorage.ResolveAsync(
            fileId: avatarFileId,
            cancellationToken: cancellationToken
        );

        return avatar is null ? null : mapper.Map<FileDto>(avatar);
    }

    /// <inheritdoc />
    public Task<StoredFile> UploadAsync(
        IFormFile avatarFile,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        return fileStorage.UploadAsync(
            file: avatarFile,
            publicId: userId.ToString(),
            folder: AvatarFolder,
            kind: EnumStoredFileKind.Image,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public Task<StoredFile?> UploadFromUrlAsync(
        Guid? currentAvatarFileId,
        string avatarUrl,
        CancellationToken cancellationToken = default
    )
    {
        return fileStorage.UploadFromUrlAsync(
            currentFileId: currentAvatarFileId,
            url: avatarUrl,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public Task<FileReferenceDto> RecordAsync(
        StoredFile avatar,
        Guid? supersededFileId = null,
        CancellationToken cancellationToken = default
    )
    {
        return fileStorage.RecordAsync(
            file: avatar,
            supersededFileId: supersededFileId,
            cancellationToken: cancellationToken
        );
    }
}
