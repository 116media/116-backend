using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsLikeEntity"/>.
/// </summary>
public class LyricsLikeEntityTests
{
    [Fact]
    public void Create_WithValidParams_ShouldAssignAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lyricsId = Guid.NewGuid();

        // Act
        LyricsLikeEntity like = LyricsLikeEntity.Create(id, userId, lyricsId);

        // Assert
        like.Id.Should().Be(id);
        like.UserId.Should().Be(userId);
        like.LyricsId.Should().Be(lyricsId);
    }

    [Fact]
    public void Create_ShouldSetCreatedAt()
    {
        // Arrange
        DateTime before = DateTime.UtcNow;

        // Act
        LyricsLikeEntity like = LyricsLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        like.CreatedAt.Should().BeOnOrAfter(before);
        like.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Create_ShouldRaisePositiveLikeEngagementEvent()
    {
        // Arrange
        var lyricsId = Guid.NewGuid();

        // Act
        LyricsLikeEntity like = LyricsLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), lyricsId);

        // Assert
        like.DomainEvents.OfType<LyricsEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new LyricsEngagedEvent(lyricsId, EnumEngagementKind.Like, 1));
    }

    [Fact]
    public void MarkRemoved_ShouldRaiseNegativeLikeEngagementEvent()
    {
        // Arrange
        var lyricsId = Guid.NewGuid();
        LyricsLikeEntity like = LyricsLikeEntity.Create(Guid.NewGuid(), Guid.NewGuid(), lyricsId);
        like.ClearDomainEvents();

        // Act
        like.MarkRemoved();

        // Assert
        like.DomainEvents.OfType<LyricsEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new LyricsEngagedEvent(lyricsId, EnumEngagementKind.Like, -1));
    }
}
