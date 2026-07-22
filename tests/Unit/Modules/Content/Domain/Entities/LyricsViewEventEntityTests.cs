using _116.Content.Domain.Entities;
using AwesomeAssertions;
using Xunit;

namespace _116.Unit.Tests.Modules.Content.Domain.Entities;

/// <summary>
/// Unit tests for <see cref="LyricsViewEventEntity"/>.
/// </summary>
public class LyricsViewEventEntityTests
{
    [Fact]
    public void Create_WithValidParams_ShouldAssignAllFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var lyricsId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        const string dedupKey = "user:00000000-0000-0000-0000-000000000000";
        const string ipAddress = "203.0.113.5";
        const string userAgent = "Mozilla/5.0";

        // Act
        LyricsViewEventEntity viewEvent = LyricsViewEventEntity.Create(
            id,
            lyricsId,
            userId,
            dedupKey,
            ipAddress,
            userAgent,
            isCounted: true,
            dwellMs: 15_000,
            scrollDepthRatio: 0.95
        );

        // Assert
        viewEvent.Id.Should().Be(id);
        viewEvent.LyricsId.Should().Be(lyricsId);
        viewEvent.UserId.Should().Be(userId);
        viewEvent.DedupKey.Should().Be(dedupKey);
        viewEvent.IpAddress.Should().Be(ipAddress);
        viewEvent.UserAgent.Should().Be(userAgent);
        viewEvent.IsCounted.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldAssignDwellMsAndScrollDepthRatio()
    {
        // Act
        LyricsViewEventEntity viewEvent = LyricsViewEventEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "unknown",
            null,
            null,
            isCounted: false,
            dwellMs: 4_200,
            scrollDepthRatio: 0.42
        );

        // Assert
        viewEvent.DwellMs.Should().Be(4_200);
        viewEvent.ScrollDepthRatio.Should().Be(0.42);
        viewEvent.IsCounted.Should().BeFalse();
    }

    [Fact]
    public void Create_ForAnonymousViewer_ShouldLeaveUserIdNull()
    {
        // Act
        LyricsViewEventEntity viewEvent = LyricsViewEventEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "ip:203.0.113.5",
            "203.0.113.5",
            null,
            isCounted: true,
            dwellMs: 10_000,
            scrollDepthRatio: 1.0
        );

        // Assert
        viewEvent.UserId.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldSetCreatedAt()
    {
        // Arrange
        DateTime before = DateTime.UtcNow;

        // Act
        LyricsViewEventEntity viewEvent = LyricsViewEventEntity.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            null,
            "unknown",
            null,
            null,
            isCounted: true,
            dwellMs: 1_000,
            scrollDepthRatio: 0.8
        );

        // Assert
        viewEvent.CreatedAt.Should().BeOnOrAfter(before);
        viewEvent.CreatedAt.Should().BeOnOrBefore(DateTime.UtcNow);
    }
}
