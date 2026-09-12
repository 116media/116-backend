using _116.Core.Application.Shared.Cache;
using _116.Core.Application.Shared.Mappers;
using _116.Core.Application.Shared.Repositories;
using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Application.Services;
using _116.Core.Contracts.Domain.Enums;
using _116.Core.Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Core.Infrastructure.Services;

/// <summary>
/// Core's implementation of the cross-module storage contract. Translates between the opaque
/// <see cref="FileReferenceDto" /> other modules hold and the file aggregate Core owns.
/// </summary>
/// <param name="fileRepository">Repository for file data access operations.</param>
/// <param name="fileUploadService">Uploads assets and stages their rows.</param>
/// <param name="cloudinaryService">Cloud storage gateway for direct asset removal.</param>
/// <param name="mapper">Injected IMapper instance</param>
/// <param name="cache">Cache holding resolved file projections.</param>
public class FileStorageService(
    IFileRepository fileRepository,
    IFileUploadService fileUploadService,
    ICloudinaryService cloudinaryService,
    IMapper mapper,
    HybridCache cache
) : IFileStorageService
{
    /// <summary>
    /// How long a resolved file projection is served from cache. A file's URL only changes when
    /// the row is replaced or deleted, and both evict the tag, so the window is a backstop.
    /// </summary>
    private static readonly HybridCacheEntryOptions CacheOptions = new()
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10),
    };

    private static readonly string[] CacheTags = [CoreCacheTags.Files];

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

        return await cache.GetOrCreateAsync(
            CacheKey(id),
            (Repository: fileRepository, Mapper: mapper, Id: id),
            static async (state, ct) =>
            {
                FileEntity? file = await state.Repository.GetByIdAsync(fileId: state.Id, cancellationToken: ct);

                return file.ToFileReferenceDtoOrNull(state.Mapper);
            },
            CacheOptions,
            CacheTags,
            cancellationToken
        );
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

        var resolved = new Dictionary<Guid, FileReferenceDto>(files.Count);

        foreach ((Guid id, FileEntity file) in files)
        {
            FileReferenceDto reference = file.ToFileReferenceDto(mapper);
            resolved[id] = reference;

            await cache.SetAsync<FileReferenceDto?>(
                CacheKey(id),
                reference,
                CacheOptions,
                CacheTags,
                cancellationToken
            );
        }

        return resolved;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, string>> ResolveUrlsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    )
    {
        IReadOnlyDictionary<Guid, FileReferenceDto> references = await ResolveManyAsync(fileIds, cancellationToken);

        return references.ToDictionary(entry => entry.Key, entry => entry.Value.StorageUrl);
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
    /// The cache key one file's projection is held under.
    /// </summary>
    /// <param name="fileId">The file.</param>
    /// <returns>The key.</returns>
    private static string CacheKey(Guid fileId) => $"core:file-ref:{fileId}";

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
