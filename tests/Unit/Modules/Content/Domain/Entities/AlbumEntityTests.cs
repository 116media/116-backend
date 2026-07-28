using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="AlbumEntity"/>.
/// </summary>
public class AlbumEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParams_ShouldCreateStandaloneAlbum()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string name = TestConstants.Content.Editorial.Album.ValidName;
        const short releaseYear = TestConstants.Content.Editorial.Album.ValidReleaseYear;
        const string label = TestConstants.Content.Editorial.Album.ValidLabel;

        // Act
        AlbumEntity album = AlbumEntity.Create(
            id,
            name,
            null,
            null,
            releaseYear,
            label,
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Assert
        album.Id.Should().Be(id);
        album.Name.Should().Be(name);
        album.ArtistId.Should().BeNull();
        album.CoverImageFileId.Should().BeNull();
        album.ReleaseYear.Should().Be(releaseYear);
        album.Label.Should().Be(label);
    }

    [Fact]
    public void Create_WithArtistId_ShouldLinkArtist()
    {
        // Arrange
        var artistId = Guid.NewGuid();

        // Act
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Album.ValidName,
            artistId,
            null,
            null,
            null,
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Assert
        album.ArtistId.Should().Be(artistId);
    }

    [Fact]
    public void Create_WithCoverImageFileId_ShouldSetCoverImageFileId()
    {
        // Arrange
        var coverImageFileId = Guid.NewGuid();

        // Act
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Album.ValidName,
            null,
            coverImageFileId,
            null,
            null,
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Assert
        album.CoverImageFileId.Should().Be(coverImageFileId);
    }

    [Fact]
    public void Create_WithNoOptionalFields_ShouldLeaveThemNull()
    {
        // Act
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Album.ValidName,
            null,
            null,
            null,
            null,
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Assert
        album.ArtistId.Should().BeNull();
        album.CoverImageFileId.Should().BeNull();
        album.ReleaseYear.Should().BeNull();
        album.Label.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Act
        Action act = () =>
            AlbumEntity.Create(
                Guid.NewGuid(),
                invalidName!,
                null,
                null,
                null,
                null,
                EnumReleaseType.Album,
                TestErrorsFactory.CreateAlbumErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidParams_ShouldUpdateAllFieldsIndependently()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();
        Guid newCoverImageFileId = Guid.NewGuid();

        // Act
        album.Update(
            "Updated Name",
            newCoverImageFileId,
            1999,
            "Updated Label",
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Assert
        album.Name.Should().Be("Updated Name");
        album.CoverImageFileId.Should().Be(newCoverImageFileId);
        album.ReleaseYear.Should().Be(1999);
        album.Label.Should().Be("Updated Label");
    }

    [Fact]
    public void Update_WithNullCoverImageFileId_ShouldClearCoverImageFileId_AndLeaveOthersUntouched()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();
        album.Update(
            "Name",
            Guid.NewGuid(),
            1990,
            "Label",
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Act
        album.Update("Name", null, 1990, "Label", EnumReleaseType.Album, TestErrorsFactory.CreateAlbumErrors());

        // Assert
        album.CoverImageFileId.Should().BeNull();
        album.ReleaseYear.Should().Be(1990);
        album.Label.Should().Be("Label");
    }

    [Fact]
    public void Update_WithNullReleaseYear_ShouldClearReleaseYearOnly_AndLeaveOthersUntouched()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();
        Guid coverImageFileId = Guid.NewGuid();
        album.Update(
            "Name",
            coverImageFileId,
            1990,
            "Label",
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Act
        album.Update(
            "Name",
            coverImageFileId,
            null,
            "Label",
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Assert
        album.ReleaseYear.Should().BeNull();
        album.CoverImageFileId.Should().Be(coverImageFileId);
        album.Label.Should().Be("Label");
    }

    [Fact]
    public void Update_WithNullLabel_ShouldClearLabelOnly_AndLeaveOthersUntouched()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();
        Guid coverImageFileId = Guid.NewGuid();
        album.Update(
            "Name",
            coverImageFileId,
            1990,
            "Label",
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Act
        album.Update(
            "Name",
            coverImageFileId,
            1990,
            null,
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Assert
        album.Label.Should().BeNull();
        album.ReleaseYear.Should().Be(1990);
        album.CoverImageFileId.Should().Be(coverImageFileId);
    }

    [Fact]
    public void Update_ShouldNeverExposeArtistIdChange()
    {
        // Arrange — Update has no ArtistId parameter at all; linking is a separate concern.
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Album.ValidName,
            Guid.NewGuid(),
            null,
            null,
            null,
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );
        Guid originalArtistId = album.ArtistId!.Value;

        // Act
        album.Update("Updated Name", null, null, null, EnumReleaseType.Album, TestErrorsFactory.CreateAlbumErrors());

        // Assert
        album.ArtistId.Should().Be(originalArtistId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        AlbumEntity album = CreateAlbum();

        // Act
        Action act = () =>
            album.Update(invalidName!, null, null, null, EnumReleaseType.Album, TestErrorsFactory.CreateAlbumErrors());

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    private static AlbumEntity CreateAlbum()
    {
        return AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Album.ValidName,
            null,
            null,
            TestConstants.Content.Editorial.Album.ValidReleaseYear,
            TestConstants.Content.Editorial.Album.ValidLabel,
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );
    }

    #region ReleaseType Tests

    [Fact]
    public void Create_WithMixtapeReleaseType_ShouldStoreIt()
    {
        // Act
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Album.ValidName,
            null,
            null,
            null,
            null,
            EnumReleaseType.Mixtape,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Assert
        album.ReleaseType.Should().Be(EnumReleaseType.Mixtape);
    }

    [Fact]
    public void Update_ShouldChangeReleaseTypeWithoutTouchingOtherFields()
    {
        // Arrange
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Album.ValidName,
            null,
            null,
            2020,
            "Label",
            EnumReleaseType.Album,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Act
        album.Update(
            album.Name,
            album.CoverImageFileId,
            album.ReleaseYear,
            album.Label,
            EnumReleaseType.EP,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Assert
        album.ReleaseType.Should().Be(EnumReleaseType.EP);
        album.ReleaseYear.Should().Be(2020);
        album.Label.Should().Be("Label");
    }

    [Fact]
    public void Update_WithSameReleaseType_ShouldNotResetIt()
    {
        // Arrange
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Album.ValidName,
            null,
            null,
            null,
            null,
            EnumReleaseType.Mixtape,
            TestErrorsFactory.CreateAlbumErrors()
        );

        // Act — a metadata edit re-supplying the current type must not change it.
        album.Update("New Name", null, null, null, album.ReleaseType, TestErrorsFactory.CreateAlbumErrors());

        // Assert
        album.ReleaseType.Should().Be(EnumReleaseType.Mixtape);
    }

    #endregion
}
