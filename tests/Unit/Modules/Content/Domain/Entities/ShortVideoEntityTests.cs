using _116.Content.Domain.Entities;
using _116.Shared.Application.Exceptions;
using _116.Tests.Fixtures.Constants;
using _116.Tests.Fixtures.Helpers;
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
        var videoFileId = Guid.NewGuid();
        const string title = TestConstants.Content.Editorial.ShortVideo.ValidTitle;
        const string slug = TestConstants.Content.Editorial.ShortVideo.ValidSlug;

        // Act
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            id,
            title,
            slug,
            videoFileId,
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
        );

        // Assert
        shortVideo.Id.Should().Be(id);
        shortVideo.Title.Should().Be(title);
        shortVideo.Slug.Should().Be(slug);
        shortVideo.VideoFileId.Should().Be(videoFileId);
        shortVideo.AuthorId.Should().Be(AuthorId);
        shortVideo.IsActive.Should().BeTrue();
        shortVideo.HasFullVideo.Should().BeFalse();
        shortVideo.VideoId.Should().BeNull();
        shortVideo.ThumbnailFileId.Should().BeNull();
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
                Guid.NewGuid(),
                AuthorId,
                TestErrorsFactory.CreateShortVideoErrors()
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
            Guid.NewGuid(),
            videoId,
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
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
                Guid.NewGuid(),
                Guid.NewGuid(),
                AuthorId,
                TestErrorsFactory.CreateShortVideoErrors()
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
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
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
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
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
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
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
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
        );

        // Act
        bool result = shortVideo.Activate();

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region SetThumbnailFileId Tests

    [Fact]
    public void SetThumbnailFileId_ShouldSetThumbnailFileId()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
        );
        var thumbnailFileId = Guid.NewGuid();

        // Act
        shortVideo.SetThumbnailFileId(thumbnailFileId);

        // Assert
        shortVideo.ThumbnailFileId.Should().Be(thumbnailFileId);
    }

    [Fact]
    public void SetThumbnailFileId_WithNull_ShouldClearThumbnailFileId()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
        );
        shortVideo.SetThumbnailFileId(Guid.NewGuid());

        // Act
        shortVideo.SetThumbnailFileId(null);

        // Assert
        shortVideo.ThumbnailFileId.Should().BeNull();
    }

    #endregion

    #region ReplaceVideoFile Tests

    [Fact]
    public void ReplaceVideoFile_ShouldUpdateVideoFileId()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
        );
        var newFileId = Guid.NewGuid();

        // Act
        shortVideo.ReplaceVideoFile(newFileId);

        // Assert
        shortVideo.VideoFileId.Should().Be(newFileId);
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
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
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
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
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
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
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
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
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
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
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
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
        );

        // Act
        shortVideo.IncrementBookmarkCount();

        // Assert
        shortVideo.BookmarkCount.Should().Be(1);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidParams_ShouldUpdateFields()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
        );

        // Act
        shortVideo.Update("New Title", null, TestErrorsFactory.CreateShortVideoErrors());

        // Assert
        shortVideo.Title.Should().Be("New Title");
        shortVideo.Slug.Should().Be(TestConstants.Content.Editorial.ShortVideo.ValidSlug);
        shortVideo.VideoId.Should().BeNull();
        shortVideo.HasFullVideo.Should().BeFalse();
    }

    [Fact]
    public void Update_WithVideoId_ShouldSetHasFullVideo()
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
        );
        Guid parentVideoId = Guid.NewGuid();

        // Act
        shortVideo.Update("Updated", parentVideoId, TestErrorsFactory.CreateShortVideoErrors());

        // Assert
        shortVideo.VideoId.Should().Be(parentVideoId);
        shortVideo.HasFullVideo.Should().BeTrue();
    }

    [Fact]
    public void Update_WithNullVideoId_ShouldRemoveParentLink()
    {
        // Arrange
        Guid parentVideoId = Guid.NewGuid();
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateTeaser(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            Guid.NewGuid(),
            parentVideoId,
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
        );

        // Act
        shortVideo.Update("Standalone Now", null, TestErrorsFactory.CreateShortVideoErrors());

        // Assert
        shortVideo.VideoId.Should().BeNull();
        shortVideo.HasFullVideo.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Update_WithEmptyTitle_ShouldThrowBadRequestException(string? invalidTitle)
    {
        // Arrange
        ShortVideoEntity shortVideo = ShortVideoEntity.CreateStandalone(
            Guid.NewGuid(),
            TestConstants.Content.Editorial.ShortVideo.ValidTitle,
            TestConstants.Content.Editorial.ShortVideo.ValidSlug,
            Guid.NewGuid(),
            AuthorId,
            TestErrorsFactory.CreateShortVideoErrors()
        );

        // Act
        Action act = () => shortVideo.Update(invalidTitle!, null, TestErrorsFactory.CreateShortVideoErrors());

        // Assert
        act.Should().Throw<BadRequestException>();
    }

    #endregion
}
