using _116.Core.Contracts.Domain.Enums;

namespace _116.Core.Application.Shared.Services;

/// <summary>
/// One asset to store: its bytes plus the identity it should be stored under.
/// </summary>
/// <param name="Content">The asset's bytes. The client reads but does not own the stream.</param>
/// <param name="FileName">The original file name, used by providers that derive a format from it.</param>
/// <param name="PublicId">The provider-side identifier to store the asset under.</param>
/// <param name="Folder">The provider-side folder, or null to store at the root.</param>
/// <param name="Kind">What the asset is, which decides how the provider addresses it.</param>
public record CloudStorageUpload(
    Stream Content,
    string FileName,
    string PublicId,
    string? Folder,
    EnumStoredFileKind Kind
);

/// <summary>
/// A stored asset as the provider describes it back.
/// </summary>
/// <param name="PublicId">The identifier the asset is stored under, folder included.</param>
/// <param name="SecureUrl">The HTTPS URL the asset is served from.</param>
/// <param name="Format">The stored format, for example <c>jpg</c>.</param>
/// <param name="Width">Pixel width, or zero for assets that have no dimensions.</param>
/// <param name="Height">Pixel height, or zero for assets that have no dimensions.</param>
/// <param name="Bytes">The stored size in bytes.</param>
/// <param name="ResourceType">The provider's own name for the asset class.</param>
public record CloudStorageAsset(
    string PublicId,
    string SecureUrl,
    string Format,
    int Width,
    int Height,
    long Bytes,
    string ResourceType
);

/// <summary>
/// The storage provider seam. Deliberately free of any provider SDK type: swapping Cloudinary
/// for another provider replaces the implementation and nothing above it.
/// </summary>
/// <remarks>
/// Implementations own the provider client and the resilience policy, so callers stay
/// policy-free. Uploads throw on provider failure; deletes report failure rather than throwing,
/// because a failed purge must never block the write that triggered it.
/// </remarks>
public interface ICloudStorageClient
{
    /// <summary>
    /// Stores one asset.
    /// </summary>
    /// <param name="upload">The asset to store.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The stored asset.</returns>
    Task<CloudStorageAsset> UploadAsync(CloudStorageUpload upload, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes one asset, addressed under its own kind.
    /// </summary>
    /// <param name="publicId">The asset's provider-side identifier.</param>
    /// <param name="kind">What the asset is; addressing it as the wrong kind is a silent no-op.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when the provider confirmed the removal.</returns>
    Task<bool> DeleteAsync(string publicId, EnumStoredFileKind kind, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes many assets sharing one kind, batching as the provider requires.
    /// </summary>
    /// <param name="publicIds">The assets' provider-side identifiers.</param>
    /// <param name="kind">What the assets are.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>True when every batch was removed.</returns>
    Task<bool> DeleteManyAsync(
        IEnumerable<string> publicIds,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    );
}
