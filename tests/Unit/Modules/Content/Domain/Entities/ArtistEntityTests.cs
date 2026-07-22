using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ArtistEntity"/>.
/// </summary>
public class ArtistEntityTests
{
    #region Create Tests

    [Fact]
    public void Create_WithValidParams_ShouldCreateUnclaimedArtist()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string name = TestConstants.Content.Editorial.Artist.ValidName;
        const string slug = TestConstants.Content.Editorial.Artist.ValidSlug;
        const string bio = TestConstants.Content.Editorial.Artist.ValidBio;

        // Act
        ArtistEntity artist = ArtistEntity.Create(id, name, slug, bio, TestErrorsFactory.CreateArtistErrors());

        // Assert
        artist.Id.Should().Be(id);
        artist.Name.Should().Be(name);
        artist.Slug.Should().Be(slug);
        artist.Bio.Should().Be(bio);
        artist.AvatarFileId.Should().BeNull();
        artist.UserId.Should().BeNull();
        artist.VerifiedAt.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullBio_ShouldCreateArtistWithoutBio()
    {
        // Act
        ArtistEntity artist = ArtistEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Artist.ValidName,
            TestConstants.Content.Editorial.Artist.ValidSlug,
            null,
            TestErrorsFactory.CreateArtistErrors()
        );

        // Assert
        artist.Bio.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Act
        Action act = () =>
            ArtistEntity.Create(
                Guid.NewGuid(),
                invalidName!,
                TestConstants.Content.Editorial.Artist.ValidSlug,
                null,
                TestErrorsFactory.CreateArtistErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptySlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Act
        Action act = () =>
            ArtistEntity.Create(
                Guid.NewGuid(),
                TestConstants.Content.Editorial.Artist.ValidName,
                invalidSlug!,
                null,
                TestErrorsFactory.CreateArtistErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidParams_ShouldUpdateNameAndBio()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();

        // Act
        artist.Update("Updated Name", "Updated Bio", TestErrorsFactory.CreateArtistErrors());

        // Assert
        artist.Name.Should().Be("Updated Name");
        artist.Bio.Should().Be("Updated Bio");
    }

    [Fact]
    public void Update_ShouldNeverTouchSlug()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        string originalSlug = artist.Slug;

        // Act
        artist.Update("Updated Name", "Updated Bio", TestErrorsFactory.CreateArtistErrors());

        // Assert
        artist.Slug.Should().Be(originalSlug);
    }

    [Fact]
    public void Update_WithNullBio_ShouldClearBio()
    {
        // Arrange
        ArtistEntity artist = ArtistEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Artist.ValidName,
            TestConstants.Content.Editorial.Artist.ValidSlug,
            TestConstants.Content.Editorial.Artist.ValidBio,
            TestErrorsFactory.CreateArtistErrors()
        );

        // Act
        artist.Update(artist.Name, null, TestErrorsFactory.CreateArtistErrors());

        // Assert
        artist.Bio.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        ArtistEntity artist = CreateArtist();

        // Act
        Action act = () => artist.Update(invalidName!, "Bio", TestErrorsFactory.CreateArtistErrors());

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region SetAvatarFileId Tests

    [Fact]
    public void SetAvatarFileId_ShouldSetTheFileId()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        Guid fileId = Guid.NewGuid();

        // Act
        artist.SetAvatarFileId(fileId);

        // Assert
        artist.AvatarFileId.Should().Be(fileId);
    }

    [Fact]
    public void SetAvatarFileId_WithNull_ShouldClearTheFileId()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        artist.SetAvatarFileId(Guid.NewGuid());

        // Act
        artist.SetAvatarFileId(null);

        // Assert
        artist.AvatarFileId.Should().BeNull();
    }

    #endregion

    #region ClaimOwnership Tests

    [Fact]
    public void ClaimOwnership_WhenUnclaimed_ShouldSetUserIdAndVerifiedAt()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        Guid userId = Guid.NewGuid();
        DateTimeOffset before = DateTimeOffset.UtcNow;

        // Act
        artist.ClaimOwnership(userId, TestErrorsFactory.CreateArtistErrors());

        // Assert
        artist.UserId.Should().Be(userId);
        artist.VerifiedAt.Should().NotBeNull();
        artist.VerifiedAt!.Value.Should().BeCloseTo(before, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ClaimOwnership_WhenAlreadyClaimed_ShouldThrowConflictException()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        artist.ClaimOwnership(Guid.NewGuid(), TestErrorsFactory.CreateArtistErrors());

        // Act
        Action act = () => artist.ClaimOwnership(Guid.NewGuid(), TestErrorsFactory.CreateArtistErrors());

        // Assert
        act.Should().Throw<ConflictException>();
    }

    [Fact]
    public void ClaimOwnership_WhenAlreadyClaimed_ShouldNotOverwriteOriginalOwner()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        Guid originalOwnerId = Guid.NewGuid();
        artist.ClaimOwnership(originalOwnerId, TestErrorsFactory.CreateArtistErrors());

        // Act
        try
        {
            artist.ClaimOwnership(Guid.NewGuid(), TestErrorsFactory.CreateArtistErrors());
        }
        catch (ConflictException)
        {
            // Expected
        }

        // Assert
        artist.UserId.Should().Be(originalOwnerId);
    }

    #endregion

    private static ArtistEntity CreateArtist()
    {
        return ArtistEntity.Create(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.Artist.ValidName,
            TestConstants.Content.Editorial.Artist.ValidSlug,
            TestConstants.Content.Editorial.Artist.ValidBio,
            TestErrorsFactory.CreateArtistErrors()
        );
    }
}
