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
            .Include(p => p.Videos)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public override async Task<PlaylistEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new PlaylistByIdSpecification(id: id);
        return await Context
            .Playlists.AsTracking()
            .Include(playlist => playlist.Videos)
            .ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<PlaylistEntity?> GetByIdWithVideosAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new PlaylistByIdSpecification(id: id);
        return await Context
            .Playlists.ApplySpecification(specification: specification)
            .Include(p => p.Videos)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Delete(PlaylistEntity playlist)
    {
        Context.Playlists.Remove(playlist);
    }
}
