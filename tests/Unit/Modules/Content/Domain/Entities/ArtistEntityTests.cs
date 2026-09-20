using _116.Content.Domain.Entities;
using _116.Content.Domain.Events;
using _116.Content.Domain.Exceptions;
using _116.Content.Domain.StateMachines;
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
        const string name = TestConstants.Artist.ValidName;
        const string slug = TestConstants.Artist.ValidSlug;
        const string bio = TestConstants.Artist.ValidBio;

        // Act
        ArtistEntity artist = ArtistEntity.Create(
            id,
            name,
            slug,
            bio,
            null,
            null,
            null,
            null,
            TestConstants.Clock.Today
        );

        // Assert
        artist.Id.Should().Be(id);
        artist.Name.Should().Be(name);
        artist.Slug.Value.Should().Be(slug);
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
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            null,
            null,
            null,
            null,
            null,
            TestConstants.Clock.Today
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
                TestConstants.Artist.ValidSlug,
                null,
                null,
                null,
                null,
                null,
                TestConstants.Clock.Today
            );

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistNameRequired);
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
                TestConstants.Artist.ValidName,
                invalidSlug!,
                null,
                null,
                null,
                null,
                null,
                TestConstants.Clock.Today
            );

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistSlugRequired);
    }

    #endregion

    #region Rename Tests

    [Fact]
    public void Rename_WithNewName_ShouldSetItAndReportChanged()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();

        // Act
        bool changed = artist.Rename("Updated Name");

        // Assert
        changed.Should().BeTrue();
        artist.Name.Should().Be("Updated Name");
    }

    [Fact]
    public void Rename_WithSameName_ShouldReportUnchangedAndRaiseNothing()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        artist.ClearDomainEvents();

        // Act
        bool changed = artist.Rename(artist.Name);

        // Assert
        changed.Should().BeFalse();
        artist.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Rename_ShouldNeverTouchSlug()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        string originalSlug = artist.Slug;

        // Act
        artist.Rename("Updated Name");

        // Assert
        artist.Slug.Value.Should().Be(originalSlug);
    }

    #endregion

    #region ReviseProfile Tests

    [Fact]
    public void ReviseProfile_WithNewBio_ShouldSetItAndReportChanged()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();

        // Act
        bool changed = artist.ReviseProfile("Updated Bio", null, null, null, null, TestConstants.Clock.Today);

        // Assert
        changed.Should().BeTrue();
        artist.Bio.Should().Be("Updated Bio");
    }

    [Fact]
    public void ReviseProfile_WithNullBio_ShouldClearBio()
    {
        // Arrange
        ArtistEntity artist = ArtistEntity.Create(
            Guid.NewGuid(),
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            TestConstants.Artist.ValidBio,
            null,
            null,
            null,
            null,
            TestConstants.Clock.Today
        );

        // Act
        artist.ReviseProfile(null, null, null, null, null, TestConstants.Clock.Today);

        // Assert
        artist.Bio.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Rename_WithEmptyName_ShouldThrowBadRequestException(string? invalidName)
    {
        // Arrange
        ArtistEntity artist = CreateArtist();

        // Act
        Action act = () => artist.Rename(invalidName!);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistNameRequired);
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

        // Act
        artist.ClaimOwnership(userId, TestConstants.Clock.Instant);

        // Assert
        artist.UserId.Should().Be(userId);
        artist.VerifiedAt.Should().NotBeNull();
        artist.VerifiedAt!.Value.Should().Be(TestConstants.Clock.Instant);
    }

    [Fact]
    public void ClaimOwnership_WhenUnclaimed_ShouldRaiseOwnershipVerifiedEvent()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        Guid userId = Guid.NewGuid();

        // Act
        artist.ClaimOwnership(userId, TestConstants.Clock.Instant);

        // Assert
        artist
            .DomainEvents.OfType<ArtistOwnershipVerifiedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ArtistOwnershipVerifiedEvent(artist.Id, userId));
    }

    [Fact]
    public void ClaimOwnership_WhenAlreadyClaimed_ShouldNotRaiseASecondOwnershipVerifiedEvent()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        artist.ClaimOwnership(Guid.NewGuid(), TestConstants.Clock.Instant);
        artist.ClearDomainEvents();

        // Act
        Action act = () => artist.ClaimOwnership(Guid.NewGuid(), TestConstants.Clock.Instant);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistAlreadyClaimed);
        artist.DomainEvents.OfType<ArtistOwnershipVerifiedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void ClaimOwnership_WhenAlreadyClaimed_ShouldThrowConflictException()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        artist.ClaimOwnership(Guid.NewGuid(), TestConstants.Clock.Instant);

        // Act
        Action act = () => artist.ClaimOwnership(Guid.NewGuid(), TestConstants.Clock.Instant);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistAlreadyClaimed);
    }

    [Fact]
    public void ClaimOwnership_WhenAlreadyClaimed_ShouldNotOverwriteOriginalOwner()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        Guid originalOwnerId = Guid.NewGuid();
        artist.ClaimOwnership(originalOwnerId, TestConstants.Clock.Instant);

        // Act
        Action act = () => artist.ClaimOwnership(Guid.NewGuid(), TestConstants.Clock.Instant);

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistAlreadyClaimed);
        artist.UserId.Should().Be(originalOwnerId);
    }

    #endregion

    #region Identity Field Tests

    [Fact]
    public void Create_WithIdentityFields_ShouldStoreThem()
    {
        // Arrange
        var birthdate = new DateOnly(1986, 10, 24);

        // Act
        ArtistEntity artist = ArtistEntity.Create(
            Guid.NewGuid(),
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            null,
            "Aubrey Drake Graham",
            ["Drizzy", "Champagne Papi"],
            birthdate,
            "Toronto, Canada",
            TestConstants.Clock.Today
        );

        // Assert
        artist.RealName.Should().Be("Aubrey Drake Graham");
        artist.Aliases.Should().Equal("Drizzy", "Champagne Papi");
        artist.Birthdate.Should().Be(birthdate);
        artist.Hometown.Should().Be("Toronto, Canada");
    }

    [Fact]
    public void Create_WithNullAliases_ShouldStoreEmptyList()
    {
        ArtistEntity artist = CreateArtist();

        artist.Aliases.Should().NotBeNull();
        artist.Aliases.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithBlankAndDuplicateAliases_ShouldNormaliseThem()
    {
        // Arrange
        ArtistEntity artist = ArtistEntity.Create(
            Guid.NewGuid(),
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            null,
            null,
            ["  Drizzy  ", "", "   ", "drizzy", "Champagne Papi"],
            null,
            null,
            TestConstants.Clock.Today
        );

        // Assert
        artist.Aliases.Should().Equal("Drizzy", "Champagne Papi");
    }

    [Fact]
    public void Create_WithTooManyAliases_ShouldThrowBadRequest()
    {
        // Arrange
        List<string> aliases = Enumerable.Range(0, 11).Select(i => $"Alias {i}").ToList();

        // Act
        Action act = () =>
            ArtistEntity.Create(
                Guid.NewGuid(),
                TestConstants.Artist.ValidName,
                TestConstants.Artist.ValidSlug,
                null,
                null,
                aliases,
                null,
                null,
                TestConstants.Clock.Today
            );

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistTooManyAliases);
    }

    [Fact]
    public void Create_WithOverlongAlias_ShouldThrowBadRequest()
    {
        // Act
        Action act = () =>
            ArtistEntity.Create(
                Guid.NewGuid(),
                TestConstants.Artist.ValidName,
                TestConstants.Artist.ValidSlug,
                null,
                null,
                [new string('a', 101)],
                null,
                null,
                TestConstants.Clock.Today
            );

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistAliasTooLong);
    }

    [Fact]
    public void Create_WithFutureBirthdate_ShouldThrowBadRequest()
    {
        // Arrange
        DateOnly future = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        // Act
        Action act = () =>
            ArtistEntity.Create(
                Guid.NewGuid(),
                TestConstants.Artist.ValidName,
                TestConstants.Artist.ValidSlug,
                null,
                null,
                null,
                future,
                null,
                TestConstants.Clock.Today
            );

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistBirthdateInFuture);
    }

    [Fact]
    public void ReviseProfile_WithNullIdentityFields_ShouldClearThem()
    {
        // Arrange
        ArtistEntity artist = ArtistEntity.Create(
            Guid.NewGuid(),
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            null,
            "Real Name",
            ["Alias"],
            new DateOnly(1990, 1, 1),
            "Kinshasa, RDC",
            TestConstants.Clock.Today
        );

        // Act
        artist.ReviseProfile(null, null, null, null, null, TestConstants.Clock.Today);

        // Assert
        artist.RealName.Should().BeNull();
        artist.Aliases.Should().BeEmpty();
        artist.Birthdate.Should().BeNull();
        artist.Hometown.Should().BeNull();
    }

    #endregion

    #region Name Folding Tests

    [Theory]
    [InlineData("Élodie", "ELODIE")]
    [InlineData("Ferré Gola", "FERRE GOLA")]
    [InlineData("  Fally   Ipupa  ", "FALLY IPUPA")]
    [InlineData("Ça Va", "CA VA")]
    public void FoldName_ShouldStripAccentsCollapseWhitespaceAndUppercase(string input, string expected)
    {
        ArtistEntity.FoldName(input).Should().Be(expected);
    }

    [Fact]
    public void FoldName_WithEmptyInput_ShouldReturnEmpty()
    {
        ArtistEntity.FoldName("   ").Should().BeEmpty();
    }

    [Theory]
    [InlineData("Élodie", "E")]
    [InlineData("fally ipupa", "F")]
    [InlineData("113 Crew", "#")]
    [InlineData("'Ndombolo", "#")]
    public void Create_ShouldDeriveInitialLetterFromFoldedName(string name, string expectedLetter)
    {
        // Act
        ArtistEntity artist = ArtistEntity.Create(
            Guid.NewGuid(),
            name,
            $"slug-{Guid.NewGuid():N}",
            null,
            null,
            null,
            null,
            null,
            TestConstants.Clock.Today
        );

        // Assert
        artist.InitialLetter.Should().Be(expectedLetter);
        artist.NameFolded.Should().Be(ArtistEntity.FoldName(name));
    }

    [Fact]
    public void Rename_ShouldRecomputeFoldedNameAndBucket()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();

        // Act
        artist.Rename("Élodie");

        // Assert — the artist moves bucket with the rename.
        artist.NameFolded.Should().Be("ELODIE");
        artist.InitialLetter.Should().Be("E");
    }

    #endregion

    private static ArtistEntity CreateArtist()
    {
        return ArtistEntity.Create(
            Guid.NewGuid(),
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            TestConstants.Artist.ValidBio,
            null,
            null,
            null,
            null,
            TestConstants.Clock.Today
        );
    }
}
