using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
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

namespace _116.Content.Unit.Tests.Domain.Entities;

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
