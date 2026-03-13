using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ILyricsRepository" /> for managing lyrics entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class LyricsRepository(ContentDbContext context) : ILyricsRepository
{
    /// <inheritdoc />
    public async Task<(List<LyricsEntity> Lyrics, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default
    )
    {
        IQueryable<LyricsEntity> query = context.Lyrics;

        int totalCount = await query.CountAsync(cancellationToken);

        List<LyricsEntity> lyrics = await query
            .OrderBy(l => l.SongTitle)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (lyrics, totalCount);
    }

    /// <inheritdoc />
    public async Task<LyricsEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new LyricsByIdSpecification(id: id);
        return await context.Lyrics.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<LyricsEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new LyricsByIdSpecification(id: id);
        return await context
            .Lyrics.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LyricsEntity?> GetBySongTitleAndArtistAsync(
        string songTitle,
        string artistName,
        CancellationToken cancellationToken = default
    )
    {
        var specification = new LyricsBySongAndArtistSpecification(songTitle: songTitle, artistName: artistName);
        return await context.Lyrics.FirstOrDefaultBySpecificationAsync(
            specification: specification,
            cancellationToken: cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task AddAsync(LyricsEntity lyrics, CancellationToken cancellationToken = default)
    {
        await context.Lyrics.AddAsync(lyrics, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(LyricsEntity lyrics)
    {
        context.Lyrics.Update(lyrics);
    }
}
