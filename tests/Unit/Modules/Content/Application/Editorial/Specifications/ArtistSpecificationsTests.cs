using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for artist specification classes.
/// Specifications using EF.Functions.ILike are evaluated through
/// <see cref="ILikeSpecificationEvaluator" />, which rewrites ILike for in-memory execution.
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

    [Theory]
    [InlineData("fally-ipupa", true)]
    [InlineData("FALLY-IPUPA", true)]
    [InlineData("fally", false)]
    [InlineData("koffi-olomide", false)]
    public void ArtistBySlugSpecification_ShouldMatchWholeSlugCaseInsensitively(string slug, bool expected)
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.CreateWithSlug("fally-ipupa");
        var spec = new ArtistBySlugSpecification(slug);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(artist);

        // Assert
        result.Should().Be(expected);
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

    [Theory]
    [InlineData("fally", true)]
    [InlineData("FALLY IPUPA", true)]
    [InlineData("rumba", true)]
    [InlineData("koffi", false)]
    public void ArtistSearchSpecification_ShouldMatchNameOrBioCaseInsensitively(string search, bool expected)
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create("Fally Ipupa", "fally-ipupa", "Icone de la rumba congolaise");
        var spec = new ArtistSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(artist);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void ArtistSearchSpecification_WithNullBio_ShouldNotMatchBioTerm()
    {
        // Arrange
        ArtistEntity artist = ArtistFactory.Create("Fally Ipupa", "fally-ipupa", bio: null);
        var spec = new ArtistSearchSpecification("rumba");

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(artist);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
