# External Service Stubs

## Why Stub External Services

Integration tests use real implementations for everything **except** services that make network calls to third-party APIs:

| Service | Interface | Why Stub |
|---------|-----------|----------|
| Cloudinary | `ICloudinaryService` | File upload/delete to cloud storage |
| YouTube Thumbnail | `IYoutubeThumbnailService` | HTTP call to YouTube for video thumbnails |
| File Service | `IFileService` | Wraps Cloudinary operations |

Stubbing these services avoids:
- Network dependency (flaky tests)
- API rate limits on third-party services
- Test data pollution in cloud storage
- Credentials requirement in CI

## Stub Implementations

### StubCloudinaryService

```csharp
using _116.Core.Application.Shared.Services;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Stub Cloudinary service that returns deterministic fake URLs
/// without making real HTTP calls.
/// </summary>
public class StubCloudinaryService : ICloudinaryService
{
    public Task<string> UploadImageAsync(
        Stream stream,
        string publicId,
        string folder,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            $"https://res.cloudinary.com/test/{folder}/{publicId}.jpg");
    }

    public Task<string> ReplaceImageAsync(
        Stream stream,
        string publicId,
        string folder,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(
            $"https://res.cloudinary.com/test/{folder}/{publicId}.jpg");
    }

    public Task DeleteAsync(
        string publicId,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
```

### StubYoutubeThumbnailService

```csharp
using _116.Content.Application.Shared.Services;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// Stub YouTube thumbnail service that returns a fake thumbnail URL
/// without making real HTTP calls.
/// </summary>
public class StubYoutubeThumbnailService : IYoutubeThumbnailService
{
    public Task<string?> GetThumbnailUrlAsync(
        string videoUrl,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(
            "https://i.ytimg.com/vi/fake/hqdefault.jpg");
    }
}
```

## Registration in ApiFixture

```csharp
private static void StubExternalServices(IServiceCollection services)
{
    // Remove real implementations
    RemoveService<ICloudinaryService>(services);
    RemoveService<IYoutubeThumbnailService>(services);

    // Register stubs
    services.AddScoped<ICloudinaryService, StubCloudinaryService>();
    services.AddScoped<IYoutubeThumbnailService, StubYoutubeThumbnailService>();
}

private static void RemoveService<T>(IServiceCollection services)
{
    ServiceDescriptor? descriptor = services.SingleOrDefault(
        d => d.ServiceType == typeof(T));
    if (descriptor is not null) services.Remove(descriptor);
}
```

## When NOT to Stub

Do **not** stub:
- **Repositories** — use real EF Core against real PostgreSQL
- **Password service** — BCrypt hashing is fast enough and must be tested end-to-end
- **JWT service** — token generation and validation must work for auth tests
- **OTP service** — in-memory OTP generation is fine
- **Session metadata service** — uses `HttpContext`, available in the test pipeline
- **Localization** — `.resx` resources are part of the app assemblies

The principle: stub only what crosses the process boundary (network calls). Everything else runs as production code.

## Testing File Upload Endpoints

Even with stubbed Cloudinary, file upload endpoints need a real `IFormFile`. Use `MultipartFormDataContent`:

```csharp
[Fact]
public async Task Post_WithPosterFile_ShouldReturn201()
{
    // Arrange
    Client.AuthenticateAsAdmin();

    using var content = new MultipartFormDataContent();
    content.Add(new StringContent("Category Name"), "name");
    content.Add(new StringContent("category-slug"), "slug");
    content.Add(new StringContent("Description"), "description");

    // Add a fake image file
    var imageBytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
    content.Add(
        new ByteArrayContent(imageBytes),
        "poster",
        "poster.png");

    // Act
    HttpResponseMessage response = await Client.PostAsync(
        $"{ApiRoutes.Admin.Categories}", content);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
}
```

The stub Cloudinary service will receive the file stream and return a fake URL. The file metadata (name, mime type, size) will be stored in the real `core.files` table via the real `FileRepository`.
