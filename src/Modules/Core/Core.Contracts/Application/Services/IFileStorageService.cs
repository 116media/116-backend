using _116.Core.Contracts.Application.DTOs;
using _116.Core.Contracts.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace _116.Core.Contracts.Application.Services;

/// <summary>
/// An asset that is in storage but has no row behind it yet. Returned by the store's upload
/// methods and passed back to <see cref="IFileStorageService.RecordAsync" /> inside the caller's
/// transaction, so the asset and the row referencing it land together. Consuming modules read
/// <see cref="Reference" /> and treat the instance itself as opaque.
/// </summary>
public sealed record StoredFile
{
    /// <summary>
    /// The reference describing what now sits in storage.
    /// </summary>
    public required FileReferenceDto Reference { get; init; }

    /// <summary>
    /// Only the storage implementation may issue a handle, so a recorded asset is always one
    /// this store uploaded.
    /// </summary>
    internal StoredFile() { }
}

/// <summary>
/// The storage capability Core exposes to other modules. Upload, record, resolve and delete —
/// no entity, no persistence detail and no provider type crosses this seam.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads an asset of the given kind. Performs no database work; the asset has no row
    /// behind it until <see cref="RecordAsync" /> runs.
    /// </summary>
    /// <param name="file">The asset to upload.</param>
    /// <param name="publicId">The storage public id.</param>
    /// <param name="folder">The destination storage folder.</param>
    /// <param name="kind">The storage class the asset is stored and deleted as.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The unrecorded asset.</returns>
    Task<StoredFile> UploadAsync(
        IFormFile file,
        string publicId,
        string folder,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Fetches an asset from a URL. Performs no database work. Returns null when
    /// <paramref name="currentFileId" /> already points at that URL.
    /// </summary>
    /// <param name="currentFileId">The file in use, or null when there is none.</param>
    /// <param name="url">The URL to fetch from.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The unrecorded asset, or null when no update was needed.</returns>
    Task<StoredFile?> UploadFromUrlAsync(
        Guid? currentFileId,
        string url,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stages the asset's row and the replacement of the file it supersedes. Call inside the
    /// transaction that also writes the row referencing it, so both land together.
    /// </summary>
    /// <param name="file">The unrecorded asset returned by an upload.</param>
    /// <param name="supersededFileId">The file this one replaces, or null when none.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The reference to the recorded file.</returns>
    Task<FileReferenceDto> RecordAsync(
        StoredFile file,
        Guid? supersededFileId = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Resolves one reference, or null when the file is absent or deleted.
    /// </summary>
    /// <param name="fileId">The file to resolve.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The reference, or null.</returns>
    Task<FileReferenceDto?> ResolveAsync(Guid? fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves references in one round trip; absent ids are simply missing from the result.
    /// </summary>
    /// <param name="fileIds">The files to resolve.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The references, keyed by file id.</returns>
    Task<IReadOnlyDictionary<Guid, FileReferenceDto>> ResolveManyAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Resolves storage URLs in one round trip; absent ids are missing from the result.
    /// </summary>
    /// <param name="fileIds">The files to resolve.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The storage URLs, keyed by file id.</returns>
    Task<IReadOnlyDictionary<Guid, string>> ResolveUrlsAsync(
        IReadOnlyCollection<Guid> fileIds,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Soft-deletes the file's row; the stored asset is removed by the resulting domain event
    /// using the file's own kind.
    /// </summary>
    /// <param name="fileId">The file to delete.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when a row was deleted.</returns>
    Task<bool> DeleteAsync(Guid fileId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes stored assets by their storage keys, using the given kind for each.
    /// </summary>
    /// <param name="storageKeys">The storage keys to remove.</param>
    /// <param name="kind">The storage class the assets were stored as.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when every asset was removed.</returns>
    Task<bool> DeleteAssetsAsync(
        IEnumerable<string> storageKeys,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    );
}
