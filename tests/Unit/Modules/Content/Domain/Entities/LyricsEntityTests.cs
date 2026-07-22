using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsEntity"/>.
/// </summary>
public class LyricsEntityTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();
    private static readonly Guid AuthorId = Guid.NewGuid();

    #region CreateFree Tests

    [Fact]
    public void CreateFree_WithValidParams_ShouldCreateDraftLyrics()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string songTitle = TestConstants.Content.Editorial.Lyrics.ValidSongTitle;
        const string artistName = TestConstants.Content.Editorial.Lyrics.ValidArtistName;
        const string lyricsText = TestConstants.Content.Editorial.Lyrics.ValidLyricsText;
        const string language = TestConstants.Content.Editorial.Lyrics.ValidLanguage;
        const string slug = TestConstants.Content.Editorial.Lyrics.ValidSlug;

        // Act
        LyricsEntity lyrics = LyricsEntity.CreateFree(
            id,
            CategoryId,
            null,
            songTitle,
            artistName,
            lyricsText,
            language,
            slug,
            AuthorId,
            TestErrorsFactory.CreateLyricsErrors()
        );

        // Assert
        lyrics.Id.Should().Be(id);
        lyrics.CategoryId.Should().Be(CategoryId);
        lyrics.VideoId.Should().BeNull();
        lyrics.SongTitle.Should().Be(songTitle);
        lyrics.ArtistName.Should().Be(artistName);
        lyrics.LyricsText.Should().Be(lyricsText);
        lyrics.Language.Should().Be(language);
        lyrics.Slug.Should().Be(slug);
        lyrics.AuthorId.Should().Be(AuthorId);
        lyrics.Status.Should().Be(EnumContentStatus.Draft);
        lyrics.CustomerId.Should().BeNull();
        lyrics.OrderItemId.Should().BeNull();
    }

    [Fact]
    public void CreateFree_WithVideoId_ShouldLinkVideo()
    {
        // Arrange
        var videoId = Guid.NewGuid();

        // Act
        LyricsEntity lyrics = LyricsEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            videoId,
            TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            TestConstants.Content.Editorial.Lyrics.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateLyricsErrors()
        );

        // Assert
        lyrics.VideoId.Should().Be(videoId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFree_WithEmptySongTitle_ShouldThrowBadRequestException(string? invalidSongTitle)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreateFree(
                Guid.NewGuid(),
                CategoryId,
                null,
                invalidSongTitle!,
                TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                TestConstants.Content.Editorial.Lyrics.ValidSlug,
                AuthorId,
                TestErrorsFactory.CreateLyricsErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFree_WithEmptyArtistName_ShouldThrowBadRequestException(string? invalidArtistName)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreateFree(
                Guid.NewGuid(),
                CategoryId,
                null,
                TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                invalidArtistName!,
                TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                TestConstants.Content.Editorial.Lyrics.ValidSlug,
                AuthorId,
                TestErrorsFactory.CreateLyricsErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFree_WithEmptyLyricsText_ShouldThrowBadRequestException(string? invalidLyricsText)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreateFree(
                Guid.NewGuid(),
                CategoryId,
                null,
                TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                invalidLyricsText!,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                TestConstants.Content.Editorial.Lyrics.ValidSlug,
                AuthorId,
                TestErrorsFactory.CreateLyricsErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateFree_WithEmptySlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreateFree(
                Guid.NewGuid(),
                CategoryId,
                null,
                TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                invalidSlug!,
                AuthorId,
                TestErrorsFactory.CreateLyricsErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region CreatePaid Tests

    [Fact]
    public void CreatePaid_WithValidParams_ShouldSetCustomerAndOrderItem()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var orderItemId = Guid.NewGuid();

        // Act
        LyricsEntity lyrics = LyricsEntity.CreatePaid(
            Guid.NewGuid(),
            customerId,
            orderItemId,
            CategoryId,
            null,
            TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            TestConstants.Content.Editorial.Lyrics.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateLyricsErrors()
        );

        // Assert
        lyrics.CustomerId.Should().Be(customerId);
        lyrics.OrderItemId.Should().Be(orderItemId);
        lyrics.Status.Should().Be(EnumContentStatus.Draft);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreatePaid_WithEmptySongTitle_ShouldThrowBadRequestException(string? invalidSongTitle)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreatePaid(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CategoryId,
                null,
                invalidSongTitle!,
                TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                TestConstants.Content.Editorial.Lyrics.ValidSlug,
                AuthorId,
                TestErrorsFactory.CreateLyricsErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreatePaid_WithEmptyArtistName_ShouldThrowBadRequestException(string? invalidArtistName)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreatePaid(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CategoryId,
                null,
                TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                invalidArtistName!,
                TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                TestConstants.Content.Editorial.Lyrics.ValidSlug,
                AuthorId,
                TestErrorsFactory.CreateLyricsErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreatePaid_WithEmptyLyricsText_ShouldThrowBadRequestException(string? invalidLyricsText)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreatePaid(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CategoryId,
                null,
                TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                invalidLyricsText!,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                TestConstants.Content.Editorial.Lyrics.ValidSlug,
                AuthorId,
                TestErrorsFactory.CreateLyricsErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreatePaid_WithEmptySlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Act
        Action act = () =>
            LyricsEntity.CreatePaid(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                CategoryId,
                null,
                TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                invalidSlug!,
                AuthorId,
                TestErrorsFactory.CreateLyricsErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public void Submit_WhenDraft_ShouldTransitionToPendingPayment()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        bool result = lyrics.Submit();

        // Assert
        result.Should().BeTrue();
        lyrics.Status.Should().Be(EnumContentStatus.PendingPayment);
    }

    [Fact]
    public void Submit_WhenAlreadyPendingPayment_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.Submit();

        // Act
        bool result = lyrics.Submit();

        // Assert
        result.Should().BeFalse();
        lyrics.Status.Should().Be(EnumContentStatus.PendingPayment);
    }

    [Fact]
    public void MarkPendingReview_ShouldTransitionToPendingReview()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        bool result = lyrics.MarkPendingReview();

        // Assert
        result.Should().BeTrue();
        lyrics.Status.Should().Be(EnumContentStatus.PendingReview);
    }

    [Fact]
    public void MarkPendingReview_WhenAlreadyPendingReview_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.MarkPendingReview();

        // Act
        bool result = lyrics.MarkPendingReview();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Approve_ShouldTransitionToApproved()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.MarkPendingReview();

        // Act
        bool result = lyrics.Approve();

        // Assert
        result.Should().BeTrue();
        lyrics.Status.Should().Be(EnumContentStatus.Approved);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.MarkPendingReview();
        lyrics.Approve();

        // Act
        bool result = lyrics.Approve();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Publish_ShouldTransitionToPublished_AndSetPublishedAt()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.MarkPendingReview();
        lyrics.Approve();

        // Act
        bool result = lyrics.Publish();

        // Assert
        result.Should().BeTrue();
        lyrics.Status.Should().Be(EnumContentStatus.Published);
        lyrics.PublishedAt.Should().NotBeNull();
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.MarkPendingReview();
        lyrics.Approve();
        lyrics.Publish();

        // Act
        bool result = lyrics.Publish();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Reject_ShouldSetRejectionReason_AndTransitionToRejected()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        const string reason = TestConstants.Content.Editorial.Lyrics.ValidRejectionReason;

        // Act
        bool result = lyrics.Reject(reason);

        // Assert
        result.Should().BeTrue();
        lyrics.Status.Should().Be(EnumContentStatus.Rejected);
        lyrics.RejectionReason.Should().Be(reason);
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.Reject(TestConstants.Content.Editorial.Lyrics.ValidRejectionReason);

        // Act
        bool result = lyrics.Reject(TestConstants.Content.Editorial.Lyrics.ValidRejectionReason);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Archive_ShouldTransitionToArchived()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.MarkPendingReview();
        lyrics.Approve();
        lyrics.Publish();

        // Act
        bool result = lyrics.Archive();

        // Assert
        result.Should().BeTrue();
        lyrics.Status.Should().Be(EnumContentStatus.Archived);
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldReturnFalse()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.MarkPendingReview();
        lyrics.Approve();
        lyrics.Publish();
        lyrics.Archive();

        // Act
        bool result = lyrics.Archive();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Update Tests

    [Fact]
    public void UpdateSeo_ShouldSetMetaFieldsAndStructuredData()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        lyrics.UpdateSeo("My SEO Title", "My SEO Description", "{\"@type\":\"MusicRecording\"}");

        // Assert
        lyrics.MetaTitle.Should().Be("My SEO Title");
        lyrics.MetaDescription.Should().Be("My SEO Description");
        lyrics.StructuredData.Should().Be("{\"@type\":\"MusicRecording\"}");
    }

    [Fact]
    public void Update_ShouldUpdateAllFields()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        Guid newCategoryId = Guid.NewGuid();
        Guid newVideoId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        Guid orderItemId = Guid.NewGuid();

        // Act
        lyrics.Update(
            categoryId: newCategoryId,
            songTitle: "Updated Song Title",
            artistName: "Updated Artist",
            slug: "updated-slug",
            lyricsText: "Updated lyrics text",
            language: "en",
            videoId: newVideoId,
            customerId: customerId,
            orderItemId: orderItemId,
            errors: TestErrorsFactory.CreateLyricsErrors()
        );

        // Assert
        lyrics.CategoryId.Should().Be(newCategoryId);
        lyrics.SongTitle.Should().Be("Updated Song Title");
        lyrics.ArtistName.Should().Be("Updated Artist");
        lyrics.Slug.Should().Be("updated-slug");
        lyrics.LyricsText.Should().Be("Updated lyrics text");
        lyrics.Language.Should().Be("en");
        lyrics.VideoId.Should().Be(newVideoId);
        lyrics.CustomerId.Should().Be(customerId);
        lyrics.OrderItemId.Should().Be(orderItemId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptySlug_ShouldThrowBadRequestException(string? invalidSlug)
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        Action act = () =>
            lyrics.Update(
                categoryId: CategoryId,
                songTitle: TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
                artistName: TestConstants.Content.Editorial.Lyrics.ValidArtistName,
                slug: invalidSlug!,
                lyricsText: TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
                language: TestConstants.Content.Editorial.Lyrics.ValidLanguage,
                videoId: null,
                customerId: null,
                orderItemId: null,
                errors: TestErrorsFactory.CreateLyricsErrors()
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region UpdateMetadata Tests

    [Fact]
    public void UpdateMetadata_ShouldSetAllFieldsIndependently()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        lyrics.UpdateMetadata(
            album: "Le Grand Kalle Et L'African Jazz",
            releaseYear: 1960,
            label: "Fiesta",
            songwriter: "Joseph Kabasele",
            producer: "Henri Bowane"
        );

        // Assert
        lyrics.Album.Should().Be("Le Grand Kalle Et L'African Jazz");
        lyrics.ReleaseYear.Should().Be(1960);
        lyrics.Label.Should().Be("Fiesta");
        lyrics.Songwriter.Should().Be("Joseph Kabasele");
        lyrics.Producer.Should().Be("Henri Bowane");
    }

    [Fact]
    public void UpdateMetadata_WithNullAlbum_ShouldClearAlbumOnly_AndLeaveOthersUntouched()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.UpdateMetadata("Album", 1990, "Label", "Songwriter", "Producer");

        // Act
        lyrics.UpdateMetadata(null, 1990, "Label", "Songwriter", "Producer");

        // Assert
        lyrics.Album.Should().BeNull();
        lyrics.ReleaseYear.Should().Be(1990);
        lyrics.Label.Should().Be("Label");
        lyrics.Songwriter.Should().Be("Songwriter");
        lyrics.Producer.Should().Be("Producer");
    }

    [Fact]
    public void UpdateMetadata_WithNullReleaseYear_ShouldClearReleaseYearOnly_AndLeaveOthersUntouched()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.UpdateMetadata("Album", 1990, "Label", "Songwriter", "Producer");

        // Act
        lyrics.UpdateMetadata("Album", null, "Label", "Songwriter", "Producer");

        // Assert
        lyrics.Album.Should().Be("Album");
        lyrics.ReleaseYear.Should().BeNull();
        lyrics.Label.Should().Be("Label");
        lyrics.Songwriter.Should().Be("Songwriter");
        lyrics.Producer.Should().Be("Producer");
    }

    [Fact]
    public void UpdateMetadata_WithNullLabel_ShouldClearLabelOnly_AndLeaveOthersUntouched()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.UpdateMetadata("Album", 1990, "Label", "Songwriter", "Producer");

        // Act
        lyrics.UpdateMetadata("Album", 1990, null, "Songwriter", "Producer");

        // Assert
        lyrics.Album.Should().Be("Album");
        lyrics.ReleaseYear.Should().Be(1990);
        lyrics.Label.Should().BeNull();
        lyrics.Songwriter.Should().Be("Songwriter");
        lyrics.Producer.Should().Be("Producer");
    }

    [Fact]
    public void UpdateMetadata_WithNullSongwriter_ShouldClearSongwriterOnly_AndLeaveOthersUntouched()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.UpdateMetadata("Album", 1990, "Label", "Songwriter", "Producer");

        // Act
        lyrics.UpdateMetadata("Album", 1990, "Label", null, "Producer");

        // Assert
        lyrics.Album.Should().Be("Album");
        lyrics.ReleaseYear.Should().Be(1990);
        lyrics.Label.Should().Be("Label");
        lyrics.Songwriter.Should().BeNull();
        lyrics.Producer.Should().Be("Producer");
    }

    [Fact]
    public void UpdateMetadata_WithNullProducer_ShouldClearProducerOnly_AndLeaveOthersUntouched()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.UpdateMetadata("Album", 1990, "Label", "Songwriter", "Producer");

        // Act
        lyrics.UpdateMetadata("Album", 1990, "Label", "Songwriter", null);

        // Assert
        lyrics.Album.Should().Be("Album");
        lyrics.ReleaseYear.Should().Be(1990);
        lyrics.Label.Should().Be("Label");
        lyrics.Songwriter.Should().Be("Songwriter");
        lyrics.Producer.Should().BeNull();
    }

    #endregion

    #region SetCoverImageFileId Tests

    [Fact]
    public void SetCoverImageFileId_ShouldSetTheFileId()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        Guid fileId = Guid.NewGuid();

        // Act
        lyrics.SetCoverImageFileId(fileId);

        // Assert
        lyrics.CoverImageFileId.Should().Be(fileId);
    }

    [Fact]
    public void SetCoverImageFileId_WithNull_ShouldClearTheFileId()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.SetCoverImageFileId(Guid.NewGuid());

        // Act
        lyrics.SetCoverImageFileId(null);

        // Assert
        lyrics.CoverImageFileId.Should().BeNull();
    }

    #endregion

    #region LinkArtist / UnlinkArtist Tests

    [Fact]
    public void LinkArtist_ShouldSetArtistId()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        Guid artistId = Guid.NewGuid();

        // Act
        lyrics.LinkArtist(artistId);

        // Assert
        lyrics.ArtistId.Should().Be(artistId);
    }

    [Fact]
    public void LinkArtist_ShouldNotTouchArtistName()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        string originalArtistName = lyrics.ArtistName;

        // Act
        lyrics.LinkArtist(Guid.NewGuid());

        // Assert
        lyrics.ArtistName.Should().Be(originalArtistName);
    }

    [Fact]
    public void UnlinkArtist_ShouldClearArtistId()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.LinkArtist(Guid.NewGuid());

        // Act
        lyrics.UnlinkArtist();

        // Assert
        lyrics.ArtistId.Should().BeNull();
    }

    [Fact]
    public void UnlinkArtist_ShouldNotTouchArtistName()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.LinkArtist(Guid.NewGuid());
        string originalArtistName = lyrics.ArtistName;

        // Act
        lyrics.UnlinkArtist();

        // Assert
        lyrics.ArtistName.Should().Be(originalArtistName);
    }

    #endregion

    #region LinkAlbum / UnlinkAlbum Tests

    [Fact]
    public void LinkAlbum_ShouldSetAlbumId()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        Guid albumId = Guid.NewGuid();

        // Act
        lyrics.LinkAlbum(albumId);

        // Assert
        lyrics.AlbumId.Should().Be(albumId);
    }

    [Fact]
    public void LinkAlbum_ShouldNotTouchFreeTextAlbum()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.UpdateMetadata("Free Text Album", 1990, "Label", "Songwriter", "Producer");

        // Act
        lyrics.LinkAlbum(Guid.NewGuid());

        // Assert
        lyrics.Album.Should().Be("Free Text Album");
    }

    [Fact]
    public void UnlinkAlbum_ShouldClearAlbumId()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.LinkAlbum(Guid.NewGuid());

        // Act
        lyrics.UnlinkAlbum();

        // Assert
        lyrics.AlbumId.Should().BeNull();
    }

    [Fact]
    public void UnlinkAlbum_ShouldNotTouchFreeTextAlbum()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.UpdateMetadata("Free Text Album", 1990, "Label", "Songwriter", "Producer");
        lyrics.LinkAlbum(Guid.NewGuid());

        // Act
        lyrics.UnlinkAlbum();

        // Assert
        lyrics.Album.Should().Be("Free Text Album");
    }

    #endregion

    #region StampPromotion Tests

    [Fact]
    public void StampPromotion_ShouldSetIsPromotedAndPromotedUntil()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        Guid promotionLevelId = Guid.NewGuid();
        DateTimeOffset until = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        lyrics.StampPromotion(promotionLevelId, until);

        // Assert
        lyrics.IsPromoted.Should().BeTrue();
        lyrics.PromotedUntil.Should().Be(until);
    }

    [Fact]
    public void StampPromotion_ShouldNotSetAnyUnpromotedAuditField()
    {
        // Arrange — LyricsEntity has no SocialBoost concept (spec 12 scopes it to
        // IsPromoted/PromotedUntil only); this proves stamping promotion alone leaves the
        // force-unpromote audit trio untouched.
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        lyrics.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(30));

        // Assert
        lyrics.UnpromotedAt.Should().BeNull();
        lyrics.UnpromotedBy.Should().BeNull();
        lyrics.UnpromotedReason.Should().BeNull();
    }

    [Fact]
    public void StampPromotion_WhenCalledAgain_ShouldOverwritePromotedUntil()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));
        DateTimeOffset extendedUntil = DateTimeOffset.UtcNow.AddDays(30);

        // Act
        lyrics.StampPromotion(Guid.NewGuid(), extendedUntil);

        // Assert
        lyrics.IsPromoted.Should().BeTrue();
        lyrics.PromotedUntil.Should().Be(extendedUntil);
    }

    #endregion

    #region ForceUnpromote Tests

    [Fact]
    public void ForceUnpromote_WhenPromoted_ShouldClearFlagAndSetAuditTrio()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));
        const string unpromotedBy = "super-admin-user-id";
        const string reason = "Government takedown request.";

        // Act
        lyrics.ForceUnpromote(unpromotedBy, reason);

        // Assert
        lyrics.IsPromoted.Should().BeFalse();
        lyrics.PromotedUntil.Should().BeNull();
        lyrics.UnpromotedAt.Should().NotBeNull();
        lyrics.UnpromotedBy.Should().Be(unpromotedBy);
        lyrics.UnpromotedReason.Should().Be(reason);
    }

    [Fact]
    public void ForceUnpromote_WhenNotCurrentlyPromoted_ShouldThrowBadRequestException()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        Action act = () => lyrics.ForceUnpromote("super-admin-user-id", "Government takedown request.");

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region IncrementViewCount / IncrementLikeCount / DecrementLikeCount / IncrementShareCount Tests

    [Fact]
    public void IncrementViewCount_ShouldIncreaseViewCountByOne()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        lyrics.IncrementViewCount();

        // Assert
        lyrics.ViewCount.Should().Be(1);
    }

    [Fact]
    public void IncrementViewCount_WhenCalledMultipleTimes_ShouldAccumulate()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        lyrics.IncrementViewCount();
        lyrics.IncrementViewCount();
        lyrics.IncrementViewCount();

        // Assert
        lyrics.ViewCount.Should().Be(3);
    }

    [Fact]
    public void IncrementLikeCount_ShouldIncreaseLikeCountByOne()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        lyrics.IncrementLikeCount();

        // Assert
        lyrics.LikeCount.Should().Be(1);
    }

    [Fact]
    public void IncrementLikeCount_WhenCalledMultipleTimes_ShouldAccumulate()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        lyrics.IncrementLikeCount();
        lyrics.IncrementLikeCount();

        // Assert
        lyrics.LikeCount.Should().Be(2);
    }

    [Fact]
    public void DecrementLikeCount_WhenLikeCountIsPositive_ShouldDecreaseByOne()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        lyrics.IncrementLikeCount();
        lyrics.IncrementLikeCount();

        // Act
        lyrics.DecrementLikeCount();

        // Assert
        lyrics.LikeCount.Should().Be(1);
    }

    [Fact]
    public void DecrementLikeCount_WhenLikeCountIsZero_ShouldFloorAtZero()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        lyrics.DecrementLikeCount();

        // Assert
        lyrics.LikeCount.Should().Be(0);
    }

    [Fact]
    public void IncrementShareCount_ShouldIncreaseShareCountByOne()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        lyrics.IncrementShareCount();

        // Assert
        lyrics.ShareCount.Should().Be(1);
    }

    [Fact]
    public void IncrementShareCount_WhenCalledMultipleTimes_ShouldAccumulate()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();

        // Act
        lyrics.IncrementShareCount();
        lyrics.IncrementShareCount();
        lyrics.IncrementShareCount();

        // Assert
        lyrics.ShareCount.Should().Be(3);
    }

    #endregion

    #region ReplaceLyricsText Tests

    [Fact]
    public void ReplaceLyricsText_ShouldSetLyricsTextOnly()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        const string newLyricsText = "These are the corrected, community-accepted lyrics.";

        // Act
        lyrics.ReplaceLyricsText(newLyricsText);

        // Assert
        lyrics.LyricsText.Should().Be(newLyricsText);
    }

    [Fact]
    public void ReplaceLyricsText_ShouldNotTouchAnyOtherField()
    {
        // Arrange
        LyricsEntity lyrics = CreateFreeLyrics();
        Guid originalId = lyrics.Id;
        Guid originalCategoryId = lyrics.CategoryId;
        string originalSongTitle = lyrics.SongTitle;
        string originalArtistName = lyrics.ArtistName;
        string originalLanguage = lyrics.Language;
        string originalSlug = lyrics.Slug;
        Guid originalAuthorId = lyrics.AuthorId;
        EnumContentStatus originalStatus = lyrics.Status;

        // Act
        lyrics.ReplaceLyricsText("These are the corrected, community-accepted lyrics.");

        // Assert
        lyrics.Id.Should().Be(originalId);
        lyrics.CategoryId.Should().Be(originalCategoryId);
        lyrics.SongTitle.Should().Be(originalSongTitle);
        lyrics.ArtistName.Should().Be(originalArtistName);
        lyrics.Language.Should().Be(originalLanguage);
        lyrics.Slug.Should().Be(originalSlug);
        lyrics.AuthorId.Should().Be(originalAuthorId);
        lyrics.Status.Should().Be(originalStatus);
    }

    #endregion

    private static LyricsEntity CreateFreeLyrics()
    {
        return LyricsEntity.CreateFree(
            Guid.NewGuid(),
            CategoryId,
            null,
            TestConstants.Content.Editorial.Lyrics.ValidSongTitle,
            TestConstants.Content.Editorial.Lyrics.ValidArtistName,
            TestConstants.Content.Editorial.Lyrics.ValidLyricsText,
            TestConstants.Content.Editorial.Lyrics.ValidLanguage,
            TestConstants.Content.Editorial.Lyrics.ValidSlug,
            AuthorId,
            TestErrorsFactory.CreateLyricsErrors()
        );
    }
}
