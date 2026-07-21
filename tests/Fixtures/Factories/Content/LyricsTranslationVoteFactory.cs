using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="LyricsTranslationVoteEntity"/> instances in tests.
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

    /// <summary>
    /// Creates several distinct users' approval votes on the given translation revision.
    /// </summary>
    public static List<LyricsTranslationVoteEntity> CreateManyApprovals(Guid revisionId, int count) =>
        Enumerable.Range(0, count).Select(_ => CreateApprove(revisionId)).ToList();
}
