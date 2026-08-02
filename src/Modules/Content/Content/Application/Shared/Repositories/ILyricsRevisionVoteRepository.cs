using _116.Content.Domain.Entities;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for lyrics-text correction revision vote data access operations.
/// </summary>
public interface ILyricsRevisionVoteRepository : IRepository<LyricsRevisionVoteEntity>
{
    /// <summary>
    /// Adds a new vote to the repository.
    /// </summary>
    /// <param name="vote">The vote entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(LyricsRevisionVoteEntity vote, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true if the given user has already voted on the given revision. Used as the
    /// application-level pre-check before the DB-level unique <c>(RevisionId, UserId)</c>
    /// constraint's backstop enforcement.
    /// </summary>
    /// <param name="revisionId">The lyrics revision to check.</param>
    /// <param name="userId">The identity user UUID of the voter.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task<bool> HasVotedAsync(Guid revisionId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Computes the net approval count (approvals minus rejections) cast on a given revision,
    /// in a single query.
    /// </summary>
    /// <param name="revisionId">The lyrics revision to tally.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The net approval count, which may be negative.</returns>
    Task<int> GetNetApprovalsAsync(Guid revisionId, CancellationToken cancellationToken = default);
}
