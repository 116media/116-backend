using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// A single user's vote on a pending translation revision. Enforced to be unique per
/// <c>(RevisionId, UserId)</c> pair by the entity configuration's DB-level unique index — the
/// actual one-vote-per-user enforcement mechanism, not application logic.
/// </summary>
public class LyricsTranslationVoteEntity : Aggregate<Guid>
{
    /// <summary>
    /// The translation revision being voted on.
    /// </summary>
    public Guid RevisionId { get; private set; }

    /// <summary>
    /// The identity user UUID of the voter. No FK to the identity schema by design.
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Whether the voter approves or rejects the proposed revision.
    /// </summary>
    public EnumVote Vote { get; private set; }

    /// <summary>
    /// Optional free-text comment justifying the vote.
    /// </summary>
    public string? Comment { get; private set; }

    private LyricsTranslationVoteEntity() { }

    /// <summary>
    /// Casts a new vote on a pending translation revision.
    /// </summary>
    /// <param name="id">The unique identifier for this vote.</param>
    /// <param name="revisionId">The translation revision being voted on.</param>
    /// <param name="userId">The identity user UUID of the voter.</param>
    /// <param name="vote">Whether the voter approves or rejects the revision.</param>
    /// <param name="comment">Optional free-text comment.</param>
    /// <returns>A new <see cref="LyricsTranslationVoteEntity" />.</returns>
    public static LyricsTranslationVoteEntity Create(
        Guid id,
        Guid revisionId,
        Guid userId,
        EnumVote vote,
        string? comment
    )
    {
        return new LyricsTranslationVoteEntity
        {
            Id = id,
            RevisionId = revisionId,
            UserId = userId,
            Vote = vote,
            Comment = comment,
        };
    }
}
