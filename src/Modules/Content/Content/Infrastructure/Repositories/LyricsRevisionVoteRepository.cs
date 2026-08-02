using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ILyricsRevisionVoteRepository" /> for managing lyrics-text
/// correction revision vote entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class LyricsRevisionVoteRepository(ContentDbContext context) : ILyricsRevisionVoteRepository
{
    /// <inheritdoc />
    public async Task AddAsync(LyricsRevisionVoteEntity vote, CancellationToken cancellationToken = default)
    {
        await context.LyricsRevisionVotes.AddAsync(vote, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasVotedAsync(Guid revisionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var specification = new LyricsRevisionVoteByRevisionAndUserSpecification(
            revisionId: revisionId,
            userId: userId
        );
        return await context
            .LyricsRevisionVotes.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetNetApprovalsAsync(Guid revisionId, CancellationToken cancellationToken = default)
    {
        var specification = new LyricsRevisionVoteByRevisionIdSpecification(revisionId: revisionId);
        return await context
            .LyricsRevisionVotes.ApplySpecification(specification: specification)
            .SumAsync(vote => vote.Vote == EnumVote.Approve ? 1 : -1, cancellationToken);
    }
}
