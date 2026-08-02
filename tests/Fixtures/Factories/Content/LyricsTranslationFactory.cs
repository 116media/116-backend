using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Factory for quickly creating <see cref="LyricsTranslationEntity"/> instances in tests.
/// </summary>
public static class LyricsTranslationFactory
{
    /// <summary>
    /// Creates an AI-sourced translation of the given lyrics page into the given language.
    /// </summary>
    public static LyricsTranslationEntity Create(Guid lyricsId, string language = "es") =>
        new LyricsTranslationBuilder().WithLyricsId(lyricsId).WithLanguage(language).Build();

    /// <summary>
    /// Creates an AI-sourced translation with a specific ID.
    /// </summary>
    public static LyricsTranslationEntity CreateWithId(Guid id, Guid lyricsId, string language = "es") =>
        new LyricsTranslationBuilder().WithId(id).WithLyricsId(lyricsId).WithLanguage(language).Build();

    /// <summary>
    /// Creates a translation with specific translated text.
    /// </summary>
    public static LyricsTranslationEntity CreateWithText(Guid lyricsId, string language, string text) =>
        new LyricsTranslationBuilder().WithLyricsId(lyricsId).WithLanguage(language).WithText(text).Build();

    /// <summary>
    /// Creates a translation whose current text and source reflect an already-accepted
    /// community revision (<c>Source == Community</c>).
    /// </summary>
    public static LyricsTranslationEntity CreateWithAcceptedRevision(
        Guid lyricsId,
        string language,
        string acceptedText
    ) =>
        new LyricsTranslationBuilder()
            .WithLyricsId(lyricsId)
            .WithLanguage(language)
            .WithAcceptedRevision(acceptedText)
            .Build();
}
