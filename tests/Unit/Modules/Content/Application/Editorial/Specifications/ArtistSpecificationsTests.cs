using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for artist specification classes.
/// Note: Specifications using EF.Functions.ILike require a real PostgreSQL provider —
/// those are covered via ToExpression().Compile() only.
/// </summary>
public class ArtistSpecificationsTests
{
    #region ArtistByIdSpecification

    [Fact]
    public void ArtistByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        var spec = new ArtistByIdSpecification(artist.Id);

        // Act
        bool result = spec.IsSatisfiedBy(artist);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArtistByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        var spec = new ArtistByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(artist);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ArtistBySlugSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void ArtistBySlugSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new ArtistBySlugSpecification("fally-ipupa");

        // Act
        Func<ArtistEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion

    #region ArtistByUserIdSpecification

    [Fact]
    public void ArtistByUserIdSpecification_WithMatchingUserId_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        ArtistEntity artist = ArtistFactory.CreateClaimed(userId);
        var spec = new ArtistByUserIdSpecification(userId);

        // Act
        bool result = spec.IsSatisfiedBy(artist);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ArtistByUserIdSpecification_WithUnclaimedArtist_ShouldReturnFalse()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create();
        var spec = new ArtistByUserIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(artist);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ArtistSearchSpecification

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void ArtistSearchSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new ArtistSearchSpecification("fally");

        // Act
        Func<ArtistEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
    }

    #endregion
}
