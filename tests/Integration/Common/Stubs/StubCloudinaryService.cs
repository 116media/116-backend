using _116.Core.Application.Shared.Services;
using Microsoft.AspNetCore.Http;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// In-memory stub that returns fake Cloudinary URLs without making real HTTP calls.
/// Setting <see cref="NextDeleteFailure" /> makes the next delete call throw
/// once, driving the failure-tolerance path of post-commit asset cleanup
/// without any provider.
/// </summary>
public class StubCloudinaryService : ICloudinaryService
{
    /// <summary>
    /// When set, the next <see cref="DeleteImageAsync" /> or
    /// <see cref="DeleteImagesAsync" /> call throws this exception once and
    /// clears the field. Uploads are unaffected.
    /// </summary>
    public Exception? NextDeleteFailure { get; set; }

    /// <summary>
    /// Every public id handed to a delete call, in call order, so a test can
    /// assert which remote assets a committed mutation asked the provider to
    /// purge. Ids queued behind <see cref="NextDeleteFailure" /> are recorded
    /// before the failure is raised, matching the provider's own semantics of
    /// having received the request.
    /// </summary>
    public List<string> DeletedPublicIds { get; } = [];

    /// <inheritdoc />
    public Task<CloudinaryUploadResult> UploadImageAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(CreateResult(publicId, folder, "image", "jpg"));
    }

    /// <inheritdoc />
    public Task<CloudinaryUploadResult> UploadRawAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(CreateResult(publicId, folder, "raw", "pdf"));
    }

    /// <inheritdoc />
    public Task<CloudinaryUploadResult> UploadVideoAsync(
        IFormFile file,
        string publicId,
        string? folder = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(CreateResult(publicId, folder, "video", "mp4"));
    }

    /// <inheritdoc />
    public Task<bool> DeleteImageAsync(string publicId, CancellationToken cancellationToken = default)
    {
        DeletedPublicIds.Add(publicId);
        ThrowNextDeleteFailureIfSet();
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> DeleteImagesAsync(IEnumerable<string> publicIds, CancellationToken cancellationToken = default)
    {
        DeletedPublicIds.AddRange(publicIds);
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

    private static CloudinaryUploadResult CreateResult(
        string publicId,
        string? folder,
        string resourceType,
        string format
    )
    {
        string path = folder is not null ? $"{folder}/{publicId}" : publicId;
        return new CloudinaryUploadResult(
            PublicId: path,
            SecureUrl: $"https://res.cloudinary.com/test-cloud/{resourceType}/upload/{path}.{format}",
            Format: format,
            Width: 800,
            Height: 600,
            Bytes: 1024,
            ResourceType: resourceType
        );
    }
}
