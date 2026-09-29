using Microsoft.AspNetCore.Http;

namespace _116.Content.Application.Editorial.Services;

/// <summary>
/// Provides YouTube thumbnail download capabilities.
/// Fetches the best available thumbnail image for a given YouTube video ID,
/// falling back to a lower resolution when the high-resolution version is unavailable.
/// </summary>
public interface IYoutubeThumbnailService
{
    /// <summary>
    /// Downloads the thumbnail for the specified YouTube video as an <see cref="IFormFile" />,
    /// ready for upload to Cloudinary.
    /// Attempts <c>maxresdefault.jpg</c> first; falls back to <c>hqdefault.jpg</c> if unavailable.
    /// </summary>
    /// <param name="youtubeVideoId">The YouTube video identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An <see cref="IFormFile" /> containing the downloaded thumbnail image.</returns>
    Task<IFormFile> DownloadThumbnailAsync(string youtubeVideoId, CancellationToken cancellationToken = default);
}
