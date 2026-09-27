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
