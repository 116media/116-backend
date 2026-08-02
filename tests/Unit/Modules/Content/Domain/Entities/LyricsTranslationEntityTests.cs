using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsTranslationEntity"/>.
/// </summary>
public class LyricsTranslationEntityTests
{
    #region CreateAi Tests

    [Fact]
    public void CreateAi_WithValidParams_ShouldAssignAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lyricsId = Guid.NewGuid();
        const string language = "es";
        const string text = "Texto traducido.";

        // Act
        LyricsTranslationEntity translation = LyricsTranslationEntity.CreateAi(id, lyricsId, language, text);

        // Assert
        translation.Id.Should().Be(id);
        translation.LyricsId.Should().Be(lyricsId);
        translation.Language.Should().Be(language);
        translation.Text.Should().Be(text);
    }

    [Fact]
    public void CreateAi_ShouldSetSourceToAi()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lyricsId = Guid.NewGuid();

        // Act
        LyricsTranslationEntity translation = LyricsTranslationEntity.CreateAi(id, lyricsId, "es", "Texto traducido.");

        // Assert
        translation.Source.Should().Be(EnumTranslationSource.Ai);
    }

    #endregion

    #region ApplyAcceptedRevision Tests

    [Fact]
    public void ApplyAcceptedRevision_ShouldReplaceText()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationEntity.CreateAi(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "es",
            "Texto original."
        );

        // Act
        translation.ApplyAcceptedRevision("Texto corregido por la comunidad.");

        // Assert
        translation.Text.Should().Be("Texto corregido por la comunidad.");
    }

    [Fact]
    public void ApplyAcceptedRevision_ShouldSetSourceToCommunity()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationEntity.CreateAi(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "es",
            "Texto original."
        );

        // Act
        translation.ApplyAcceptedRevision("Texto corregido por la comunidad.");

        // Assert
        translation.Source.Should().Be(EnumTranslationSource.Community);
    }

    [Fact]
    public void ApplyAcceptedRevision_ShouldNotTouchLyricsIdOrLanguage()
    {
        // Arrange
        var lyricsId = Guid.NewGuid();
        LyricsTranslationEntity translation = LyricsTranslationEntity.CreateAi(
            Guid.NewGuid(),
            lyricsId,
            "es",
            "Texto original."
        );

        // Act
        translation.ApplyAcceptedRevision("Texto corregido por la comunidad.");

        // Assert
        translation.LyricsId.Should().Be(lyricsId);
        translation.Language.Should().Be("es");
    }

    #endregion
}
