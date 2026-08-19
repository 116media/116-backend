using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="VideoShareEntity"/>.
/// </summary>
public class VideoShareEntityTests
{
    [Fact]
    public void Create_ShouldRaisePositiveShareEngagementEvent()
    {
        // Arrange
        var videoId = Guid.NewGuid();

        // Act
        VideoShareEntity share = VideoShareEntity.Create(Guid.NewGuid(), Guid.NewGuid(), videoId);

        // Assert
        share
            .DomainEvents.OfType<VideoEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new VideoEngagedEvent(videoId, EnumEngagementKind.Share, 1));
    }
}
