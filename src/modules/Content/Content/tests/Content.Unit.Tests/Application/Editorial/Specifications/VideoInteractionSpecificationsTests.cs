using _116.Content.Application.Editorial.Specifications;
using _116.Content.Domain.Entities;
using _116.Content.TestData.Factories;
using _116.Content.TestData.Factories.Helpers;
using _116.Content.TestData.Mocks.Factories;
using _116.Content.TestData.Mocks.Infrastructure;
using _116.Content.TestData.Mocks.Repositories;
using _116.Content.TestData.Mocks.Services;
using _116.Identity.TestData.Factories;
using _116.Identity.TestData.Mocks.Infrastructure;
using _116.Identity.TestData.Mocks.Repositories;
using _116.Identity.TestData.Mocks.Services;
using _116.Storage.TestData.Factories;
using _116.Storage.TestData.Mocks.Infrastructure;
using _116.Storage.TestData.Mocks.Services;
using _116.Tests.TestData.Mocks;
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
        VideoRatingEntity rating = VideoRatingEntity.Create(Guid.NewGuid(), userId: userId, videoId: videoId, stars: 4);
        var spec = new VideoRatingByUserAndVideoSpecification(userId, videoId);

        bool result = spec.IsSatisfiedBy(rating);

        result.Should().BeTrue();
    }

    [Fact]
    public void VideoRatingByUserAndVideoSpecification_WithDifferentUserId_ShouldReturnFalse()
    {
        VideoRatingEntity rating = VideoRatingEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            videoId: Guid.NewGuid(),
            stars: 4
        );
        var spec = new VideoRatingByUserAndVideoSpecification(Guid.NewGuid(), rating.VideoId);

        bool result = spec.IsSatisfiedBy(rating);

        result.Should().BeFalse();
    }

    [Fact]
    public void VideoRatingByUserAndVideoSpecification_WithDifferentVideoId_ShouldReturnFalse()
    {
        Guid userId = Guid.NewGuid();
        VideoRatingEntity rating = VideoRatingEntity.Create(
            Guid.NewGuid(),
            userId: userId,
            videoId: Guid.NewGuid(),
            stars: 4
        );
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
        VideoRatingEntity rating = VideoRatingEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            videoId: videoId,
            stars: 3
        );
        var spec = new VideoRatingByVideoIdSpecification(videoId);

        bool result = spec.IsSatisfiedBy(rating);

        result.Should().BeTrue();
    }

    [Fact]
    public void VideoRatingByVideoIdSpecification_WithDifferentVideoId_ShouldReturnFalse()
    {
        VideoRatingEntity rating = VideoRatingEntity.Create(
            Guid.NewGuid(),
            userId: Guid.NewGuid(),
            videoId: Guid.NewGuid(),
            stars: 3
        );
        var spec = new VideoRatingByVideoIdSpecification(Guid.NewGuid());

        bool result = spec.IsSatisfiedBy(rating);

        result.Should().BeFalse();
    }

    #endregion
}
