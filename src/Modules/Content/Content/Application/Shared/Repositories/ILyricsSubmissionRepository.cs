using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Application.Shared.Repositories;

/// <summary>
/// Repository interface for community lyrics submission data access operations.
/// </summary>
public interface ILyricsSubmissionRepository : IRepository<LyricsSubmissionEntity>
{
    /// <summary>
    /// Retrieves a lyrics submission by its unique identifier.
    /// Returns null if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the submission.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The submission entity if found, otherwise null.</returns>
    Task<LyricsSubmissionEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a lyrics submission by its unique identifier.
    /// Throws a NotFoundException if not found.
    /// </summary>
    /// <param name="id">The unique identifier of the submission.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The submission entity.</returns>
    /// <exception cref="_116.Shared.Application.Exceptions.NotFoundException">Thrown when the submission is not found.</exception>
    Task<LyricsSubmissionEntity> GetByIdOrThrowAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a paginated moderation queue of lyrics submissions, optionally filtered by
    /// status.
    /// </summary>
    /// <param name="page">The 1-based page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="status">Optional filter by moderation status.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>A tuple containing the list of submissions and the total count.</returns>
    Task<(List<LyricsSubmissionEntity> Submissions, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        EnumSubmissionStatus? status,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Reconciliation query (this feature's documented ACID safety net): finds every
    /// still-<c>Pending</c> submission for which a <see cref="LyricsEntity" /> matching its
    /// song title and artist name has already been created. In the normal approval sequence,
    /// the created lyrics record's own commit and the submission's <c>Approve</c> commit are
    /// two separate, individually-safe steps — if the process is interrupted between them, the
    /// lyrics record exists correctly but the submission is left behind in <c>Pending</c>. This
    /// surfaces that detectable, repairable state for ops.
    /// <para>
    /// Matches by song title and artist name rather than slug — the submission itself never
    /// carries a slug (the slug is only decided at approval time, from the reviewer's input),
    /// so title/artist name is the closest available correlation key.
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    /// <returns>The set of pending submissions with a matching, already-created lyrics record.</returns>
    Task<IReadOnlyList<LyricsSubmissionEntity>> GetPendingWithMatchingLyricsAsync(
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Adds a new lyrics submission to the repository.
    /// </summary>
    /// <param name="submission">The submission entity to add.</param>
    /// <param name="cancellationToken">Token to observe for cancellation requests.</param>
    Task AddAsync(LyricsSubmissionEntity submission, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks an existing lyrics submission as modified.
    /// </summary>
    /// <param name="submission">The submission entity to update.</param>
    void Update(LyricsSubmissionEntity submission);
}
