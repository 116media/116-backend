using _116.Content.Domain.Entities;

namespace _116.Tests.Fixtures.Builders.Entities.Content;

/// <summary>
/// Fluent builder for creating <see cref="LyricsTranslationEntity" /> instances in tests.
/// Drives the real domain transitions, so every state it produces is one the application can reach.
/// Use it for any shape a test needs; LyricsTranslationFactory only names chains three or more tests share.
/// </summary>
public class LyricsTranslationBuilder
{
    private Guid _id = Guid.NewGuid();
    private Guid _lyricsId = Guid.NewGuid();
    private string _language = "es";
    private string _text = "Texto traducido de la letra.";
    private string? _acceptedRevisionText;

    /// <summary>
    /// Sets the lyrics page this translation belongs to.
    /// </summary>
    public LyricsTranslationBuilder WithLyricsId(Guid lyricsId)
    {
        _lyricsId = lyricsId;
        return this;
    }

    /// <summary>
    /// Sets the ISO 639-1 language code.
    /// </summary>
    public LyricsTranslationBuilder WithLanguage(string language)
    {
        _language = language;
        return this;
    }

    /// <summary>
    /// Sets the translated text.
    /// </summary>
    public LyricsTranslationBuilder WithText(string text)
    {
        _text = text;
        return this;
    }

    /// <summary>
    /// Builds the <see cref="LyricsTranslationEntity"/> instance.
    /// </summary>
    public LyricsTranslationEntity Build()
    {
        LyricsTranslationEntity entity = LyricsTranslationEntity.CreateAi(
            id: _id,
            lyricsId: _lyricsId,
            language: _language,
            text: _text
        );

        if (_acceptedRevisionText is not null)
        {
            entity.ApplyAcceptedRevision(_acceptedRevisionText);
        }

        entity.CreatedAt = DateTime.UtcNow;

        return entity;
    }
}
