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
/// Unit tests for <see cref="ShortVideoViewEventEntity"/>.
/// </summary>
public class ShortVideoViewEventEntityTests
{
    [Fact]
    public void Create_WhenCounted_ShouldRaisePositiveViewEngagementEvent()
    {
        // Arrange
        var shortVideoId = Guid.NewGuid();

        // Act
        ShortVideoViewEventEntity viewEvent = ShortVideoViewEventEntity.Create(
            Guid.NewGuid(),
            shortVideoId,
            Guid.NewGuid(),
            "user:abc",
            null,
            null,
            isCounted: true
        );

        // Assert
        viewEvent.IsCounted.Should().BeTrue();
        viewEvent
            .DomainEvents.OfType<ShortVideoEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ShortVideoEngagedEvent(shortVideoId, EnumEngagementKind.View, 1));
    }

    [Fact]
    public void Create_WhenNotCounted_ShouldRaiseNothing()
    {
        // Act
        ShortVideoViewEventEntity viewEvent = ShortVideoViewEventEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "user:abc",
            null,
            null,
            isCounted: false
        );

        // Assert
        viewEvent.DomainEvents.Should().BeEmpty();
    }
}
