using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for lyrics translation, translation revision, and translation vote specification
/// classes.
/// </summary>
public class TranslationSpecificationsTests
{
    #region TranslationByIdSpecification

    [Fact]
    public void TranslationByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(Guid.NewGuid());
        var spec = new TranslationByIdSpecification(translation.Id);

        // Act
        bool result = spec.IsSatisfiedBy(translation);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TranslationByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(Guid.NewGuid());
        var spec = new TranslationByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(translation);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region TranslationByLyricsAndLanguageSpecification

    [Fact]
    public void TranslationByLyricsAndLanguageSpecification_WithMatchingLyricsAndLanguage_ShouldReturnTrue()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(lyricsId, "es");
        var spec = new TranslationByLyricsAndLanguageSpecification(lyricsId, "es");

        // Act
        bool result = spec.IsSatisfiedBy(translation);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TranslationByLyricsAndLanguageSpecification_WithDifferentLanguage_ShouldReturnFalse()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(lyricsId, "es");
        var spec = new TranslationByLyricsAndLanguageSpecification(lyricsId, "fr");

        // Act
        bool result = spec.IsSatisfiedBy(translation);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region TranslationByLyricsIdSpecification

    [Fact]
    public void TranslationByLyricsIdSpecification_WithMatchingLyricsId_ShouldReturnTrue()
    {
        // Arrange
        Guid lyricsId = Guid.NewGuid();
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(lyricsId);
        var spec = new TranslationByLyricsIdSpecification(lyricsId);

        // Act
        bool result = spec.IsSatisfiedBy(translation);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TranslationByLyricsIdSpecification_WithDifferentLyricsId_ShouldReturnFalse()
    {
        // Arrange
        LyricsTranslationEntity translation = LyricsTranslationFactory.Create(Guid.NewGuid());
        var spec = new TranslationByLyricsIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(translation);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region TranslationRevisionByIdSpecification

    [Fact]
    public void TranslationRevisionByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        var spec = new TranslationRevisionByIdSpecification(revision.Id);

        // Act
        bool result = spec.IsSatisfiedBy(revision);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TranslationRevisionByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        var spec = new TranslationRevisionByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(revision);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region TranslationRevisionByTranslationIdSpecification

    [Fact]
    public void TranslationRevisionByTranslationIdSpecification_WithMatchingTranslationId_ShouldReturnTrue()
    {
        // Arrange
        Guid translationId = Guid.NewGuid();
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(translationId);
        var spec = new TranslationRevisionByTranslationIdSpecification(translationId);

        // Act
        bool result = spec.IsSatisfiedBy(revision);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TranslationRevisionByTranslationIdSpecification_WithDifferentTranslationId_ShouldReturnFalse()
    {
        // Arrange
        LyricsTranslationRevisionEntity revision = LyricsTranslationRevisionFactory.Create(Guid.NewGuid());
        var spec = new TranslationRevisionByTranslationIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(revision);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region TranslationVoteByRevisionAndUserSpecification

    [Fact]
    public void TranslationVoteByRevisionAndUserSpecification_WithMatchingRevisionAndUser_ShouldReturnTrue()
    {
        // Arrange
        Guid revisionId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        LyricsTranslationVoteEntity vote = LyricsTranslationVoteFactory.CreateApprove(revisionId, userId);
        var spec = new TranslationVoteByRevisionAndUserSpecification(revisionId, userId);

        // Act
        bool result = spec.IsSatisfiedBy(vote);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TranslationVoteByRevisionAndUserSpecification_WithDifferentUser_ShouldReturnFalse()
    {
        // Arrange
        Guid revisionId = Guid.NewGuid();
        LyricsTranslationVoteEntity vote = LyricsTranslationVoteFactory.CreateApprove(revisionId, Guid.NewGuid());
        var spec = new TranslationVoteByRevisionAndUserSpecification(revisionId, Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(vote);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region TranslationVoteByRevisionIdSpecification

    [Fact]
    public void TranslationVoteByRevisionIdSpecification_WithMatchingRevisionId_ShouldReturnTrue()
    {
        // Arrange
        Guid revisionId = Guid.NewGuid();
        LyricsTranslationVoteEntity vote = LyricsTranslationVoteFactory.CreateReject(revisionId);
        var spec = new TranslationVoteByRevisionIdSpecification(revisionId);

        // Act
        bool result = spec.IsSatisfiedBy(vote);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TranslationVoteByRevisionIdSpecification_WithDifferentRevisionId_ShouldReturnFalse()
    {
        // Arrange
        LyricsTranslationVoteEntity vote = LyricsTranslationVoteFactory.CreateReject(Guid.NewGuid());
        var spec = new TranslationVoteByRevisionIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(vote);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
