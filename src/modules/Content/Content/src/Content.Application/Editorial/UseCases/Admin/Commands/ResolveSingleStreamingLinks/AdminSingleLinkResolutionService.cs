using _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks.Contracts;
using _116.Content.Application.Shared.Errors.Facade;
using _116.Content.Application.Shared.Ports;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Content.Application.Editorial.UseCases.Admin.Commands.ResolveSingleStreamingLinks;

/// <summary>
/// Resolves a single's streaming links for the admin resolve-links use case.
/// </summary>
/// <param name="lyricsRepository">Repository gating the single.</param>
/// <param name="resolutionService">Provider port resolving the links.</param>
/// <param name="i18n">Single i18n entry point for the Content module.</param>
public class AdminSingleLinkResolutionService(
    ILyricsRepository lyricsRepository,
    IStreamingLinkResolutionService resolutionService,
    ContentI18n i18n
) : IAdminSingleLinkResolutionService
{
    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> ResolveAsync(
        Guid lyricsId,
        string sourceUrl,
        CancellationToken cancellationToken
    )
    {
        LyricsEntity lyrics = await lyricsRepository.GetByIdOrThrowAsync(
            id: lyricsId,
            cancellationToken: cancellationToken
        );

        if (lyrics.AlbumId.HasValue)
        {
            throw i18n.Lyrics.BelongsToAlbum();
        }

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
