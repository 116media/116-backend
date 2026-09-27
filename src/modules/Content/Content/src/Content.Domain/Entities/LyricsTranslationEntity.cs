using _116.Content.Domain.Enums;
using _116.Shared.Domain;

namespace _116.Content.Domain.Entities;

/// <summary>
/// A published translation of a lyrics page into a given language. One row per
/// <c>(LyricsId, Language)</c> pair — corrections update this row's text via an accepted
/// <see cref="LyricsTranslationRevisionEntity" />, they never create a second row for the same
/// language.
/// </summary>
public class LyricsTranslationEntity : Aggregate<Guid>
{
    /// <summary>
    /// The lyrics page this translation belongs to.
    /// </summary>
    public Guid LyricsId { get; private set; }

    /// <summary>
    /// ISO 639-1 language code this translation is written in.
    /// </summary>
    public string Language { get; private set; } = null!;

    /// <summary>
    /// The current published translated text.
    /// </summary>
    public string Text { get; private set; } = null!;

    /// <summary>
    /// Where the current <see cref="Text" /> came from — the original AI generation, or a since
    /// accepted community revision.
    /// </summary>
    public EnumTranslationSource Source { get; private set; }

    private LyricsTranslationEntity() { }

    /// <summary>
    /// Creates a new translation from an AI generation call.
    /// </summary>
    /// <param name="id">The unique identifier for this translation.</param>
    /// <param name="lyricsId">The lyrics page being translated.</param>
    /// <param name="language">ISO 639-1 language code of the translation.</param>
    /// <param name="text">The AI-generated translated text.</param>
    /// <returns>A new <see cref="LyricsTranslationEntity" /> with <see cref="EnumTranslationSource.Ai" />.</returns>
    public static LyricsTranslationEntity CreateAi(Guid id, Guid lyricsId, string language, string text)
    {
        return new LyricsTranslationEntity
        {
            Id = id,
            LyricsId = lyricsId,
            Language = language,
            Text = text,
            Source = EnumTranslationSource.Ai,
        };
    }

    /// <summary>
    /// Applies an accepted community revision's text as the new published translation.
    /// </summary>
    /// <param name="newText">The proposed text of the accepted revision.</param>
    public void ApplyAcceptedRevision(string newText)
    {
        Text = newText;
        Source = EnumTranslationSource.Community;
    }
}
