using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using AwesomeAssertions;
using Xunit;

namespace _116.Content.Unit.Tests.Application.Editorial.Specifications;

/// <summary>
/// Unit tests for the new video interaction and sub-entity specification classes.
/// </summary>
public class VideoInteractionSpecificationsTests
{
    private static readonly Guid CategoryId = Guid.NewGuid();

    #region VideoByOrderItemIdSpecification

    [Fact]
    public void VideoByOrderItemIdSpecification_WithMatchingOrderItemId_ShouldReturnTrue()
    {
        Guid orderItemId = Guid.NewGuid();
        VideoEntity video = VideoFactory.CreatePaid(CategoryId, Guid.NewGuid(), orderItemId);
        var spec = new VideoByOrderItemIdSpecification(orderItemId);

        bool result = spec.IsSatisfiedBy(video);

        result.Should().BeTrue();
    }

    [Fact]
    public void VideoByOrderItemIdSpecification_WithDifferentOrderItemId_ShouldReturnFalse()
    {
        VideoEntity video = VideoFactory.CreatePaid(CategoryId, Guid.NewGuid(), Guid.NewGuid());
        var spec = new VideoByOrderItemIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(video);

        result.Should().BeFalse();
    }

    #endregion

    #region VideoRatingByUserAndVideoSpecification

    [Fact]
    public void VideoRatingByUserAndVideoSpecification_WithMatchingUserAndVideo_ShouldReturnTrue()
    {
        Guid userId = Guid.NewGuid();
        Guid videoId = Guid.NewGuid();
        VideoRatingEntity rating = VideoRatingFactory.Create(videoId, userId, 4);
        var spec = new VideoRatingByUserAndVideoSpecification(userId, videoId);

        bool result = spec.IsSatisfiedBy(rating);

        result.Should().BeTrue();
    }

    [Fact]
    public void VideoRatingByUserAndVideoSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        VideoRatingEntity rating = VideoRatingFactory.Create(Guid.NewGuid(), Guid.NewGuid(), 4);
        var spec = new VideoRatingByUserAndVideoSpecification(Guid.NewGuid(), rating.VideoId);

        bool result = spec.IsSatisfiedBy(rating);

        result.Should().BeFalse();
    }

    [Fact]
    public void VideoRatingByUserAndVideoSpecification_WithDifferentVideoId_ShouldReturnFalse()
    {
        Guid userId = Guid.NewGuid();
        VideoRatingEntity rating = VideoRatingFactory.Create(Guid.NewGuid(), userId, 4);
        var spec = new VideoRatingByUserAndVideoSpecification(userId, Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(rating);

        result.Should().BeFalse();
    }

    #endregion

    #region VideoRatingByVideoIdSpecification

    [Fact]
    public void VideoRatingByVideoIdSpecification_WithMatchingVideoId_ShouldReturnTrue()
    {
        Guid videoId = Guid.NewGuid();
        VideoRatingEntity rating = VideoRatingFactory.Create(videoId, Guid.NewGuid(), 3);
        var spec = new VideoRatingByVideoIdSpecification(videoId);

        bool result = spec.IsSatisfiedBy(rating);

        result.Should().BeTrue();
    }

    [Fact]
    public void VideoRatingByVideoIdSpecification_WithDifferentVideoId_ShouldReturnFalse()
    {
        VideoRatingEntity rating = VideoRatingFactory.Create(Guid.NewGuid(), Guid.NewGuid(), 3);
        var spec = new VideoRatingByVideoIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(rating);

        result.Should().BeFalse();
    }

    #endregion
}
