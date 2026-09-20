using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IStreamingLinkRepository" /> for managing streaming platform
/// link entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class StreamingLinkRepository(ContentDbContext context)
    : ContentRepository<StreamingLinkEntity>(context),
        IStreamingLinkRepository
{
    /// <inheritdoc />
    public async Task<StreamingLinkEntity?> GetByAlbumAndPlatformAsync(
        Guid albumId,
        EnumStreamingPlatform platform,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .StreamingLinks.AsTracking()
            .FirstOrDefaultAsync(link => link.AlbumId == albumId && link.Platform == platform, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> GetByAlbumAsync(
        Guid albumId,
        CancellationToken cancellationToken = default
    )
    {
        List<StreamingLinkEntity> links = await Context
            .StreamingLinks.Where(link => link.AlbumId == albumId)
            .ToListAsync(cancellationToken);

        return links.ToDictionary(link => link.Platform, link => link.Url);
    }

    /// <inheritdoc />
    public async Task<StreamingLinkEntity?> GetByLyricsAndPlatformAsync(
        Guid lyricsId,
        EnumStreamingPlatform platform,
        CancellationToken cancellationToken = default
    )
    {
        return await Context
            .StreamingLinks.AsTracking()
            .FirstOrDefaultAsync(link => link.LyricsId == lyricsId && link.Platform == platform, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> GetByLyricsAsync(
        Guid lyricsId,
        CancellationToken cancellationToken = default
    )
    {
        List<StreamingLinkEntity> links = await Context
            .StreamingLinks.Where(link => link.LyricsId == lyricsId)
            .ToListAsync(cancellationToken);

        return links.ToDictionary(link => link.Platform, link => link.Url);
    }
}
