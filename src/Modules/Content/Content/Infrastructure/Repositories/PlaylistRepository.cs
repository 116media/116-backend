using _116.Content.Application.Interactions.Persistence;
using _116.Content.Application.Interactions.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="IPlaylistRepository" /> for managing playlist entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class PlaylistRepository(ContentDbContext context)
    : ContentRepository<PlaylistEntity>(context),
        IPlaylistRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<PlaylistEntity>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new PlaylistByUserIdSpecification(userId: userId);
        return await Context
            .Playlists.ApplySpecification(specification: specification)
            .Include(p => p.Videos.Where(pv => pv.Video.Status == EnumContentStatus.Published))
                .ThenInclude(pv => pv.Video)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<PlaylistEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new PlaylistByIdSpecification(id: id);
        return await Context
            .Playlists.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlaylistEntity?> GetByIdWithVideosAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new PlaylistByIdSpecification(id: id);
        return await Context
            .Playlists.ApplySpecification(specification: specification)
            .Include(p => p.Videos.Where(pv => pv.Video.Status == EnumContentStatus.Published))
                .ThenInclude(pv => pv.Video)
                    .ThenInclude(video => video.Category)
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
        return await Context
            .PlaylistVideos.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddVideoAsync(PlaylistVideoEntity playlistVideo, CancellationToken cancellationToken = default)
    {
        await Context.PlaylistVideos.AddAsync(playlistVideo, cancellationToken);
    }

    /// <inheritdoc />
    public async Task RemoveVideoAsync(Guid playlistId, Guid videoId, CancellationToken cancellationToken = default)
    {
        var specification = new PlaylistVideoByPlaylistAndVideoSpecification(playlistId: playlistId, videoId: videoId);
        PlaylistVideoEntity? entry = await Context
            .PlaylistVideos.AsTracking()
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);

        if (entry is not null)
        {
            Context.PlaylistVideos.Remove(entry);
        }
    }

    /// <inheritdoc />
    public void Delete(PlaylistEntity playlist)
    {
        Context.Playlists.Remove(playlist);
    }
}
