using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for album specification classes.
/// Note: Specifications using EF.Functions.ILike require a real PostgreSQL provider —
/// those are covered via ToExpression().Compile() only.
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

    // ILike: requires PostgreSQL provider — compile-only
    [Fact]
    public void AlbumSearchSpecification_ShouldCompileExpression()
    {
        // Arrange
        var spec = new AlbumSearchSpecification("control");

        // Act
        Func<AlbumEntity, bool> predicate = spec.ToExpression().Compile();

        // Assert
        predicate.Should().NotBeNull();
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
