using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Builders.Entities.Content;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for album specification classes.
/// Specifications using EF.Functions.ILike are evaluated through
/// <see cref="ILikeSpecificationEvaluator" />, which rewrites ILike for in-memory execution.
/// </summary>
public class AlbumSpecificationsTests
{
    #region AlbumByIdSpecification

    [Fact]
    public void AlbumByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        var spec = new AlbumByIdSpecification(album.Id);

        // Act
        bool result = spec.IsSatisfiedBy(album);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void AlbumByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.Create();
        var spec = new AlbumByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(album);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AlbumSearchSpecification

    [Theory]
    [InlineData("control", true)]
    [InlineData("CONTROL", true)]
    [InlineData("obouo", true)]
    [InlineData("tokooos", false)]
    public void AlbumSearchSpecification_ShouldMatchNameOrLabelCaseInsensitively(string search, bool expected)
    {
        // Arrange
        AlbumEntity album = new AlbumBuilder().WithName("Control").WithLabel("Obouo Music").Build();
        var spec = new AlbumSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(album);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void AlbumSearchSpecification_WithNullLabel_ShouldNotMatchLabelTerm()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.CreateWithName("Control");
        var spec = new AlbumSearchSpecification("obouo");

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(album);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region AlbumByArtistSpecification

    [Fact]
    public void AlbumByArtistSpecification_WithMatchingArtist_ShouldReturnTrue()
    {
        // Arrange
        var artistId = Guid.NewGuid();
        AlbumEntity album = AlbumFactory.CreateForArtist(artistId);
        var spec = new AlbumByArtistSpecification(artistId);

        // Act & Assert
        spec.IsSatisfiedBy(album).Should().BeTrue();
    }

    [Fact]
    public void AlbumByArtistSpecification_WithDifferentArtist_ShouldReturnFalse()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.CreateForArtist(Guid.NewGuid());
        var spec = new AlbumByArtistSpecification(Guid.NewGuid());

        // Act & Assert
        spec.IsSatisfiedBy(album).Should().BeFalse();
    }

    [Fact]
    public void AlbumByArtistSpecification_WithUnlinkedAlbum_ShouldReturnFalse()
    {
        // Arrange — ArtistId is nullable; an unlinked album matches no artist scope.
        AlbumEntity album = AlbumFactory.Create();
        var spec = new AlbumByArtistSpecification(Guid.NewGuid());

        // Act & Assert
        spec.IsSatisfiedBy(album).Should().BeFalse();
    }

    #endregion

    #region AlbumByReleaseTypeSpecification

    [Fact]
    public void AlbumByReleaseTypeSpecification_WithMatchingType_ShouldReturnTrue()
    {
        // Arrange
        AlbumEntity mixtape = AlbumFactory.CreateForArtist(Guid.NewGuid(), EnumReleaseType.Mixtape);
        var spec = new AlbumByReleaseTypeSpecification(EnumReleaseType.Mixtape);

        // Act & Assert
        spec.IsSatisfiedBy(mixtape).Should().BeTrue();
    }

    [Fact]
    public void AlbumByReleaseTypeSpecification_WithDifferentType_ShouldReturnFalse()
    {
        // Arrange
        AlbumEntity album = AlbumFactory.CreateForArtist(Guid.NewGuid(), EnumReleaseType.Album);
        var spec = new AlbumByReleaseTypeSpecification(EnumReleaseType.Mixtape);

        // Act & Assert
        spec.IsSatisfiedBy(album).Should().BeFalse();
    }

    #endregion
}
