using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="LyricsRevisionVoteEntity"/> instances in tests.
/// </summary>
public static class LyricsRevisionVoteFactory
{
    /// <summary>
    /// Creates an approval vote on the given lyrics-text correction revision.
    /// </summary>
    public static LyricsRevisionVoteEntity CreateApprove(Guid revisionId, Guid? userId = null) =>
        new LyricsRevisionVoteBuilder()
            .WithRevisionId(revisionId)
            .WithUserId(userId ?? Guid.NewGuid())
            .WithVote(EnumVote.Approve)
            .Build();

    /// <summary>
    /// Creates a rejection vote on the given lyrics-text correction revision.
    /// </summary>
    public static LyricsRevisionVoteEntity CreateReject(Guid revisionId, Guid? userId = null) =>
        new LyricsRevisionVoteBuilder()
            .WithRevisionId(revisionId)
            .WithUserId(userId ?? Guid.NewGuid())
            .WithVote(EnumVote.Reject)
            .Build();

    /// <summary>
    /// Creates several distinct users' approval votes on the given lyrics-text revision.
    /// </summary>
    public static List<LyricsRevisionVoteEntity> CreateManyApprovals(Guid revisionId, int count) =>
        Enumerable.Range(0, count).Select(_ => CreateApprove(revisionId)).ToList();
}
