using _116.Content.Domain.Enums;

namespace _116.Content.Domain.Entities;

/// <summary>
/// Revision behaviour of <see cref="LyricsTranslationEntity" />. Its state lives in <c>Entities/LyricsTranslationEntity.cs</c>.
/// </summary>
public partial class LyricsTranslationEntity
{
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
