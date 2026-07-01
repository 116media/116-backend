using _116.Shared.Application.Exceptions.Messages;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Shared.Exceptions.Messages;

/// <summary>
/// Unit tests for <see cref="SharedExceptionMessage"/>, pinning the friendly,
/// localized not-found labels resolved from the embedded .resx resources.
/// </summary>
public class SharedExceptionMessageTests
{
    private readonly SharedExceptionMessage _message = LocalizerFactory.CreateMessage<SharedExceptionMessage>();

    #region EntityNotFound — English

    [Theory]
    [InlineData("User", "the requested user account")]
    [InlineData("Session", "the requested session")]
    [InlineData("Role", "the requested role")]
    [InlineData("Article", "the requested article")]
    [InlineData("Video", "the requested video")]
    [InlineData("Lyrics", "the requested lyrics")]
    [InlineData("Category", "the requested category")]
    public void EntityNotFound_InEnglish_ReturnsFriendlyLabel(string entity, string label)
    {
        // Arrange
        using var scope = new CultureScope("en");

        // Act
        string message = _message.EntityNotFound(entity);

        // Assert
        message.Should().Be($"Could not find {label}.");
    }

    [Fact]
    public void EntityNotFound_InEnglish_WithUnmappedEntity_FallsBackToGenericLabel()
    {
        // Arrange
        using var scope = new CultureScope("en");

        // Act
        string message = _message.EntityNotFound("TotallyUnmappedThing");

        // Assert — no specific label, so the generic one is used, name not leaked
        message.Should().Be("Could not find the requested resource.");
        message.Should().NotContain("TotallyUnmappedThing");
    }

    #endregion

    #region EntityNotFound — French

    [Theory]
    [InlineData("User", "le compte utilisateur demandé")]
    [InlineData("Session", "la session demandée")]
    [InlineData("Role", "le rôle demandé")]
    [InlineData("Article", "l'article demandé")]
    [InlineData("Video", "la vidéo demandée")]
    [InlineData("Lyrics", "les paroles demandées")]
    [InlineData("Category", "la catégorie demandée")]
    public void EntityNotFound_InFrench_ReturnsFriendlyLabel(string entity, string label)
    {
        // Arrange
        using var scope = new CultureScope("fr");

        // Act
        string message = _message.EntityNotFound(entity);

        // Assert
        message.Should().Be($"Impossible de trouver {label}.");
    }

    [Fact]
    public void EntityNotFound_InFrench_WithUnmappedEntity_FallsBackToGenericLabel()
    {
        // Arrange
        using var scope = new CultureScope("fr");

        // Act
        string message = _message.EntityNotFound("TotallyUnmappedThing");

        // Assert
        message.Should().Be("Impossible de trouver la ressource demandée.");
        message.Should().NotContain("TotallyUnmappedThing");
    }

    #endregion

    #region EntityNotFound — localization contract

    [Fact]
    public void EntityNotFound_ForSameEntity_DiffersBetweenEnglishAndFrench()
    {
        // Arrange
        string en;
        string fr;

        // Act
        using (var _ = new CultureScope("en"))
        {
            en = _message.EntityNotFound("User");
        }

        using (var _ = new CultureScope("fr"))
        {
            fr = _message.EntityNotFound("User");
        }

        // Assert
        en.Should().NotBe(fr);
    }

    #endregion
}
