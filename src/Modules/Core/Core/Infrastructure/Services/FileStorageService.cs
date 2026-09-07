using _116.Core.Application.Shared.Mappers;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Core.Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Http;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Core's implementation of the cross-module storage contract. Translates between the opaque
/// <see cref="FileReferenceDto" /> other modules hold and the file aggregate Core owns.
/// </summary>
/// <param name="fileRepository">Repository for file data access operations.</param>
/// <param name="fileUploadService">Uploads assets and stages their rows.</param>
/// <param name="cloudinaryService">Cloud storage gateway for direct asset removal.</param>
/// <param name="mapper">Injected IMapper instance</param>
public class FileStorageService(
    IFileRepository fileRepository,
    IFileUploadService fileUploadService,
    ICloudinaryService cloudinaryService,
    IMapper mapper
) : IFileStorageService
{
    /// <inheritdoc />
    public async Task<StoredFile> UploadAsync(
        IFormFile file,
        string publicId,
        string folder,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    )
    {
        FileEntity uploaded = kind switch
        {
            EnumStoredFileKind.Video => await fileUploadService.UploadVideoAsync(
                file: file,
                publicId: publicId,
                folder: folder,
                originalFileName: file.FileName,
                mimeType: NormalizeMimeType(file.ContentType),
                cancellationToken: cancellationToken
            ),
            EnumStoredFileKind.Raw => await fileUploadService.UploadRawAsync(
                file: file,
                publicId: publicId,
                folder: folder,
                originalFileName: file.FileName,
                mimeType: NormalizeMimeType(file.ContentType),
                cancellationToken: cancellationToken
            ),
            _ => await fileUploadService.UploadImageAsync(
                file: file,
                publicId: publicId,
                folder: folder,
                originalFileName: file.FileName,
                mimeType: NormalizeMimeType(file.ContentType),
                cancellationToken: cancellationToken
            ),
        };

        return Handle(uploaded);
    }

    /// <inheritdoc />
    public async Task<StoredFile?> UploadFromUrlAsync(
        Guid? currentFileId,
        string url,
        CancellationToken cancellationToken = default
    )
    {
        FileEntity? uploaded = await fileUploadService.UploadAvatarFromUrlAsync(
            currentAvatarFileId: currentFileId,
            avatarUrl: url,
            cancellationToken: cancellationToken
        );

        return uploaded is null ? null : Handle(uploaded);
    }

    /// <inheritdoc />
    public async Task<FileReferenceDto> RecordAsync(
        StoredFile file,
        Guid? supersededFileId = null,
        CancellationToken cancellationToken = default
    )
    {
        FileEntity recorded = await fileUploadService.RecordAsync(
            file: Rebuild(file.Reference),
            supersededFileId: supersededFileId,
            cancellationToken: cancellationToken
        );

        return recorded.ToFileReferenceDto(mapper);
    }

    /// <inheritdoc />
    public async Task<FileReferenceDto?> ResolveAsync(Guid? fileId, CancellationToken cancellationToken = default)
    {
        if (fileId is not Guid id)
        {
            return null;
        }

        FileEntity? file = await fileRepository.GetByIdAsync(fileId: id, cancellationToken: cancellationToken);

        return file.ToFileReferenceDtoOrNull(mapper);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, FileReferenceDto>> ResolveManyAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyDictionary<Guid, FileEntity> files = await fileRepository.GetByIdsAsync(
            fileIds: fileIds,
            cancellationToken: cancellationToken
        );

        return files.ToDictionary(entry => entry.Key, entry => entry.Value.ToFileReferenceDto(mapper));
    }

    /// <inheritdoc />
    public Task<IReadOnlyDictionary<Guid, string>> ResolveUrlsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    )
    {
        return fileRepository.GetStorageUrlsByIdsAsync(fileIds: fileIds, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default)
    {
        return fileRepository.SoftDeleteByIdAsync(fileId: fileId, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> DeleteAssetsAsync(
        IEnumerable<string> storageKeys,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    )
    {
        return cloudinaryService.DeleteManyAsync(
            publicIds: storageKeys,
            kind: kind,
            cancellationToken: cancellationToken
        );
    }

    /// <summary>
    /// Wraps an uploaded asset in the handle consuming modules hold.
    /// </summary>
    /// <param name="file">The unrecorded file aggregate.</param>
    /// <returns>The handle.</returns>
    private StoredFile Handle(FileEntity file)
    {
        return new StoredFile { Reference = file.ToFileReferenceDto(mapper) };
    }

    /// <summary>
    /// Rebuilds the aggregate a handle describes. The reference carries every field the
    /// factory takes, so nothing is lost across the contract.
    /// </summary>
    /// <param name="reference">The handle's reference.</param>
    /// <returns>The unrecorded file aggregate.</returns>
    private static FileEntity Rebuild(FileReferenceDto reference)
    {
        return FileEntity.Create(
            id: reference.Id,
            fileName: reference.FileName,
            originalFileName: reference.OriginalFileName,
            mimeType: reference.MimeType,
            storageUrl: reference.StorageUrl,
            sizeInBytes: reference.SizeInBytes,
            storageKey: reference.StorageKey,
            dominantColorHex: reference.DominantColorHex,
            foregroundColorHex: reference.ForegroundColorHex
        );
    }

    /// <summary>
    /// Drops the parameters browsers append to a content type, so the stored value is the bare
    /// media type the delete path classifies on.
    /// </summary>
    /// <param name="contentType">The submitted content type.</param>
    /// <returns>The bare media type.</returns>
    private static string NormalizeMimeType(string contentType)
    {
        return contentType.Split(';')[0].Trim().ToLowerInvariant();
    }
}
