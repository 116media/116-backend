using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="LyricsTranslationVoteEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; LyricsTranslationVoteFactory only names chains three or more tests share.
/// </summary>
public class LyricsTranslationVoteBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _revisionId = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();
    private EnumVote _vote = EnumVote.Approve;

    /// <summary>
    /// Sets the translation revision being voted on.
    /// </summary>
    public LyricsTranslationVoteBuilder WithRevisionId(Guid revisionId)
    {
        _revisionId = revisionId;
        return this;
    }

    /// <summary>
    /// Sets the identity user UUID of the voter.
    /// </summary>
    public LyricsTranslationVoteBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the cast vote direction.
    /// </summary>
    public LyricsTranslationVoteBuilder WithVote(EnumVote vote)
    {
        _vote = vote;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="LyricsTranslationVoteEntity"/> instance.
    /// </summary>
    public LyricsTranslationVoteEntity Build()
    {
        LyricsTranslationVoteEntity entity = LyricsTranslationVoteEntity.Create(
            id: _id,
            revisionId: _revisionId,
            userId: _userId,
            vote: _vote,
            comment: null
        );

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }
}
