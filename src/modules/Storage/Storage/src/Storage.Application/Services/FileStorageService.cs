using _116.Storage.Application.Cache;
using _116.Storage.Application.Mappers;
using _116.Storage.Application.Ports;
using _116.Storage.Application.Repositories;
using _116.Storage.Contracts.Application.DTOs;
using _116.Storage.Contracts.Application.Services;
using _116.Storage.Contracts.Domain.Enums;
using _116.Storage.Domain.Entities;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Hybrid;

namespace _116.Storage.Application.Services;

/// <summary>
/// Storage's implementation of the cross-module storage contract. Translates between the opaque
/// <see cref="FileReferenceDto" /> other modules hold and the file aggregate Storage owns.
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

    private static readonly string[] CacheTags = [StorageCacheTags.Files];

    /// <summary>
    /// Reads the cache without running a factory, so a probe never falls through to the
    /// repository and never stores a miss.
    /// </summary>
    private static readonly HybridCacheEntryOptions ProbeOptions = new()
    {
        Flags = HybridCacheEntryFlags.DisableUnderlyingData,
    };

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
    /// <remarks>
    /// Serves what the cache already holds and queries only the rest, with the probes and
    /// the stores each going out together rather than one round trip per file.
    /// </remarks>
    public async Task<IReadOnlyDictionary<Guid, FileReferenceDto>> ResolveManyAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    )
    {
        Guid[] distinct = [.. fileIds.Distinct()];

        FileReferenceDto?[] cached = await Task.WhenAll(distinct.Select(id => ProbeAsync(id, cancellationToken)));

        var resolved = new Dictionary<Guid, FileReferenceDto>(distinct.Length);
        var missing = new List<Guid>(distinct.Length);

        for (int index = 0; index < distinct.Length; index++)
        {
            if (cached[index] is FileReferenceDto hit)
            {
                resolved[distinct[index]] = hit;
            }
            else
            {
                missing.Add(distinct[index]);
            }
        }

        if (missing.Count == 0)
        {
            return resolved;
        }

        IReadOnlyDictionary<Guid, FileEntity> files = await fileRepository.GetByIdsAsync(
            fileIds: missing,
            cancellationToken: cancellationToken
        );

        var writes = new List<Task>(files.Count);

        foreach ((Guid id, FileEntity file) in files)
        {
            FileReferenceDto reference = file.ToFileReferenceDto(mapper);
            resolved[id] = reference;

            writes.Add(
                cache
                    .SetAsync<FileReferenceDto?>(CacheKey(id), reference, CacheOptions, CacheTags, cancellationToken)
                    .AsTask()
            );
        }

        // The writes are independent, so they go out together rather than one round trip per file.
        await Task.WhenAll(writes);

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

    /// <summary>
    /// Reads one entry without falling through to the repository, so a batch can find out
    /// what it already has before querying for the rest.
    /// </summary>
    /// <param name="fileId">The file to look for.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The cached reference, or null when the cache does not hold it.</returns>
    private async Task<FileReferenceDto?> ProbeAsync(Guid fileId, CancellationToken cancellationToken)
    {
        return await cache.GetOrCreateAsync<Guid, FileReferenceDto?>(
            CacheKey(fileId),
            fileId,
            static (_, _) => default,
            ProbeOptions,
            CacheTags,
            cancellationToken
        );
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
