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
        ArtistEntity artist = ArtistEntity.Create(id, name, slug, bio, null, null, null, null);

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
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            null,
            null,
            null,
            null,
            null
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
                null
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
                null
            );

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistSlugRequired);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidParams_ShouldUpdateNameAndBio()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();

        // Act
        artist.Update("Updated Name", "Updated Bio", null, null, null, null);

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
        artist.Update("Updated Name", "Updated Bio", null, null, null, null);

        // Assert
        artist.Slug.Should().Be(originalSlug);
    }

    [Fact]
    public void Update_WithNullBio_ShouldClearBio()
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
            null
        );

        // Act
        artist.Update(artist.Name, null, null, null, null, null);

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
        Action act = () => artist.Update(invalidName!, "Bio", null, null, null, null);

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
        DateTimeOffset before = DateTimeOffset.UtcNow;

        // Act
        artist.ClaimOwnership(userId);

        // Assert
        artist.UserId.Should().Be(userId);
        artist.VerifiedAt.Should().NotBeNull();
        artist.VerifiedAt!.Value.Should().BeCloseTo(before, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ClaimOwnership_WhenUnclaimed_ShouldRaiseOwnershipVerifiedEvent()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        Guid userId = Guid.NewGuid();

        // Act
        artist.ClaimOwnership(userId);

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
        artist.ClaimOwnership(Guid.NewGuid());
        artist.ClearDomainEvents();

        // Act
        Action act = () => artist.ClaimOwnership(Guid.NewGuid());

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistAlreadyClaimed);
        artist.DomainEvents.OfType<ArtistOwnershipVerifiedEvent>().Should().BeEmpty();
    }

    [Fact]
    public void ClaimOwnership_WhenAlreadyClaimed_ShouldThrowConflictException()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        artist.ClaimOwnership(Guid.NewGuid());

        // Act
        Action act = () => artist.ClaimOwnership(Guid.NewGuid());

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistAlreadyClaimed);
    }

    [Fact]
    public void ClaimOwnership_WhenAlreadyClaimed_ShouldNotOverwriteOriginalOwner()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();
        Guid originalOwnerId = Guid.NewGuid();
        artist.ClaimOwnership(originalOwnerId);

        // Act
        Action act = () => artist.ClaimOwnership(Guid.NewGuid());

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
            "Toronto, Canada"
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
        // Arrange — blanks dropped, whitespace trimmed, case-insensitive dedupe keeps first casing.
        ArtistEntity artist = ArtistEntity.Create(
            Guid.NewGuid(),
            TestConstants.Artist.ValidName,
            TestConstants.Artist.ValidSlug,
            null,
            null,
            ["  Drizzy  ", "", "   ", "drizzy", "Champagne Papi"],
            null,
            null
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
                null
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
                null
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
                null
            );

        // Assert
        act.Should().Throw<ContentRuleException>().Which.Code.Should().Be(ContentRuleCodes.ArtistBirthdateInFuture);
    }

    [Fact]
    public void Update_WithNullIdentityFields_ShouldClearThem()
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
            "Kinshasa, RDC"
        );

        // Act
        artist.Update(artist.Name, null, null, null, null, null);

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
            null
        );

        // Assert
        artist.InitialLetter.Should().Be(expectedLetter);
        artist.NameFolded.Should().Be(ArtistEntity.FoldName(name));
    }

    [Fact]
    public void Update_WhenRenamed_ShouldRecomputeFoldedNameAndBucket()
    {
        // Arrange
        ArtistEntity artist = CreateArtist();

        // Act
        artist.Update("Élodie", null, null, null, null, null);

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
            null
        );
    }
}
