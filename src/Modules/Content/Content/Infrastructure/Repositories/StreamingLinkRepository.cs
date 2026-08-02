using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IStreamingLinkRepository" /> for managing streaming platform
/// link entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class StreamingLinkRepository(ContentDbContext context) : IStreamingLinkRepository
{
    /// <inheritdoc />
    public async Task<StreamingLinkEntity?> GetByAlbumAndPlatformAsync(
        Guid albumId,
        EnumStreamingPlatform platform,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new StreamingLinkByAlbumAndPlatformSpecification(albumId: albumId, platform: platform);
        return await context
            .StreamingLinks.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> GetByAlbumAsync(
        Guid albumId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new StreamingLinkByAlbumSpecification(albumId: albumId);
        List<StreamingLinkEntity> links = await context
            .StreamingLinks.ApplySpecification(specification: specification)
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
        var specification = new StreamingLinkByLyricsAndPlatformSpecification(lyricsId: lyricsId, platform: platform);
        return await context
            .StreamingLinks.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<EnumStreamingPlatform, string>> GetByLyricsAsync(
        Guid lyricsId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new StreamingLinkByLyricsSpecification(lyricsId: lyricsId);
        List<StreamingLinkEntity> links = await context
            .StreamingLinks.ApplySpecification(specification: specification)
            .ToListAsync(cancellationToken);

        return links.ToDictionary(link => link.Platform, link => link.Url);
    }

    /// <inheritdoc />
    public async Task AddAsync(StreamingLinkEntity streamingLink, CancellationToken cancellationToken = default)
    {
        await context.StreamingLinks.AddAsync(streamingLink, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(StreamingLinkEntity streamingLink)
    {
        context.StreamingLinks.Update(streamingLink);
    }

    /// <inheritdoc />
    public void Remove(StreamingLinkEntity streamingLink)
    {
        context.StreamingLinks.Remove(streamingLink);
    }
}
