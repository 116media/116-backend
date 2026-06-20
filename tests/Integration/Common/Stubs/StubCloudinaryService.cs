using _116.Core.Application.Shared.Services;
using Microsoft.AspNetCore.Http;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// In-memory stub that returns fake Cloudinary URLs without making real HTTP calls.
/// </summary>
public class StubCloudinaryService : ICloudinaryService
{
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
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<bool> DeleteImagesAsync(IEnumerable<string> publicIds, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
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
