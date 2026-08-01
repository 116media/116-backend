using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="ShortVideoShareEntity"/>.
/// </summary>
public class ShortVideoShareEntityTests
{
    [Fact]
    public void Create_ShouldRaisePositiveShareEngagementEvent()
    {
        // Arrange
        var shortVideoId = Guid.NewGuid();

        // Act
        ShortVideoShareEntity share = ShortVideoShareEntity.Create(Guid.NewGuid(), Guid.NewGuid(), shortVideoId);

        // Assert
        share
            .DomainEvents.OfType<ShortVideoEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new ShortVideoEngagedEvent(shortVideoId, EnumEngagementKind.Share, 1));
    }
}
