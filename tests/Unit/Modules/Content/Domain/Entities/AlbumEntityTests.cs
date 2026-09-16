using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
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
        const string name = TestConstants.Album.ValidName;
        const short releaseYear = TestConstants.Album.ValidReleaseYear;
        const string label = TestConstants.Album.ValidLabel;

        // Act
        AlbumEntity album = AlbumEntity.Create(id, name, null, null, releaseYear, label, EnumReleaseType.Album);

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
            TestConstants.Album.ValidName,
            artistId,
            null,
            null,
            null,
            EnumReleaseType.Album
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
            TestConstants.Album.ValidName,
            null,
            coverImageFileId,
            null,
            null,
            EnumReleaseType.Album
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
            TestConstants.Album.ValidName,
            null,
            null,
            null,
            null,
            EnumReleaseType.Album
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
            AlbumEntity.Create(Guid.NewGuid(), invalidName!, null, null, null, null, EnumReleaseType.Album);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.AlbumNameRequired);
    }

    #endregion

    #region Rename Tests

    [Fact]
    public void Rename_WithNewName_ShouldSetItAndReportChanged()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();

        // Act
        bool changed = album.Rename("Updated Name");

        // Assert
        changed.Should().BeTrue();
        album.Name.Should().Be("Updated Name");
    }

    [Fact]
    public void Rename_WithSameName_ShouldReportUnchangedAndRaiseNothing()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();
        album.ClearDomainEvents();

        // Act
        bool changed = album.Rename(album.Name);

        // Assert
        changed.Should().BeFalse();
        album.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region SetCoverImage Tests

    [Fact]
    public void SetCoverImage_WithNewFileId_ShouldSetItAndLeaveOtherFieldsUntouched()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();
        album.ReviseRelease(1990, "Label", EnumReleaseType.Album);
        Guid newCoverImageFileId = Guid.NewGuid();

        // Act
        bool changed = album.SetCoverImage(newCoverImageFileId);

        // Assert
        changed.Should().BeTrue();
        album.CoverImageFileId.Should().Be(newCoverImageFileId);
        album.ReleaseYear.Should().Be(1990);
        album.Label.Should().Be("Label");
    }

    [Fact]
    public void SetCoverImage_WithNull_ShouldClearCoverImageFileId_AndLeaveOthersUntouched()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();
        album.ReviseRelease(1990, "Label", EnumReleaseType.Album);
        album.SetCoverImage(Guid.NewGuid());

        // Act
        bool changed = album.SetCoverImage(null);

        // Assert
        changed.Should().BeTrue();
        album.CoverImageFileId.Should().BeNull();
        album.ReleaseYear.Should().Be(1990);
        album.Label.Should().Be("Label");
    }

    [Fact]
    public void SetCoverImage_WithTheSameFileId_ShouldReportUnchanged()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();
        Guid coverImageFileId = Guid.NewGuid();
        album.SetCoverImage(coverImageFileId);

        // Act
        bool changed = album.SetCoverImage(coverImageFileId);

        // Assert
        changed.Should().BeFalse();
    }

    #endregion

    #region ReviseRelease Tests

    [Fact]
    public void ReviseRelease_WithNullReleaseYear_ShouldClearReleaseYearOnly_AndLeaveOthersUntouched()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();
        Guid coverImageFileId = Guid.NewGuid();
        album.SetCoverImage(coverImageFileId);
        album.ReviseRelease(1990, "Label", EnumReleaseType.Album);

        // Act
        album.ReviseRelease(null, "Label", EnumReleaseType.Album);

        // Assert
        album.ReleaseYear.Should().BeNull();
        album.CoverImageFileId.Should().Be(coverImageFileId);
        album.Label.Should().Be("Label");
    }

    [Fact]
    public void ReviseRelease_WithNullLabel_ShouldClearLabelOnly_AndLeaveOthersUntouched()
    {
        // Arrange
        AlbumEntity album = CreateAlbum();
        Guid coverImageFileId = Guid.NewGuid();
        album.SetCoverImage(coverImageFileId);
        album.ReviseRelease(1990, "Label", EnumReleaseType.Album);

        // Act
        album.ReviseRelease(1990, null, EnumReleaseType.Album);

        // Assert
        album.Label.Should().BeNull();
        album.ReleaseYear.Should().Be(1990);
        album.CoverImageFileId.Should().Be(coverImageFileId);
    }

    [Fact]
    public void Rename_ShouldNeverExposeArtistIdChange()
    {
        // Arrange
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Album.ValidName,
            Guid.NewGuid(),
            null,
            null,
            null,
            EnumReleaseType.Album
        );
        Guid originalArtistId = album.ArtistId!.Value;

        // Act
        album.Rename("Updated Name");

        // Assert
        album.ArtistId.Should().Be(originalArtistId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithEmptyName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        AlbumEntity album = CreateAlbum();

        // Act
        Action act = () => album.Rename(invalidName!);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.AlbumNameRequired);
    }

    #endregion

    private static AlbumEntity CreateAlbum()
    {
        return AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Album.ValidName,
            null,
            null,
            TestConstants.Album.ValidReleaseYear,
            TestConstants.Album.ValidLabel,
            EnumReleaseType.Album
        );
    }

    #region ReleaseType Tests

    [Fact]
    public void Create_WithMixtapeReleaseType_ShouldStoreIt()
    {
        // Act
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Album.ValidName,
            null,
            null,
            null,
            null,
            EnumReleaseType.Mixtape
        );

        // Assert
        album.ReleaseType.Should().Be(EnumReleaseType.Mixtape);
    }

    [Fact]
    public void ReviseRelease_ShouldChangeReleaseTypeWithoutTouchingOtherFields()
    {
        // Arrange
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Album.ValidName,
            null,
            null,
            2020,
            "Label",
            EnumReleaseType.Album
        );

        // Act
        album.ReviseRelease(album.ReleaseYear, album.Label, EnumReleaseType.EP);

        // Assert
        album.ReleaseType.Should().Be(EnumReleaseType.EP);
        album.ReleaseYear.Should().Be(2020);
        album.Label.Should().Be("Label");
    }

    [Fact]
    public void ReviseRelease_WithSameReleaseType_ShouldNotResetIt()
    {
        // Arrange
        AlbumEntity album = AlbumEntity.Create(
            Guid.NewGuid(),
            TestConstants.Album.ValidName,
            null,
            null,
            null,
            null,
            EnumReleaseType.Mixtape
        );

        // Act — a metadata edit re-supplying the current type must not change it.
        album.ReviseRelease(null, null, album.ReleaseType);

        // Assert
        album.ReleaseType.Should().Be(EnumReleaseType.Mixtape);
    }

    #endregion
}
