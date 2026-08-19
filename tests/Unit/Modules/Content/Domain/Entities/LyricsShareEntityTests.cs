using _116.Content.Domain.Entities;
using _116.Content.Domain.Enums;
using _116.Content.Domain.Events;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsShareEntity"/>.
/// </summary>
public class LyricsShareEntityTests
{
    [Fact]
    public void Create_WithAuthenticatedUserAndChannel_ShouldAssignAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var lyricsId = Guid.NewGuid();

        // Act
        LyricsShareEntity share = LyricsShareEntity.Create(id, userId, lyricsId, EnumShareChannel.WhatsApp);

        // Assert
        share.Id.Should().Be(id);
        share.UserId.Should().Be(userId);
        share.LyricsId.Should().Be(lyricsId);
        share.ShareChannel.Should().Be(EnumShareChannel.WhatsApp);
    }

    [Fact]
    public void Create_WithAnonymousUser_ShouldLeaveUserIdNull()
    {
        // Arrange
        var lyricsId = Guid.NewGuid();

        // Act
        LyricsShareEntity share = LyricsShareEntity.Create(Guid.NewGuid(), null, lyricsId);

        // Assert
        share.UserId.Should().BeNull();
        share.LyricsId.Should().Be(lyricsId);
    }

    [Fact]
    public void Create_WithoutShareChannel_ShouldLeaveShareChannelNull()
    {
        // Act
        LyricsShareEntity share = LyricsShareEntity.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        // Assert
        share.ShareChannel.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSetCreatedAt()
    {
        // Arrange
        DateTime before = DateTime.UtcNow;

        // Act
        LyricsShareEntity share = LyricsShareEntity.Create(Guid.NewGuid(), null, Guid.NewGuid());

        // Assert
        share.CreatedAt.Should().BeOnOrAfter(before);
        share.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }

    [Fact]
    public void Create_ShouldRaisePositiveShareEngagementEvent()
    {
        // Arrange
        var lyricsId = Guid.NewGuid();

        // Act
        LyricsShareEntity share = LyricsShareEntity.Create(Guid.NewGuid(), Guid.NewGuid(), lyricsId);

        // Assert
        share
            .DomainEvents.OfType<LyricsEngagedEvent>()
            .Should()
            .ContainSingle()
            .Which.Should()
            .Be(new LyricsEngagedEvent(lyricsId, EnumEngagementKind.Share, 1));
    }
}
