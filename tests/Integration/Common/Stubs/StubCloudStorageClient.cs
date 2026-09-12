using _116.Core.Application.Shared.Services;
using _116.Core.Contracts.Domain.Enums;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// In-memory stub of the storage provider seam. Sitting below <c>CloudinaryService</c> rather
/// than replacing it means the real validation and result projection run under integration,
/// while no request leaves the process.
/// </summary>
public class StubCloudStorageClient : ICloudStorageClient, IResettableStub
{
    /// <summary>
    /// When set, the next delete call throws this exception once and clears the field. Uploads
    /// are unaffected.
    /// </summary>
    public Exception? NextDeleteFailure { get; set; }

    /// <summary>
    /// Every public id handed to a delete call, in call order. Ids queued behind
    /// <see cref="NextDeleteFailure" /> are recorded before the failure is raised.
    /// </summary>
    public List<string> DeletedPublicIds { get; } = [];

    /// <summary>
    /// The kind each delete call carried, in call order. This is what proves a video or raw
    /// asset is not addressed as an image.
    /// </summary>
    public List<EnumStoredFileKind> DeletedKinds { get; } = [];

    /// <summary>
    /// Every public id handed to an upload call, in call order, so a test can assert that a
    /// request rejected on its way in never reached storage.
    /// </summary>
    public List<string> UploadedPublicIds { get; } = [];

    /// <inheritdoc />
    public void Reset()
    {
        NextDeleteFailure = null;
        DeletedPublicIds.Clear();
        DeletedKinds.Clear();
        UploadedPublicIds.Clear();
    }

    /// <inheritdoc />
    public Task<CloudStorageAsset> UploadAsync(CloudStorageUpload upload, CancellationToken cancellationToken = default)
    {
        UploadedPublicIds.Add(upload.PublicId);

        (string resourceType, string format) = upload.Kind switch
        {
            EnumStoredFileKind.Video => ("video", "mp4"),
            EnumStoredFileKind.Raw => ("raw", "pdf"),
            _ => ("image", "jpg"),
        };

        string path = string.IsNullOrEmpty(upload.Folder) ? upload.PublicId : $"{upload.Folder}/{upload.PublicId}";

        return Task.FromResult(
            new CloudStorageAsset(
                PublicId: path,
                SecureUrl: $"https://res.cloudinary.com/test-cloud/{resourceType}/upload/{path}.{format}",
                Format: format,
                Width: upload.Kind == EnumStoredFileKind.Raw ? 0 : 800,
                Height: upload.Kind == EnumStoredFileKind.Raw ? 0 : 600,
                Bytes: 1024,
                ResourceType: resourceType
            )
        );
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(
        string publicId,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    )
    {
        DeletedPublicIds.Add(publicId);
        DeletedKinds.Add(kind);
        ThrowNextDeleteFailureIfSet();

        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> DeleteManyAsync(
        IEnumerable<string> publicIds,
        EnumStoredFileKind kind,
        CancellationToken cancellationToken = default
    )
    {
        List<string> keys = [.. publicIds];

        DeletedPublicIds.AddRange(keys);
        DeletedKinds.AddRange(keys.Select(_ => kind));
        ThrowNextDeleteFailureIfSet();

        return Task.FromResult(true);
    }

    /// <summary>
    /// Throws the queued delete failure once and clears it.
    /// </summary>
    private void ThrowNextDeleteFailureIfSet()
    {
        if (NextDeleteFailure is not null)
        {
            Exception failure = NextDeleteFailure;
            NextDeleteFailure = null;
            throw failure;
        }
    }
}
