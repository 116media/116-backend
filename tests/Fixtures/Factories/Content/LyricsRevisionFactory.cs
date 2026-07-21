using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="LyricsRevisionEntity"/> instances in tests.
/// </summary>
public static class LyricsRevisionFactory
{
    /// <summary>
    /// Creates a pending lyrics-text correction revision proposed against the given lyrics page.
    /// </summary>
    public static LyricsRevisionEntity Create(Guid lyricsId) =>
        new LyricsRevisionBuilder().WithLyricsId(lyricsId).Build();

    /// <summary>
    /// Creates a pending lyrics-text correction revision with a specific ID.
    /// </summary>
    public static LyricsRevisionEntity CreateWithId(Guid id, Guid lyricsId) =>
        new LyricsRevisionBuilder().WithId(id).WithLyricsId(lyricsId).Build();

    /// <summary>
    /// Creates a pending lyrics-text correction revision proposed by a specific user, with
    /// specific text.
    /// </summary>
    public static LyricsRevisionEntity Create(Guid lyricsId, Guid proposedByUserId, string proposedText) =>
        new LyricsRevisionBuilder()
            .WithLyricsId(lyricsId)
            .WithProposedByUserId(proposedByUserId)
            .WithProposedText(proposedText)
            .Build();

    /// <summary>
    /// Creates a lyrics-text revision already accepted by the community vote threshold
    /// (<c>DecidedByUserId == null</c>).
    /// </summary>
    public static LyricsRevisionEntity CreateAutoAccepted(Guid lyricsId) =>
        new LyricsRevisionBuilder().WithLyricsId(lyricsId).AsAccepted().Build();

    /// <summary>
    /// Creates a lyrics-text revision already accepted by a moderator override.
    /// </summary>
    public static LyricsRevisionEntity CreateAcceptedByModerator(Guid lyricsId, Guid decidedByUserId) =>
        new LyricsRevisionBuilder().WithLyricsId(lyricsId).AsAccepted(decidedByUserId).Build();

    /// <summary>
    /// Creates a lyrics-text revision already rejected by a moderator.
    /// </summary>
    public static LyricsRevisionEntity CreateRejected(Guid lyricsId, Guid decidedByUserId) =>
        new LyricsRevisionBuilder().WithLyricsId(lyricsId).AsRejected(decidedByUserId).Build();
}
