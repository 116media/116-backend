using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ShortVideoLikeEntity"/>.
/// </summary>
public class ShortVideoLikeEntityTests
{
    [Fact]
    public void Create_ShouldAssignFieldsAndRaisePositiveEngagementEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var shortVideoId = Guid.NewGuid();

        // Act
        ShortVideoLikeEntity row = ShortVideoLikeEntity.Create(id, userId, shortVideoId);

        // Assert
        row.Id.Should().Be(id);
        row.UserId.Should().Be(userId);
        row.ShortVideoId.Should().Be(shortVideoId);
        row.DomainEvents.OfType<ShortVideoEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ShortVideoEngagedEvent(shortVideoId, EnumEngagementKind.Like, 1));
    }

    [Fact]
    public void MarkRemoved_ShouldRaiseNegativeEngagementEvent()
    {
        // Arrange
        var shortVideoId = Guid.NewGuid();
        ShortVideoLikeEntity row = ShortVideoLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), shortVideoId);
        row.ClearDomainEvents();

        // Act
        row.MarkRemoved();

        // Assert
        row.DomainEvents.OfType<ShortVideoEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ShortVideoEngagedEvent(shortVideoId, EnumEngagementKind.Like, -1));
    }
}
