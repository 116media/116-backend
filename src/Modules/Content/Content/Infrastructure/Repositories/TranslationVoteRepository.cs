using _116.Content.Application.Editorial.Specifications;
using _116.Content.Application.Shared.Repositories;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Infrastructure.Persistence;
using _116.Shared.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace _116.Content.Infrastructure.Repositories;

/// <summary>
/// Implementation of <see cref="ITranslationVoteRepository" /> for managing translation
/// revision vote entities.
/// </summary>
/// <param name="context">The Content module database context.</param>
public class TranslationVoteRepository(ContentDbContext context) : ITranslationVoteRepository
{
    /// <inheritdoc />
    public async Task AddAsync(LyricsTranslationVoteEntity vote, CancellationToken cancellationToken = default)
    {
        await context.LyricsTranslationVotes.AddAsync(vote, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> HasVotedAsync(Guid revisionId, Guid userId, CancellationToken cancellationToken = default)
    {
        var specification = new TranslationVoteByRevisionAndUserSpecification(revisionId: revisionId, userId: userId);
        return await context
            .LyricsTranslationVotes.ApplySpecification(specification: specification)
            .AnyAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetNetApprovalsAsync(Guid revisionId, CancellationToken cancellationToken = default)
    {
        var specification = new TranslationVoteByRevisionIdSpecification(revisionId: revisionId);
        return await context
            .LyricsTranslationVotes.ApplySpecification(specification: specification)
            .SumAsync(vote => vote.Vote == EnumVote.Approve ? 1 : -1, cancellationToken);
    }
}
