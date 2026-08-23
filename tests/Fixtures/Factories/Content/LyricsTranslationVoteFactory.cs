using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="LyricsTranslationVoteBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class LyricsTranslationVoteFactory
{
    /// <summary>
    /// Creates an approval vote on the given translation revision.
    /// </summary>
    public static LyricsTranslationVoteEntity CreateApprove(Guid revisionId, Guid? userId = null) =>
        new LyricsTranslationVoteBuilder()
            .WithRevisionId(revisionId)
            .WithUserId(userId ?? Guid.NewGuid())
            .WithVote(EnumVote.Approve)
            .Build();

    /// <summary>
    /// Creates a rejection vote on the given translation revision.
    /// </summary>
    public static LyricsTranslationVoteEntity CreateReject(Guid revisionId, Guid? userId = null) =>
        new LyricsTranslationVoteBuilder()
            .WithRevisionId(revisionId)
            .WithUserId(userId ?? Guid.NewGuid())
            .WithVote(EnumVote.Reject)
            .Build();
}
