using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks.Contracts;

/// <summary>
/// Resolves the streaming links of an album from a source URL: gates the target, asks the
/// provider, and refuses an empty answer.
/// </summary>
public interface IAdminAlbumLinkResolutionService
{
    /// <summary>
    /// Resolves the platform links, throwing the localized error when the target is not eligible
    /// or nothing resolved. A provider failure surfaces as its own exception.
    /// </summary>
    /// <param name="albumId">The album.</param>
    /// <param name="sourceUrl">The URL handed to the provider.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> ResolveAsync(
        Guid albumId,
        string sourceUrl,
        CancellationToken cancellationToken
    );
}
