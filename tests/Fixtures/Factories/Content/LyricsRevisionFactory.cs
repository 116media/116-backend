using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="LyricsRevisionBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class LyricsRevisionFactory
{
    /// <summary>
    /// Creates a pending lyrics-text correction revision proposed against the given lyrics page.
    /// </summary>
    public static LyricsRevisionEntity Create(Guid lyricsId) =>
        new LyricsRevisionBuilder().WithLyricsId(lyricsId).Build();

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
}
