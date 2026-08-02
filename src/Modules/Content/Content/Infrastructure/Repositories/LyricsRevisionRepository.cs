using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ILyricsRevisionRepository" /> for managing lyrics-text
/// community correction revision entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class LyricsRevisionRepository(ContentDbContext context) : ILyricsRevisionRepository
{
    /// <inheritdoc />
    public async Task<LyricsRevisionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new LyricsRevisionByIdSpecification(id: id);
        return await context
            .LyricsRevisions.ApplySpecification(specification: specification)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<LyricsRevisionEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var specification = new LyricsRevisionByIdSpecification(id: id);
        return await context
            .LyricsRevisions.ApplySpecification(specification: specification)
            .FirstDefaultOrThrowAsync(keyValue: id, cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task AddAsync(LyricsRevisionEntity revision, CancellationToken cancellationToken = default)
    {
        await context.LyricsRevisions.AddAsync(revision, cancellationToken);
    }

    /// <inheritdoc />
    public void Update(LyricsRevisionEntity revision)
    {
        context.LyricsRevisions.Update(revision);
    }
}
