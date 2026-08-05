using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="LyricsRevisionVoteEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; LyricsRevisionVoteFactory only names chains three or more tests share.
/// </summary>
public class LyricsRevisionVoteBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _revisionId = Guid.NewGuid();
    private Guid _userId = Guid.NewGuid();
    private EnumVote _vote = EnumVote.Approve;
    private string? _comment;

    /// <summary>
    /// Sets the vote ID.
    /// </summary>
    public LyricsRevisionVoteBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    /// <summary>
    /// Sets the lyrics revision being voted on.
    /// </summary>
    public LyricsRevisionVoteBuilder WithRevisionId(Guid revisionId)
    {
        _revisionId = revisionId;
        return this;
    }

    /// <summary>
    /// Sets the identity user UUID of the voter.
    /// </summary>
    public LyricsRevisionVoteBuilder WithUserId(Guid userId)
    {
        _userId = userId;
        return this;
    }

    /// <summary>
    /// Sets the cast vote direction.
    /// </summary>
    public LyricsRevisionVoteBuilder WithVote(EnumVote vote)
    {
        _vote = vote;
        return this;
    }

    /// <summary>
    /// Sets the optional free-text comment.
    /// </summary>
    public LyricsRevisionVoteBuilder WithComment(string? comment)
    {
        _comment = comment;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="LyricsRevisionVoteEntity"/> instance.
    /// </summary>
    public LyricsRevisionVoteEntity Build()
    {
        LyricsRevisionVoteEntity entity = LyricsRevisionVoteEntity.Create(
            id: _id,
            revisionId: _revisionId,
            userId: _userId,
            vote: _vote,
            comment: _comment
        );

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }
}
