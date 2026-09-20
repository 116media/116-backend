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
public class TranslationVoteRepository(ContentDbContext context)
    : ContentRepository<LyricsTranslationVoteEntity>(context),
        ITranslationVoteRepository
{
    /// <inheritdoc />
    public async Task<bool> HasVotedAsync(Guid revisionId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await Context.LyricsTranslationVotes.AnyAsync(
            vote => vote.RevisionId == revisionId && vote.UserId == userId,
            cancellationToken
        );
    }

    /// <inheritdoc />
    public async Task<int> GetNetApprovalsAsync(Guid revisionId, CancellationToken cancellationToken = default)
    {
        return await Context
            .LyricsTranslationVotes.Where(vote => vote.RevisionId == revisionId)
            .SumAsync(vote => vote.Vote == EnumVote.Approve ? 1 : -1, cancellationToken);
    }
}
