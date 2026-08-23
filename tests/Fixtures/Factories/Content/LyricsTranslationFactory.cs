using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Builders.Entities.Content;

namespace _116.Tests.Fixtures.Factories.Content;

/// <summary>
/// Named aliases for <see cref="LyricsTranslationBuilder" /> chains that three or more tests share verbatim.
/// A shape fewer tests need belongs at the call site as a builder chain, not here —
/// factory names carry the combinatorics, and combinatorics multiply.
/// </summary>
public static class LyricsTranslationFactory
{
    /// <summary>
    /// Creates an AI-sourced translation of the given lyrics page into the given language.
    /// </summary>
    public static LyricsTranslationEntity Create(Guid lyricsId, string language = "es") =>
        new LyricsTranslationBuilder().WithLyricsId(lyricsId).WithLanguage(language).Build();

    /// <summary>
    /// Creates a translation with specific translated text.
    /// </summary>
    public static LyricsTranslationEntity CreateWithText(Guid lyricsId, string language, string text) =>
        new LyricsTranslationBuilder().WithLyricsId(lyricsId).WithLanguage(language).WithText(text).Build();
}
