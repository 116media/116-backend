using _116.Content.Application.Editorial.Services;
using Microsoft.AspNetCore.Http;

namespace _116.Content.Infrastructure.Services;

/// <summary>
/// Downloads YouTube video thumbnails via HTTP.
/// Tries <c>maxresdefault.jpg</c> first; falls back to <c>hqdefault.jpg</c> if unavailable.
/// </summary>
/// <param name="httpClient">The HTTP client used to fetch thumbnail images.</param>
public class YoutubeThumbnailService(HttpClient httpClient) : IYoutubeThumbnailService
{
    /// <inheritdoc />
    public async Task<IFormFile> DownloadThumbnailAsync(
        string youtubeVideoId,
        CancellationToken cancellationToken = default
    )
    {
        string thumbUrl = $"https://img.youtube.com/vi/{youtubeVideoId}/maxresdefault.jpg";

        byte[] imageBytes;

        try
        {
            imageBytes = await httpClient.GetByteArrayAsync(thumbUrl, cancellationToken);
        }
        catch
        {
            string fallbackUrl = $"https://img.youtube.com/vi/{youtubeVideoId}/hqdefault.jpg";
            imageBytes = await httpClient.GetByteArrayAsync(fallbackUrl, cancellationToken);
        }

        var memStream = new MemoryStream(imageBytes);

        return new FormFile(
            baseStream: memStream,
            baseStreamOffset: 0,
            length: imageBytes.Length,
            name: "thumbnail",
            fileName: "thumbnail.jpg"
        )
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg",
        };
    }
}
