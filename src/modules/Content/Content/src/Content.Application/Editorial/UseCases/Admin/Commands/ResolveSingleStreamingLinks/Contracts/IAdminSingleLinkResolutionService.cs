using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks.Contracts;

/// <summary>
/// Resolves the streaming links of a single from a source URL: gates the target, asks the
/// provider, and refuses an empty answer.
/// </summary>
public interface IAdminSingleLinkResolutionService
{
    /// <summary>
    /// Resolves the platform links, throwing the localized error when the target is not eligible
    /// or nothing resolved. A provider failure surfaces as its own exception.
    /// </summary>
    /// <param name="lyricsId">The single.</param>
    /// <param name="sourceUrl">The URL handed to the provider.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> ResolveAsync(
        Guid lyricsId,
        string sourceUrl,
        CancellationToken cancellationToken
    );
}
