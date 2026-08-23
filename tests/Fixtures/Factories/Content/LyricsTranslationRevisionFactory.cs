using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="LyricsTranslationRevisionBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class LyricsTranslationRevisionFactory
{
    /// <summary>
    /// Creates a pending translation revision proposed against the given translation.
    /// </summary>
    public static LyricsTranslationRevisionEntity Create(Guid translationId) =>
        new LyricsTranslationRevisionBuilder().WithTranslationId(translationId).Build();

    /// <summary>
    /// Creates a pending translation revision proposed by a specific user, with specific text.
    /// </summary>
    public static LyricsTranslationRevisionEntity Create(
        Guid translationId,
        Guid proposedByUserId,
        string proposedText
    ) =>
        new LyricsTranslationRevisionBuilder()
            .WithTranslationId(translationId)
            .WithProposedByUserId(proposedByUserId)
            .WithProposedText(proposedText)
            .Build();

    /// <summary>
    /// Creates a translation revision already accepted by the community vote threshold
    /// (<c>DecidedByUserId == null</c>).
    /// </summary>
    public static LyricsTranslationRevisionEntity CreateAutoAccepted(Guid translationId) =>
        new LyricsTranslationRevisionBuilder().WithTranslationId(translationId).AsAccepted().Build();

    /// <summary>
    /// Creates a translation revision already accepted by a moderator override.
    /// </summary>
    public static LyricsTranslationRevisionEntity CreateAcceptedByModerator(Guid translationId, Guid decidedByUserId) =>
        new LyricsTranslationRevisionBuilder().WithTranslationId(translationId).AsAccepted(decidedByUserId).Build();

    /// <summary>
    /// Creates a translation revision already rejected by a moderator.
    /// </summary>
    public static LyricsTranslationRevisionEntity CreateRejected(Guid translationId, Guid decidedByUserId) =>
        new LyricsTranslationRevisionBuilder().WithTranslationId(translationId).AsRejected(decidedByUserId).Build();
}
