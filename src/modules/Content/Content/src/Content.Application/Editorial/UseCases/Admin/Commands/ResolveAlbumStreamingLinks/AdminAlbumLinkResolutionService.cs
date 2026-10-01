using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Ports;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveAlbumStreamingLinks;

/// <summary>
/// Resolves an album's streaming links for the admin resolve-links use case.
/// </summary>
/// <param name="albumRepository">Repository gating the album.</param>
/// <param name="resolutionService">Provider port resolving the links.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminAlbumLinkResolutionService(
    IAlbumRepository albumRepository,
    IStreamingLinkResolutionService resolutionService,
    ContentI18n i18n
) : IAdminAlbumLinkResolutionService
{
    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> ResolveAsync(
        Guid albumId,
        string sourceUrl,
        CancellationToken cancellationToken
    )
    {
        await albumRepository.GetByIdOrThrowAsync(id: albumId, cancellationToken: cancellationToken);

        IReadOnlyDictionary<EnumStreamingPlatform, string> resolved = await resolutionService.ResolveAsync(
            sourceUrl: sourceUrl,
            cancellationToken: cancellationToken
        );

        if (resolved.Count == 0)
        {
            throw i18n.StreamingLink.NothingResolved();
        }

        return resolved;
    }
}
