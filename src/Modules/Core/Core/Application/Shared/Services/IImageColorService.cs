using Microsoft.AspNetCore.Http;

namespace _116.Core.Application.Shared.Services;

/// <summary>
/// The pair of colors derived from an image: the dominant color (used as a
/// background) and the contrasting foreground (text) color computed from it.
/// </summary>
/// <param name="DominantColorHex">The most-dominant color of the image as <c>#RRGGBB</c>.</param>
/// <param name="ForegroundColorHex">The accessible text color for the dominant color as <c>#RRGGBB</c> (black or white).</param>
public record ImageColors(string DominantColorHex, string ForegroundColorHex);

/// <summary>
/// Extracts the dominant color from an image and derives a contrasting
/// foreground color, decoupled from any specific storage provider so the
/// behavior survives a future move off Cloudinary.
/// </summary>
/// <remarks>
/// Color extraction is best-effort: it must never fail an upload. Implementations
/// return <c>null</c> when the file is not a decodable image or carries no usable
/// color (e.g. fully transparent), in which case the caller stores no colors.
/// </remarks>
public interface IImageColorService
{
    /// <summary>
    /// Extracts the dominant color from an uploaded image and computes its
    /// contrasting foreground color.
    /// </summary>
    /// <param name="file">The uploaded image file to analyze.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// The dominant/foreground color pair, or <c>null</c> when the file cannot be
    /// decoded as an image or yields no usable color.
    /// </returns>
    Task<ImageColors?> ExtractAsync(IFormFile file, CancellationToken cancellationToken = default);
}
