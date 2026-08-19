using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="VideoRatingEntity"/>.
/// </summary>
public class VideoRatingEntityTests
{
    [Fact]
    public void Create_ShouldRaisePositiveRatingEngagementEvent()
    {
        // Arrange
        var videoId = Guid.NewGuid();

        // Act
        VideoRatingEntity rating = VideoRatingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), videoId, stars: 4);

        // Assert
        rating.Stars.Should().Be(4);
        rating
            .DomainEvents.OfType<VideoEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new VideoEngagedEvent(videoId, EnumEngagementKind.Rating, 1));
    }

    [Fact]
    public void UpdateStars_ShouldRaiseZeroDeltaRatingEngagementEvent()
    {
        // Arrange
        var videoId = Guid.NewGuid();
        VideoRatingEntity rating = VideoRatingEntity.Create(Guid.NewGuid(), Guid.NewGuid(), videoId, stars: 4);
        rating.ClearDomainEvents();

        // Act
        rating.UpdateStars(stars: 2);

        // Assert
        rating.Stars.Should().Be(2);
        rating
            .DomainEvents.OfType<VideoEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new VideoEngagedEvent(videoId, EnumEngagementKind.Rating, 0));
    }
}
