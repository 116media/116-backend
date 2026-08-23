using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="LyricsRevisionVoteBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
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
}
