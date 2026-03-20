using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IPlaylistRepository" /> for managing playlist entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class PlaylistRepository(ContentDbContext context) : IPlaylistRepository
{
    /// <inheritdoc />
    public async Task AddAsync(PlaylistEntity playlist, CancellationToken cancellationToken = default)
    {
        await context.Playlists.AddAsync(playlist, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PlaylistEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new PlaylistByUserIdSpecification(userId: userId);
        return await context
            .Playlists.ApplySpecification(specification: specification)
            .Include(p => p.Videos)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlaylistEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new PlaylistByIdSpecification(id: id);
        return await context
            .Playlists.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlaylistEntity?> GetByIdWithVideosAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new PlaylistByIdSpecification(id: id);
        return await context
            .Playlists.ApplySpecification(specification: specification)
            .Include(p => p.Videos)
                .ThenInclude(pv => pv.Video)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> VideoExistsInPlaylistAsync(
        Guid playlistId,
        Guid videoId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new PlaylistVideoByPlaylistAndVideoSpecification(playlistId: playlistId, videoId: videoId);
        return await context
            .PlaylistVideos.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddVideoAsync(PlaylistVideoEntity playlistVideo, CancellationToken cancellationToken = default)
    {
        await context.PlaylistVideos.AddAsync(playlistVideo, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveVideoAsync(Guid playlistId, Guid videoId, CancellationToken cancellationToken = default)
    {
        var specification = new PlaylistVideoByPlaylistAndVideoSpecification(playlistId: playlistId, videoId: videoId);
        PlaylistVideoEntity? entry = await context
            .PlaylistVideos.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (entry is not null)
        {
            context.PlaylistVideos.Remove(entry);
        }
    }

    /// <inheritdoc />
    public void Update(PlaylistEntity playlist)
    {
        context.Playlists.Update(playlist);
    }

    /// <inheritdoc />
    public void Delete(PlaylistEntity playlist)
    {
        context.Playlists.Remove(playlist);
    }
}
