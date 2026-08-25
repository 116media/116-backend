using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Tests.Fixtures.Factories.Content;
using _116.Unit.Tests.Common.Helpers;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for video specification classes.
/// Specifications using EF.Functions.ILike are evaluated through
/// <see cref="ILikeSpecificationEvaluator" />, which rewrites ILike for in-memory execution.
/// </summary>
public class VideoSpecificationsTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    /// <summary>
    /// Attaches a tag to a video through the junction entity, populating the Tag
    /// navigation EF Core would load via Include.
    /// </summary>
    private static void AttachTag(VideoEntity video, TagEntity tag)
    {
        VideoTagEntity videoTag = VideoTagEntity.Create(Guid.NewGuid(), video.Id, tag.Id);
        typeof(VideoTagEntity).GetProperty(nameof(VideoTagEntity.Tag))!.SetValue(videoTag, tag);
        video.Tags.Add(videoTag);
    }

    #region VideoByIdSpecification

    [Fact]
    public void VideoByIdSpecification_WithMatchingId_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByIdSpecification(video.Id);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VideoByIdSpecification_WithDifferentId_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region VideoBySlugSpecification

    [Theory]
    [InlineData("116-le-focus-fally-ipupa", true)]
    [InlineData("116-LE-FOCUS-FALLY-IPUPA", true)]
    [InlineData("le-focus", false)]
    [InlineData("116-le-focus-koffi-olomide", false)]
    public void VideoBySlugSpecification_ShouldMatchWholeSlugCaseInsensitively(string slug, bool expected)
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateWithSlug(CategoryId, "116-le-focus-fally-ipupa");
        var spec = new VideoBySlugSpecification(slug);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(video);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region VideoByStatusSpecification

    [Fact]
    public void VideoByStatusSpecification_WithMatchingStatus_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByStatusSpecification(EnumContentStatus.Draft);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VideoByStatusSpecification_WithDifferentStatus_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByStatusSpecification(EnumContentStatus.Published);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region VideoByCategorySpecification

    [Fact]
    public void VideoByCategorySpecification_WithMatchingCategoryId_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByCategorySpecification(CategoryId);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VideoByCategorySpecification_WithDifferentCategoryId_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByCategorySpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region VideoSearchSpecification

    [Theory]
    [InlineData("fally", true)]
    [InlineData("116 LE FOCUS", true)]
    [InlineData("le focus — fally", true)]
    [InlineData("koffi", false)]
    public void VideoSearchSpecification_ShouldMatchTitleSubstringCaseInsensitively(string search, bool expected)
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateWithTitle(CategoryId, "116 Le Focus — Fally Ipupa");
        var spec = new VideoSearchSpecification(search);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(video);

        // Assert
        result.Should().Be(expected);
    }

    #endregion

    #region VideoByTagSlugSpecification

    [Theory]
    [InlineData("rumba", true)]
    [InlineData("RUMBA", true)]
    [InlineData("ndombolo", false)]
    public void VideoByTagSlugSpecification_ShouldMatchTagSlugCaseInsensitively(string tagSlug, bool expected)
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        AttachTag(video, TagFactory.Create("Rumba", "rumba"));
        var spec = new VideoByTagSlugSpecification(tagSlug);

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(video);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void VideoByTagSlugSpecification_WithUntaggedVideo_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new VideoByTagSlugSpecification("rumba");

        // Act
        bool result = spec.IsSatisfiedInMemoryBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region PromotedVideoSpecification

    [Fact]
    public void PromotedVideoSpecification_WithPromotedPublishedVideo_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePromoted(CategoryId);
        var spec = new PromotedVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void PromotedVideoSpecification_WithNonPromotedPublishedVideo_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        var spec = new PromotedVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void PromotedVideoSpecification_WithPromotedDraftVideo_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        video.StampPromotion(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(7));
        var spec = new PromotedVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region ActiveVideoSpecification

    [Fact]
    public void ActiveVideoSpecification_WithDraftVideo_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.Create(CategoryId);
        var spec = new ActiveVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ActiveVideoSpecification_WithPublishedVideo_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreatePublished(CategoryId);
        var spec = new ActiveVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ActiveVideoSpecification_WithApprovedVideo_ShouldReturnTrue()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateApproved(CategoryId);
        var spec = new ActiveVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ActiveVideoSpecification_WithRejectedVideo_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateRejected(CategoryId);
        var spec = new ActiveVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ActiveVideoSpecification_WithArchivedVideo_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateArchived(CategoryId);
        var spec = new ActiveVideoSpecification();

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region VideoByArtistSpecification

    [Fact]
    public void VideoByArtistSpecification_WithMatchingArtistId_ShouldReturnTrue()
    {
        // Arrange
        Guid artistId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreateForArtist(CategoryId, artistId);
        var spec = new VideoByArtistSpecification(artistId);

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VideoByArtistSpecification_WithDifferentArtistId_ShouldReturnFalse()
    {
        // Arrange
        VideoEntity video = VideoFactory.CreateForArtist(CategoryId, Guid.NewGuid());
        var spec = new VideoByArtistSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(video);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region VideoRatingByUserIdSpecification

    [Fact]
    public void VideoRatingByUserIdSpecification_WithMatchingUserId_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        VideoRatingEntity rating = VideoRatingFactory.Create(Guid.NewGuid(), userId);
        var spec = new VideoRatingByUserIdSpecification(userId);

        // Act
        bool result = spec.IsSatisfiedBy(rating);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VideoRatingByUserIdSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        // Arrange
        VideoRatingEntity rating = VideoRatingFactory.Create(Guid.NewGuid(), Guid.NewGuid());
        var spec = new VideoRatingByUserIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(rating);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region VideoShareByUserIdSpecification

    [Fact]
    public void VideoShareByUserIdSpecification_WithMatchingUserId_ShouldReturnTrue()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        VideoShareEntity share = VideoShareEntity.Create(Guid.NewGuid(), userId, Guid.NewGuid());
        var spec = new VideoShareByUserIdSpecification(userId);

        // Act
        bool result = spec.IsSatisfiedBy(share);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VideoShareByUserIdSpecification_WithAnonymousShare_ShouldReturnFalse()
    {
        // Arrange
        VideoShareEntity share = VideoShareEntity.Create(Guid.NewGuid(), userId: null, videoId: Guid.NewGuid());
        var spec = new VideoShareByUserIdSpecification(Guid.NewGuid());

        // Act
        bool result = spec.IsSatisfiedBy(share);

        // Assert
        result.Should().BeFalse();
    }

    #endregion
}
