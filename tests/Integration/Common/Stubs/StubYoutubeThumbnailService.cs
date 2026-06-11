using _116.Content.Application.Editorial.Services;
using Microsoft.AspNetCore.Http;

namespace _116.Integration.Tests.Common.Stubs;

/// <summary>
/// In-memory stub that returns a fake thumbnail without downloading from YouTube.
/// </summary>
public class StubYoutubeThumbnailService : IYoutubeThumbnailService
{
    /// <inheritdoc />
    public Task<IFormFile> DownloadThumbnailAsync(string youtubeVideoId, CancellationToken cancellationToken = default)
    {
        byte[] fakeImage = new byte[64];
        var stream = new MemoryStream(fakeImage);

        IFormFile file = new FormFile(
            baseStream: stream,
            baseStreamOffset: 0,
            length: fakeImage.Length,
            name: "thumbnail",
            fileName: $"{youtubeVideoId}_thumbnail.jpg"
        )
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg",
        };

        return Task.FromResult(file);
    }
}
