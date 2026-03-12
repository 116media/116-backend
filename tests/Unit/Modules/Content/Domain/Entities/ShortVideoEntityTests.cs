using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ShortVideoEntity"/>.
/// </summary>
public class ShortVideoEntityTests
{
    private static readonly Guid AuthorId = Guid.NewGuid();

    #region CreateStandalone Tests

    [Fact]
    public void CreateStandalone_WithValidParams_ShouldCreateActiveShortVideo()
    {
        // Arrange
        var id = Guid.NewGuid();
        const string title = TestConstants.Content.Editorial.ShortVideo.ValidTitle;
        const string slug = TestConstants.Content.Editorial.ShortVideo.ValidSlug;
        const string videoUrl = TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl;
        const string storageKey = TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey;

        // Act
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            id,
            title,
            slug,
            videoUrl,
            storageKey,
            AuthorId
        );

        // Assert
        shortVideo.Id.Should().Be(id);
        shortVideo.Title.Should().Be(title);
        shortVideo.Slug.Should().Be(slug);
        shortVideo.VideoUrl.Should().Be(videoUrl);
        shortVideo.VideoStorageKey.Should().Be(storageKey);
        shortVideo.AuthorId.Should().Be(AuthorId);
        shortVideo.IsActive.Should().BeTrue();
        shortVideo.HasFullVideo.Should().BeFalse();
        shortVideo.VideoId.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateStandalone_WithEmptyTitle_ShouldThrowBadRequestException(string? invalidTitle)
    {
        // Act
        Action act = () =>
            ShortVideoEntity.CreateStandalone(
                Guid.NewGuid(),
                invalidTitle!,
                TestConstants.Content.Editorial.ShortVideo.ValidSlug,
                TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
                TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
                AuthorId
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region CreateTeaser Tests

    [Fact]
    public void CreateTeaser_ShouldSetVideoIdAndHasFullVideoTrue()
    {
        // Arrange
        var videoId = Guid.NewGuid();

        // Act
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateTeaser(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            videoId,
            AuthorId
        );

        // Assert
        shortVideo.VideoId.Should().Be(videoId);
        shortVideo.HasFullVideo.Should().BeTrue();
        shortVideo.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void CreateTeaser_WithEmptyTitle_ShouldThrowBadRequestException(string? invalidTitle)
    {
        // Act
        Action act = () =>
            ShortVideoEntity.CreateTeaser(
                Guid.NewGuid(),
                invalidTitle!,
                TestConstants.Content.Editorial.ShortVideo.ValidSlug,
                TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
                TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
                Guid.NewGuid(),
                AuthorId
            );

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion

    #region Activate / Deactivate Tests

    [Fact]
    public void Deactivate_WhenActive_ShouldReturnTrue()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );

        // Act
        bool result = shortVideo.Deactivate();

        // Assert
        result.Should().BeTrue();
        shortVideo.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFalse()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );
        shortVideo.Deactivate();

        // Act
        bool result = shortVideo.Deactivate();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldReturnTrue()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );
        shortVideo.Deactivate();

        // Act
        bool result = shortVideo.Activate();

        // Assert
        result.Should().BeTrue();
        shortVideo.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldReturnFalse()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );

        // Act
        bool result = shortVideo.Activate();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region UpdateThumbnail Tests

    [Fact]
    public void UpdateThumbnail_ShouldSetThumbnailUrlAndStorageKey()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );
        const string url = TestConstants.Content.Editorial.Cloudinary.ValidSecureUrl;
        const string storageKey = TestConstants.Content.Editorial.Cloudinary.ValidPublicId;

        // Act
        shortVideo.UpdateThumbnail(url, storageKey);

        // Assert
        shortVideo.ThumbnailUrl.Should().Be(url);
        shortVideo.ThumbnailStorageKey.Should().Be(storageKey);
    }

    #endregion

    #region Counter Tests

    [Fact]
    public void IncrementViewCount_ShouldIncrement()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );

        // Act
        shortVideo.IncrementViewCount();

        // Assert
        shortVideo.ViewCount.Should().Be(1);
    }

    [Fact]
    public void DecrementLikeCount_WhenAtZero_ShouldStayAtZero()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );

        // Act
        shortVideo.DecrementLikeCount();

        // Assert
        shortVideo.LikeCount.Should().Be(0);
    }

    [Fact]
    public void DecrementBookmarkCount_WhenAtZero_ShouldStayAtZero()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );

        // Act
        shortVideo.DecrementBookmarkCount();

        // Assert
        shortVideo.BookmarkCount.Should().Be(0);
    }

    [Fact]
    public void IncrementLikeCount_ShouldIncrementLikeCount()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );

        // Act
        shortVideo.IncrementLikeCount();

        // Assert
        shortVideo.LikeCount.Should().Be(1);
    }

    [Fact]
    public void IncrementShareCount_ShouldIncrementShareCount()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );

        // Act
        shortVideo.IncrementShareCount();

        // Assert
        shortVideo.ShareCount.Should().Be(1);
    }

    [Fact]
    public void IncrementBookmarkCount_ShouldIncrementBookmarkCount()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoUrl,
            TestConstants.Content.Editorial.ShortVideo.ValidVideoStorageKey,
            AuthorId
        );

        // Act
        shortVideo.IncrementBookmarkCount();

        // Assert
        shortVideo.BookmarkCount.Should().Be(1);
    }

    #endregion
}
